namespace Template.WindowsApp.Views.Layout;

using Template.WindowsApp.Models;

[Collection(ViewModelTestGroup.Name)]
public sealed class TabsViewModelTests
{
    [Fact]
    public void StartsWithThreeDocuments()
    {
        // Arrange
        using var viewModel = new TabsViewModel();

        // Assert
        Assert.Equal(["Document 1", "Document 2", "Document 3"], Titles(viewModel));
        Assert.Same(viewModel.Documents[0], viewModel.SelectedDocument);
    }

    [Fact]
    public void AddAppendsAndSelectsDocument()
    {
        // Arrange
        using var viewModel = new TabsViewModel();
        viewModel.OnActivated();

        // Act
        viewModel.AddCommand.Execute(null);

        // Assert
        Assert.Equal(["Document 1", "Document 2", "Document 3", "Document 4"], Titles(viewModel));
        Assert.Same(viewModel.Documents[^1], viewModel.SelectedDocument);
    }

    [Fact]
    public void CloseSelectedSelectsRightThenLeft()
    {
        // Arrange
        using var viewModel = new TabsViewModel();
        viewModel.OnActivated();
        viewModel.SelectedDocument = viewModel.Documents[1];

        // Act
        viewModel.CloseSelectedCommand.Execute(null);
        var afterMiddle = viewModel.SelectedDocument?.Title;
        viewModel.CloseSelectedCommand.Execute(null);
        var afterLast = viewModel.SelectedDocument?.Title;

        // Assert
        Assert.Equal("Document 3", afterMiddle);
        Assert.Equal("Document 1", afterLast);
        Assert.Equal(["Document 1"], Titles(viewModel));
    }

    [Fact]
    public void CloseOtherKeepsSelection()
    {
        // Arrange
        using var viewModel = new TabsViewModel();
        viewModel.OnActivated();

        // Act
        viewModel.CloseCommand.Execute(viewModel.Documents[2]);

        // Assert
        Assert.Equal(["Document 1", "Document 2"], Titles(viewModel));
        Assert.Same(viewModel.Documents[0], viewModel.SelectedDocument);
    }

    [Fact]
    public void CloseAllLeavesNoSelection()
    {
        // Arrange
        using var viewModel = new TabsViewModel();
        viewModel.OnActivated();

        // Act
        for (var i = 0; i < 3; i++)
        {
            viewModel.CloseSelectedCommand.Execute(null);
        }

        // Assert
        Assert.Empty(viewModel.Documents);
        Assert.Null(viewModel.SelectedDocument);
        Assert.False(viewModel.CloseSelectedCommand.CanExecute(null));
    }

    [Fact]
    public void SelectNumberSelectsNthOrLast()
    {
        // Arrange
        using var viewModel = new TabsViewModel();
        viewModel.OnActivated();
        viewModel.AddCommand.Execute(null);
        viewModel.AddCommand.Execute(null);

        // Act
        viewModel.SelectNumberCommand.Execute(2);
        var second = viewModel.SelectedDocument?.Title;
        viewModel.SelectNumberCommand.Execute(9);
        var last = viewModel.SelectedDocument?.Title;
        viewModel.SelectNumberCommand.Execute(1);
        viewModel.SelectNumberCommand.Execute(7);
        var outOfRange = viewModel.SelectedDocument?.Title;

        // Assert
        Assert.Equal("Document 2", second);
        Assert.Equal("Document 5", last);
        Assert.Equal("Document 1", outOfRange);
    }

    [Fact]
    public void MoveReordersDocuments()
    {
        // Arrange
        using var viewModel = new TabsViewModel();
        viewModel.OnActivated();

        // Act
        viewModel.MoveCommand.Execute(new ItemMove(0, 2));

        // Assert
        Assert.Equal(["Document 2", "Document 3", "Document 1"], Titles(viewModel));
        Assert.Same(viewModel.Documents[2], viewModel.SelectedDocument);
    }

    private static string[] Titles(TabsViewModel viewModel) =>
        viewModel.Documents.Select(static x => x.Title).ToArray();
}
