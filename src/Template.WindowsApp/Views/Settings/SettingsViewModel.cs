namespace Template.WindowsApp.Views.Settings;

using System.Runtime.InteropServices;

using Template.WindowsApp.Extender;
using Template.WindowsApp.Services;
using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class SettingsViewModel : AppViewModelBase, IShellCommandProvider
{
    private SettingsPage lastPage = SettingsPage.General;

    public IWindowManager WindowManager { get; }

    public NotificationService Notifications { get; }

    public IReadOnlyList<AppTheme> Themes { get; } = [AppTheme.Light, AppTheme.Dark, AppTheme.System];

    public string ApplicationName { get; }

    public string VersionText { get; }

    [ObservableProperty]
    public partial SettingsPage? SelectedPage { get; set; }

    [ObservableProperty]
    public partial string SearchText { get; set; }

    public EventRequest FindRequest { get; } = new();

    public IObserveCommand FindCommand { get; }

    public IObserveCommand ClearSearchCommand { get; }

    public IObserveCommand ThemeCommand { get; }

    public IObserveCommand ClearRecentCommand { get; }

    public IObserveCommand ClearRecentPagesCommand { get; }

    public IObserveCommand OpenLogsCommand { get; }

    public SettingsViewModel(IWindowManager windowManager, NotificationService notifications, TaskbarService taskbar)
    {
        WindowManager = windowManager;
        Notifications = notifications;

        var name = typeof(SettingsViewModel).Assembly.GetName();
        ApplicationName = name.Name ?? string.Empty;
        VersionText = $"Version {name.Version?.ToString(3)} · {RuntimeInformation.FrameworkDescription}";

        SelectedPage = SettingsPage.General;
        SearchText = string.Empty;

        SubscribeSearchText(x =>
        {
            if (String.IsNullOrWhiteSpace(x))
            {
                SelectedPage ??= lastPage;
            }
            else if (SelectedPage is { } page)
            {
                lastPage = page;
                SelectedPage = null;
            }
        });
        SubscribeSelectedPage(x =>
        {
            if (x is not null)
            {
                SearchText = string.Empty;
            }
        });

        FindCommand = MakeDelegateCommand(FindRequest.Request);
        ClearSearchCommand = MakeDelegateCommand(() => SearchText = string.Empty, () => SearchText.Length > 0);
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
        OpenLogsCommand = MakeAsyncCommand(() => Navigator.ForwardAsync(ViewId.Logs, new NavigationParameter().WithEffect(EntranceEffect.Name)));
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
