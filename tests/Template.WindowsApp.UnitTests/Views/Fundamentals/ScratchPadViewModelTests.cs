namespace Template.WindowsApp.Views.Fundamentals;

using NSubstitute;

using Template.WindowsApp.Services;

[Collection(ViewModelTestGroup.Name)]
public sealed class ScratchPadViewModelTests
{
    [Fact]
    public void LoadDrawsTheTypedText()
    {
        // Arrange
        using var viewModel = new ScratchPadViewModel(Substitute.For<IDialogService>());
        viewModel.OnActivated();
        viewModel.Text = "<Button Content=\"OK\" />";

        // Act
        viewModel.LoadCommand.Execute(null);

        // Assert
        Assert.Equal("<Button Content=\"OK\" />", viewModel.LoadedXaml);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ResetAsksFirst(bool accepted)
    {
        // Arrange
        var dialogService = Substitute.For<IDialogService>();
        dialogService.ConfirmAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(accepted);
        using var viewModel = new ScratchPadViewModel(dialogService);
        viewModel.OnActivated();
        var first = viewModel.Text;
        viewModel.Text = "<Button />";
        viewModel.LoadCommand.Execute(null);

        // Act
        viewModel.ResetCommand.Execute(null);

        // Assert
        var expected = accepted ? first : "<Button />";
        Assert.Equal(expected, viewModel.Text);
        Assert.Equal(expected, viewModel.LoadedXaml);
    }
}
