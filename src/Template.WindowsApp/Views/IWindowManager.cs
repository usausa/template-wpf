namespace Template.WindowsApp.Views;

using Microsoft.Extensions.DependencyInjection;

using Template.WindowsApp.Controls;
using Template.WindowsApp.Services;
using Template.WindowsApp.Settings;

public interface IWindowManager
{
    bool IsPaneOpen { get; set; }

    AppTheme Theme { get; set; }

    IList<string> RecentCommands { get; }

    IDictionary<string, int> CommandCounts { get; }

    Window Load();

    void Save();
}

public sealed class WindowManager : NotificationObject, IWindowManager
{
    private static readonly string CurrentVersion = typeof(WindowManager).Assembly.GetName().Version?.ToString(3) ?? string.Empty;

    private readonly IServiceProvider provider;

    private readonly NotificationService notifications;

    private readonly WindowSettings settings = new();

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

    public IList<string> RecentCommands { get; } = [];

    public IDictionary<string, int> CommandCounts { get; } = new Dictionary<string, int>();

    public WindowManager(IServiceProvider provider, NotificationService notifications)
    {
        this.provider = provider;
        this.notifications = notifications;
    }

    public Window Load()
    {
        if (settings.UpgradeRequired)
        {
            settings.Upgrade();
            settings.UpgradeRequired = false;
            settings.Save();
        }

        var mainWindow = provider.GetRequiredService<MainWindow>();

        if (settings.MainWindowPlacement is not null)
        {
            RestorePlacement(mainWindow, settings.MainWindowPlacement);
        }
        IsPaneOpen = settings.IsPaneOpen;
        Theme = Enum.TryParse<AppTheme>(settings.Theme, out var theme) ? theme : AppTheme.Light;
        foreach (var id in (settings.RecentCommands ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            RecentCommands.Add(id);
        }
        foreach (var pair in (settings.CommandCounts ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = pair.LastIndexOf('=');
            if ((separator > 0) && Int32.TryParse(pair.AsSpan(separator + 1), NumberStyles.None, CultureInfo.InvariantCulture, out var count))
            {
                CommandCounts[pair[..separator]] = count;
            }
        }

        if (!String.IsNullOrEmpty(settings.LastVersion) && (settings.LastVersion != CurrentVersion))
        {
            notifications.Show(InfoBarSeverity.Informational, "Updated", $"Version {CurrentVersion} is now installed.");
        }

        mainWindow.Show();

        return mainWindow;
    }

    private static void RestorePlacement(Window window, MainWindowPlacement placement)
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

        settings.MainWindowPlacement = new MainWindowPlacement
        {
            Left = (int)mainWindow.Left,
            Top = (int)mainWindow.Top,
            Width = (int)mainWindow.Width,
            Height = (int)mainWindow.Height,
            Maximized = mainWindow.WindowState == WindowState.Maximized
        };
        settings.IsPaneOpen = IsPaneOpen;
        settings.Theme = Theme.ToString();
        settings.RecentCommands = string.Join(',', RecentCommands);
        settings.CommandCounts = string.Join(',', CommandCounts.Select(static x => String.Create(CultureInfo.InvariantCulture, $"{x.Key}={x.Value}")));
        settings.LastVersion = CurrentVersion;

        settings.Save();
    }
}
