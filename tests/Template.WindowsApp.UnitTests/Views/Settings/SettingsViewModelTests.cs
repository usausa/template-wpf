namespace Template.WindowsApp.Views.Settings;

using System.Runtime.InteropServices;

using NSubstitute;

using Template.WindowsApp.Mocks;
using Template.WindowsApp.Services;
using Template.WindowsApp.Shell;

[Collection(ViewModelTestGroup.Name)]
public sealed class SettingsViewModelTests
{
    [Fact]
    public void CopyVersionWritesNameVersionOsAndRuntime()
    {
        // Arrange
        var clipboard = Substitute.For<IClipboardService>();
        var copied = string.Empty;
        clipboard.When(static x => x.SetText(Arg.Any<string>())).Do(x => copied = x.Arg<string>());
        using var viewModel = MakeViewModel(clipboard);
        viewModel.OnActivated();

        // Act
        viewModel.CopyVersionCommand.Execute(null);

        // Assert
        var name = typeof(SettingsViewModel).Assembly.GetName();
        Assert.Equal(
            [name.Name, $"Version: {name.Version?.ToString(3)}", $"OS: {RuntimeInformation.OSDescription}", $".NET: {RuntimeInformation.FrameworkDescription}"],
            copied.ReplaceLineEndings("\n").Split('\n'));
    }

    [Fact]
    public void PaletteHasCopyVersionInfo()
    {
        // Arrange
        using var viewModel = MakeViewModel(Substitute.For<IClipboardService>());

        // Act
        var command = viewModel.GetCommands().Single(static x => x.Id == "Settings:CopyVersionInfo");

        // Assert
        Assert.Equal("Copy version info", command.Title);
        Assert.Same(viewModel.CopyVersionCommand, command.Command);
    }

    private static SettingsViewModel MakeViewModel(IClipboardService clipboard) =>
        new(
            Substitute.For<IWindowManager>(),
            Substitute.For<IDialogService>(),
            clipboard,
            new NavigationMenu(),
            new NotificationService(new FixedTimeProvider(new DateTimeOffset(2026, 10, 26, 12, 0, 0, TimeSpan.Zero))));
}
