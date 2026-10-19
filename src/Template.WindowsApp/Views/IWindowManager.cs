namespace Template.WindowsApp.Views;

using Microsoft.Extensions.DependencyInjection;

using Template.WindowsApp.Settings;

public interface IWindowManager
{
    bool IsPaneOpen { get; set; }

    IList<string> RecentCommands { get; }

    Window Load();

    void Save();
}

public sealed class WindowManager : NotificationObject, IWindowManager
{
    private readonly IServiceProvider provider;

    private readonly WindowSettings settings = new();

    public bool IsPaneOpen
    {
        get;
        set => SetProperty(ref field, value);
    }

    public IList<string> RecentCommands { get; } = [];

    public WindowManager(IServiceProvider provider)
    {
        this.provider = provider;
    }

    public Window Load()
    {
        var mainWindow = provider.GetRequiredService<MainWindow>();

        if (settings.MainWindowPlacement is not null)
        {
            RestorePlacement(mainWindow, settings.MainWindowPlacement);
        }
        IsPaneOpen = settings.IsPaneOpen;
        foreach (var id in (settings.RecentCommands ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            RecentCommands.Add(id);
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
        settings.RecentCommands = string.Join(',', RecentCommands);

        settings.Save();
    }
}
