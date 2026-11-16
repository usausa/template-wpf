namespace Template.WindowsApp.Views.Fundamentals;

using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class ResourcesStylesViewModel : AppViewModelBase, IShellCommandProvider
{
    public IWindowManager WindowManager { get; }

    [ObservableProperty]
    public partial string? Pressed { get; set; }

    [ObservableProperty]
    public partial bool IsHighlighted { get; set; }

    public IObserveCommand SwitchThemeCommand { get; }

    public IObserveCommand PressCommand { get; }

    public ResourcesStylesViewModel(IWindowManager windowManager)
    {
        WindowManager = windowManager;

        SwitchThemeCommand = MakeDelegateCommand(() => windowManager.Theme = windowManager.Theme == AppTheme.Light ? AppTheme.Dark : AppTheme.Light);
        PressCommand = MakeDelegateCommand<string>(x => Pressed = x);
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("ResourcesStyles:SwitchTheme", "Switch theme", "Resources & styles", "\uE790", SwitchThemeCommand)
    ];
}
