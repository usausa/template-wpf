namespace Template.WindowsApp.Shell;

using System.Windows.Input;

using NSubstitute;

using Template.WindowsApp.Models;

public sealed class CommandPaletteTests
{
    [Fact]
    public void OpenListsRecentCommandsFirst()
    {
        // Arrange
        var palette = new CommandPalette(["Settings"], new Dictionary<string, int>());

        // Act
        palette.Open([MakeCommand("Home", "Home"), MakeCommand("Settings", "Settings")]);

        // Assert
        Assert.True(palette.IsOpen);
        Assert.Equal(["Settings", "Home"], Titles(palette));
        Assert.Equal(0, palette.SelectedIndex);
    }

    [Fact]
    public void OpenSkipsCommandsThatCannotExecute()
    {
        // Arrange
        var palette = new CommandPalette([], new Dictionary<string, int>());

        // Act
        palette.Open([MakeCommand("Alpha", "Alpha"), MakeCommand("Beta", "Beta", canExecute: false)]);

        // Assert
        Assert.Equal(["Alpha"], Titles(palette));
    }

    [Fact]
    public void TextRanksTitleStartBeforeWordStartAndPart()
    {
        // Arrange
        var palette = new CommandPalette([], new Dictionary<string, int>());
        palette.Open([MakeCommand("DataGrid", "Data grid"), MakeCommand("Engrid", "Engrid"), MakeCommand("GridVisuals", "Grid visuals")]);

        // Act
        palette.Text = "grid";

        // Assert
        Assert.Equal(["Grid visuals", "Data grid", "Engrid"], Titles(palette));
        Assert.Equal([new HighlightRun("Grid", true), new HighlightRun(" visuals", false)], palette.Candidates[0].TitleRuns);
    }

    [Fact]
    public void TextMatchesCharactersInOrder()
    {
        // Arrange
        var palette = new CommandPalette([], new Dictionary<string, int>());
        palette.Open([MakeCommand("DataGrid", "Data grid"), MakeCommand("Home", "Home")]);

        // Act
        palette.Text = "dg";

        // Assert
        Assert.Equal(["Data grid"], Titles(palette));
        Assert.Equal(
            [new HighlightRun("D", true), new HighlightRun("ata ", false), new HighlightRun("g", true), new HighlightRun("rid", false)],
            palette.Candidates[0].TitleRuns);
    }

    [Fact]
    public void TextNeedsEveryWord()
    {
        // Arrange
        var palette = new CommandPalette([], new Dictionary<string, int>());
        palette.Open([MakeCommand("DataGrid", "Data grid")]);

        // Act
        palette.Text = "data xyz";

        // Assert
        Assert.Empty(palette.Candidates);
        Assert.Equal(-1, palette.SelectedIndex);
    }

    [Fact]
    public void TextMatchesCategoryWithoutHighlight()
    {
        // Arrange
        var palette = new CommandPalette([], new Dictionary<string, int>());
        palette.Open([MakeCommand("Logs:Clear", "Clear", "Logs")]);

        // Act
        palette.Text = "logs";

        // Assert
        Assert.Equal(["Clear"], Titles(palette));
        Assert.Equal([new HighlightRun("Clear", false)], palette.Candidates[0].TitleRuns);
    }

    [Fact]
    public void TextRanksOftenUsedCommandsFirst()
    {
        // Arrange
        var palette = new CommandPalette([], new Dictionary<string, int> { ["OpenFolder"] = 1 });
        palette.Open([MakeCommand("OpenFile", "Open file"), MakeCommand("OpenFolder", "Open folder")]);

        // Act
        palette.Text = "open";

        // Assert
        Assert.Equal(["Open folder", "Open file"], Titles(palette));
    }

    [Fact]
    public void TextRanksRecentCommandsFirstWhenScoresAreEqual()
    {
        // Arrange
        var palette = new CommandPalette(["OpenFolder"], new Dictionary<string, int>());
        palette.Open([MakeCommand("OpenFile", "Open file"), MakeCommand("OpenFolder", "Open folder")]);

        // Act
        palette.Text = "open";

        // Assert
        Assert.Equal(["Open folder", "Open file"], Titles(palette));
    }

    [Fact]
    public void MoveWrapsAround()
    {
        // Arrange
        var palette = new CommandPalette([], new Dictionary<string, int>());
        palette.Open([MakeCommand("A", "A"), MakeCommand("B", "B"), MakeCommand("C", "C")]);

        // Act
        palette.Move(CommandPaletteMove.Previous);
        var last = palette.SelectedIndex;
        palette.Move(CommandPaletteMove.Next);

        // Assert
        Assert.Equal(2, last);
        Assert.Equal(0, palette.SelectedIndex);
    }

    [Fact]
    public void PickUpdatesRecentCommandsAndCounts()
    {
        // Arrange
        var recent = new List<string> { "A" };
        var counts = new Dictionary<string, int>();
        var palette = new CommandPalette(recent, counts);
        palette.Open([MakeCommand("A", "Alpha"), MakeCommand("B", "Beta")]);

        // Act
        var picked = palette.Pick(palette.Candidates[1]);

        // Assert
        Assert.Equal("B", picked?.Id);
        Assert.False(palette.IsOpen);
        Assert.Equal(["B", "A"], recent);
        Assert.Equal(1, counts["B"]);
    }

    [Fact]
    public void PickKeepsTenRecentCommands()
    {
        // Arrange
        var recent = Enumerable.Range(1, 10).Select(static x => $"C{x}").ToList();
        var palette = new CommandPalette(recent, new Dictionary<string, int>());
        palette.Open([MakeCommand("New", "New")]);

        // Act
        palette.Pick(null);

        // Assert
        Assert.Equal(10, recent.Count);
        Assert.Equal("New", recent[0]);
        Assert.DoesNotContain("C10", recent);
    }

    [Fact]
    public void PickWithoutCandidatesReturnsNull()
    {
        // Arrange
        var palette = new CommandPalette([], new Dictionary<string, int>());
        palette.Open([]);

        // Act
        var picked = palette.Pick(null);

        // Assert
        Assert.Null(picked);
    }

    private static ShellCommand MakeCommand(string id, string title, string category = "View", bool canExecute = true)
    {
        var command = Substitute.For<ICommand>();
        command.CanExecute(Arg.Any<object?>()).Returns(canExecute);
        return new ShellCommand(id, title, category, string.Empty, command);
    }

    private static string[] Titles(CommandPalette palette) => [.. palette.Candidates.Select(static x => x.Command.Title)];
}
