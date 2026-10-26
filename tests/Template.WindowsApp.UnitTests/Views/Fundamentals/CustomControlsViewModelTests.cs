namespace Template.WindowsApp.Views.Fundamentals;

[Collection(ViewModelTestGroup.Name)]
public sealed class CustomControlsViewModelTests
{
    [Fact]
    public void RemoveTagAndResetTags()
    {
        // Arrange
        using var viewModel = new CustomControlsViewModel();
        viewModel.OnActivated();

        // Act
        viewModel.RemoveTagCommand.Execute("WPF");
        var removed = viewModel.Tags.ToArray();
        viewModel.ResetTagsCommand.Execute(null);

        // Assert
        Assert.Equal(["XAML", "MVVM", "Binding", "Styles"], removed);
        Assert.Equal(["XAML", "WPF", "MVVM", "Binding", "Styles"], viewModel.Tags);
    }

    [Fact]
    public void EnterCountsEachTime()
    {
        // Arrange
        using var viewModel = new CustomControlsViewModel();
        viewModel.OnActivated();

        // Act
        viewModel.EnterCommand.Execute(null);
        viewModel.EnterCommand.Execute(null);

        // Assert
        Assert.Equal(2, viewModel.EnterCount);
    }
}
