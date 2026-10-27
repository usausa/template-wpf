namespace Template.WindowsApp.Views.Fundamentals;

using NSubstitute;

using Template.WindowsApp.Settings;
using Template.WindowsApp.Shell;

[Collection(ViewModelTestGroup.Name)]
public sealed class ResourcesStylesViewModelTests
{
    [Theory]
    [InlineData(AppTheme.Light, AppTheme.Dark)]
    [InlineData(AppTheme.Dark, AppTheme.Light)]
    [InlineData(AppTheme.System, AppTheme.Light)]
    public void SwitchThemeTurnsLightAndDark(AppTheme current, AppTheme expected)
    {
        // Arrange
        var windowManager = Substitute.For<IWindowManager>();
        windowManager.Theme = current;
        using var viewModel = new ResourcesStylesViewModel(windowManager);
        viewModel.OnActivated();

        // Act
        viewModel.SwitchThemeCommand.Execute(null);

        // Assert
        Assert.Equal(expected, windowManager.Theme);
    }

    [Fact]
    public void PressKeepsTheButtonName()
    {
        // Arrange
        using var viewModel = new ResourcesStylesViewModel(Substitute.For<IWindowManager>());
        viewModel.OnActivated();

        // Act
        viewModel.PressCommand.Execute("Styled");

        // Assert
        Assert.Equal("Styled", viewModel.Pressed);
    }
}
