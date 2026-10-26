namespace Template.WindowsApp.Views.Settings;

using System.Runtime.InteropServices;

using Template.WindowsApp.Extender;
using Template.WindowsApp.Services;
using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class SettingsViewModel : AppViewModelBase, IShellCommandProvider
{
    private readonly IDialogService dialogService;

    private readonly NavigationMenu menu;

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

    public IObserveCommand ClearFavoritePagesCommand { get; }

    public IObserveCommand OpenLogsCommand { get; }

    public IObserveCommand CopyVersionCommand { get; }

    public SettingsViewModel(IWindowManager windowManager, IDialogService dialogService, IClipboardService clipboardService, NavigationMenu menu, NotificationService notifications)
    {
        this.dialogService = dialogService;
        this.menu = menu;
        WindowManager = windowManager;
        Notifications = notifications;

        var name = typeof(SettingsViewModel).Assembly.GetName();
        var version = name.Version?.ToString(3);
        ApplicationName = name.Name ?? string.Empty;
        VersionText = $"Version {version} · {RuntimeInformation.FrameworkDescription}";
        var versionInfo = $"""
            {ApplicationName}
            Version: {version}
            OS: {RuntimeInformation.OSDescription}
            .NET: {RuntimeInformation.FrameworkDescription}
            """;

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
        ClearRecentPagesCommand = MakeDelegateCommand(menu.ClearRecentPages);
        ClearFavoritePagesCommand = Observe(
            Observable.FromEventPattern(h => menu.Changed += h, h => menu.Changed -= h),
            MakeAsyncCommand(ClearFavoritePagesAsync, () => menu.Favorites.Items.Count > 0));
        OpenLogsCommand = MakeAsyncCommand(() => Navigator.ForwardAsync(ViewId.Logs, new NavigationParameter().WithEffect(EntranceEffect.Name)));
        CopyVersionCommand = MakeDelegateCommand(() => clipboardService.SetText(versionInfo));
    }

    private async Task ClearFavoritePagesAsync()
    {
        if (await dialogService.ConfirmAsync("Remove all favorites?", "The pages are removed from the navigation pane, Home and the jump list.", "Remove", "Cancel").ConfigureAwait(true))
        {
            menu.ClearFavorites();
        }
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("Settings:LightTheme", "Light theme", "Settings", "\uE790", ThemeCommand, AppTheme.Light),
        new("Settings:DarkTheme", "Dark theme", "Settings", "\uE790", ThemeCommand, AppTheme.Dark),
        new("Settings:SystemTheme", "System theme", "Settings", "\uE790", ThemeCommand, AppTheme.System),
        new("Settings:ClearRecentCommands", "Clear recent commands", "Settings", "\uE74D", ClearRecentCommand),
        new("Settings:ClearRecentPages", "Clear recent pages", "Settings", "\uE74D", ClearRecentPagesCommand),
        new("Settings:ClearFavoritePages", "Clear favorite pages", "Settings", "\uE74D", ClearFavoritePagesCommand),
        new("Settings:CopyVersionInfo", "Copy version info", "Settings", "\uE8C8", CopyVersionCommand)
    ];
}
