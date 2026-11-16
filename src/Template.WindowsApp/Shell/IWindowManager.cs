namespace Template.WindowsApp.Shell;

using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;

using Template.WindowsApp.Components;
using Template.WindowsApp.Models;
using Template.WindowsApp.Settings;
using Template.WindowsApp.Views;
using Template.WindowsApp.Views.Platform;

public enum AppTheme
{
    Light,
    Dark,
    System
}

public enum StartPage
{
    Home,
    LastPage
}

public sealed class GridLayout
{
    public int FrozenColumnCount { get; init; }

    public IReadOnlyList<GridColumnLayout> Columns { get; init; } = [];
}

public sealed class GridColumnLayout
{
    public string Key { get; init; } = string.Empty;

    public bool IsVisible { get; init; } = true;

    public double? Width { get; init; }
}

public interface IWindowManager
{
    bool IsPaneOpen { get; set; }

    AppTheme Theme { get; set; }

    StartPage StartPage { get; set; }

    ViewId? LastView { get; set; }

    IList<string> RecentCommands { get; }

    IDictionary<string, int> CommandCounts { get; }

    IDictionary<string, GridLayout> GridLayouts { get; }

    event EventHandler? WindowsChanged;

    Window Load();

    void Save();

    int Count(WindowId id);

    bool Activate(WindowId id);

    void Show(WindowId id, object viewModel, bool owned = false);
}

public sealed class WindowManager : NotificationObject, IWindowManager
{
    private static readonly string CurrentVersion = typeof(WindowManager).Assembly.GetName().Version?.ToString(3) ?? string.Empty;

    private readonly IServiceProvider provider;

    private readonly NavigationMenu menu;

    private readonly NotificationCenter notificationCenter;

    private readonly WindowSetting setting = new();

    private readonly List<KeyValuePair<WindowId, Window>> windows = [];

    public bool IsPaneOpen
    {
        get;
        set => SetProperty(ref field, value);
    }

    public AppTheme Theme
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                ApplyTheme(value);
            }
        }
    }

    public StartPage StartPage
    {
        get;
        set => SetProperty(ref field, value);
    }

    public ViewId? LastView { get; set; }

    public IList<string> RecentCommands { get; } = [];

    public IDictionary<string, int> CommandCounts { get; } = new Dictionary<string, int>();

    public IDictionary<string, GridLayout> GridLayouts { get; } = new Dictionary<string, GridLayout>();

    public event EventHandler? WindowsChanged;

    public WindowManager(IServiceProvider provider, NavigationMenu menu, NotificationCenter notificationCenter)
    {
        this.provider = provider;
        this.menu = menu;
        this.notificationCenter = notificationCenter;
    }

    public Window Load()
    {
        if (setting.UpgradeRequired)
        {
            setting.Upgrade();
            setting.UpgradeRequired = false;
            setting.Save();
        }

        var mainWindow = provider.GetRequiredService<MainWindow>();

        if (setting.MainWindowPlacement is not null)
        {
            RestorePlacement(mainWindow, setting.MainWindowPlacement);
        }
        IsPaneOpen = setting.IsPaneOpen;
        Theme = Enum.TryParse<AppTheme>(setting.Theme, out var theme) ? theme : AppTheme.Light;
        StartPage = Enum.TryParse<StartPage>(setting.StartPage, out var startPage) ? startPage : StartPage.Home;
        LastView = Enum.TryParse<ViewId>(setting.LastView, out var lastView) && Enum.IsDefined(lastView) ? lastView : null;
        notificationCenter.ShowsPopups = setting.ShowsPopups;
        notificationCenter.ShowsInformational = setting.ShowsInformational;
        notificationCenter.ShowsSuccess = setting.ShowsSuccess;
        notificationCenter.ShowsWarning = setting.ShowsWarning;
        notificationCenter.ShowsError = setting.ShowsError;
        foreach (var id in (setting.RecentCommands ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            RecentCommands.Add(id);
        }
        foreach (var pair in (setting.CommandCounts ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = pair.LastIndexOf('=', StringComparison.Ordinal);
            if ((separator > 0) && Int32.TryParse(pair.AsSpan(separator + 1), NumberStyles.None, CultureInfo.InvariantCulture, out var count))
            {
                CommandCounts[pair[..separator]] = count;
            }
        }
        menu.Restore(
            (setting.FavoriteViews ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries),
            (setting.RecentViews ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries));
        RestoreGridLayouts(setting.GridLayouts);

        if (!String.IsNullOrEmpty(setting.LastVersion) && (setting.LastVersion != CurrentVersion))
        {
            notificationCenter.Show(InfoBarSeverity.Informational, "Updated", $"Version {CurrentVersion} is now installed.");
        }

        mainWindow.Show();

        return mainWindow;
    }

    private static void RestorePlacement(Window window, WindowPlacementSetting placement)
    {
        var left = SystemParameters.VirtualScreenLeft;
        var top = SystemParameters.VirtualScreenTop;
        var width = Math.Min(placement.Width, SystemParameters.VirtualScreenWidth);
        var height = Math.Min(placement.Height, SystemParameters.VirtualScreenHeight);

        window.Left = Math.Clamp(placement.Left, left, Math.Max(left, left + SystemParameters.VirtualScreenWidth - width));
        window.Top = Math.Clamp(placement.Top, top, Math.Max(top, top + SystemParameters.VirtualScreenHeight - height));
        window.Width = width;
        window.Height = height;
        if (placement.Maximized)
        {
            window.WindowState = WindowState.Maximized;
        }
    }

    private void RestoreGridLayouts(string? json)
    {
        if (String.IsNullOrEmpty(json))
        {
            return;
        }

        try
        {
            foreach (var pair in JsonSerializer.Deserialize<Dictionary<string, GridLayout>>(json) ?? [])
            {
                GridLayouts[pair.Key] = pair.Value;
            }
        }
        catch (JsonException)
        {
            GridLayouts.Clear();
        }
    }

    private static void ApplyTheme(AppTheme theme)
    {
        Application.Current.ThemeMode = theme switch
        {
            AppTheme.Dark => ThemeMode.Dark,
            AppTheme.System => ThemeMode.System,
            _ => ThemeMode.Light
        };
    }

    public void Save()
    {
        var mainWindow = provider.GetRequiredService<MainWindow>();

        setting.MainWindowPlacement = new WindowPlacementSetting
        {
            Left = (int)mainWindow.Left,
            Top = (int)mainWindow.Top,
            Width = (int)mainWindow.Width,
            Height = (int)mainWindow.Height,
            Maximized = mainWindow.WindowState == WindowState.Maximized
        };
        setting.IsPaneOpen = IsPaneOpen;
        setting.Theme = Theme.ToString();
        setting.StartPage = StartPage.ToString();
        setting.LastView = LastView?.ToString();
        setting.ShowsPopups = notificationCenter.ShowsPopups;
        setting.ShowsInformational = notificationCenter.ShowsInformational;
        setting.ShowsSuccess = notificationCenter.ShowsSuccess;
        setting.ShowsWarning = notificationCenter.ShowsWarning;
        setting.ShowsError = notificationCenter.ShowsError;
        setting.RecentCommands = string.Join(',', RecentCommands);
        setting.CommandCounts = string.Join(',', CommandCounts.Select(static x => String.Create(CultureInfo.InvariantCulture, $"{x.Key}={x.Value}")));
        setting.RecentViews = string.Join(',', menu.RecentPages.Select(static x => x.Id));
        setting.FavoriteViews = string.Join(',', menu.Favorites.Items.Select(static x => x.Id));
        setting.GridLayouts = JsonSerializer.Serialize(GridLayouts);
        setting.LastVersion = CurrentVersion;

        setting.Save();
    }

    public int Count(WindowId id) => windows.Count(x => x.Key == id);

    public bool Activate(WindowId id)
    {
        foreach (var (key, window) in windows)
        {
            if (key == id)
            {
                if (window.WindowState == WindowState.Minimized)
                {
                    SystemCommands.RestoreWindow(window);
                }
                window.Activate();
                return true;
            }
        }

        return false;
    }

    public void Show(WindowId id, object viewModel, bool owned = false)
    {
        var window = CreateWindow(id);
        window.DataContext = viewModel;
        if (owned)
        {
            window.Owner = provider.GetRequiredService<MainWindow>();
        }
        window.Closed += OnWindowClosed;
        windows.Add(new(id, window));
        window.Show();

        WindowsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnWindowClosed(object? sender, EventArgs e)
    {
        if (sender is not Window window)
        {
            return;
        }

        window.Closed -= OnWindowClosed;
        windows.RemoveAll(x => x.Value == window);

        WindowsChanged?.Invoke(this, EventArgs.Empty);
    }

    private static Window CreateWindow(WindowId id) =>
        id switch
        {
            WindowId.Sample or WindowId.Tool => new SampleWindow(),
            WindowId.Memory => new MemoryWindow(),
            _ => throw new ArgumentOutOfRangeException(nameof(id), id, null)
        };
}
