namespace Template.WindowsApp.Views.Settings;

using System.Runtime.InteropServices;

using Template.WindowsApp.Services;
using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class SettingsViewModel : AppViewModelBase, IShellCommandProvider
{
    public IWindowManager WindowManager { get; }

    public IReadOnlyList<AppTheme> Themes { get; } = [AppTheme.Light, AppTheme.Dark, AppTheme.System];

    public string ApplicationName { get; }

    public string VersionText { get; }

    public IObserveCommand ThemeCommand { get; }

    public IObserveCommand ClearRecentCommand { get; }

    public IObserveCommand ClearRecentPagesCommand { get; }

    public SettingsViewModel(IWindowManager windowManager, TaskbarService taskbar)
    {
        WindowManager = windowManager;

        var name = typeof(SettingsViewModel).Assembly.GetName();
        ApplicationName = name.Name ?? string.Empty;
        VersionText = $"Version {name.Version?.ToString(3)} · {RuntimeInformation.FrameworkDescription}";

        ThemeCommand = MakeDelegateCommand<AppTheme>(x => windowManager.Theme = x);
        ClearRecentCommand = MakeDelegateCommand(() =>
        {
            windowManager.RecentCommands.Clear();
            windowManager.CommandCounts.Clear();
        });
        ClearRecentPagesCommand = MakeDelegateCommand(() =>
        {
            windowManager.RecentViews.Clear();
            taskbar.SetRecentViews([]);
        });
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("Settings:LightTheme", "Light theme", "Settings", "\uE790", ThemeCommand, AppTheme.Light),
        new("Settings:DarkTheme", "Dark theme", "Settings", "\uE790", ThemeCommand, AppTheme.Dark),
        new("Settings:SystemTheme", "System theme", "Settings", "\uE790", ThemeCommand, AppTheme.System),
        new("Settings:ClearRecentCommands", "Clear recent commands", "Settings", "\uE74D", ClearRecentCommand),
        new("Settings:ClearRecentPages", "Clear recent pages", "Settings", "\uE74D", ClearRecentPagesCommand)
    ];
}
