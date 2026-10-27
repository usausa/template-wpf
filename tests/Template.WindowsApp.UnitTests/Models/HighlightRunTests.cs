namespace Template.WindowsApp.Models;

public sealed class HighlightRunTests
{
    [Fact]
    public void SplitGroupsMarkedCharacters()
    {
        // Arrange
        var marks = new[] { true, false, false, false, false, true, true, true, true };

        // Act
        var runs = HighlightRun.Split("Data grid", marks);

        // Assert
        Assert.Equal([new HighlightRun("D", true), new HighlightRun("ata ", false), new HighlightRun("grid", true)], runs);
    }

    [Fact]
    public void SplitWithoutMarksReturnsWholeText()
    {
        // Act
        var runs = HighlightRun.Split("Home", new bool[4]);

        // Assert
        Assert.Equal([new HighlightRun("Home", false)], runs);
    }

    [Fact]
    public void SplitWithAllMarksReturnsOneHighlightedRun()
    {
        // Act
        var runs = HighlightRun.Split("Logs", [true, true, true, true]);

        // Assert
        Assert.Equal([new HighlightRun("Logs", true)], runs);
    }

    [Fact]
    public void SplitEmptyTextReturnsNoRuns()
    {
        // Act
        var runs = HighlightRun.Split(string.Empty, []);

        // Assert
        Assert.Empty(runs);
    }
}
