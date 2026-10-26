namespace Template.WindowsApp.Views.Fundamentals;

[Collection(ViewModelTestGroup.Name)]
public sealed class TemplatesViewModelTests
{
    [Fact]
    public void PressCountsAndResetClears()
    {
        // Arrange
        using var viewModel = new TemplatesViewModel();
        viewModel.OnActivated();
        viewModel.SelectedSwatch = viewModel.Swatches[3];
        viewModel.SelectedTag = null;

        // Act
        viewModel.PressCommand.Execute(null);
        viewModel.PressCommand.Execute(null);
        var pressed = viewModel.PressCount;
        viewModel.ResetCommand.Execute(null);

        // Assert
        Assert.Equal(2, pressed);
        Assert.Equal(0, viewModel.PressCount);
        Assert.Same(viewModel.Swatches[0], viewModel.SelectedSwatch);
        Assert.Equal("Binding", viewModel.SelectedTag);
    }
}
