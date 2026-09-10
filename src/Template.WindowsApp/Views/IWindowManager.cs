namespace Template.WindowsApp.Views;

using Microsoft.Extensions.DependencyInjection;

using Template.WindowsApp.Settings;

public interface IWindowManager
{
    Window Load();

    void Save();
}

public sealed class WindowManager : NotificationObject, IWindowManager
{
    private readonly IServiceProvider provider;

    private readonly WindowSettings settings = new();

    public WindowManager(IServiceProvider provider)
    {
        this.provider = provider;
    }

    public Window Load()
    {
        var mainWindow = provider.GetRequiredService<MainWindow>();

        if (settings.MainWindowPlacement is not null)
        {
            mainWindow.Left = settings.MainWindowPlacement.Left;
            mainWindow.Top = settings.MainWindowPlacement.Top;
            mainWindow.Width = settings.MainWindowPlacement.Width;
            mainWindow.Height = settings.MainWindowPlacement.Height;
            if (settings.MainWindowPlacement.Maximized)
            {
                mainWindow.WindowState = WindowState.Maximized;
            }
        }

        mainWindow.Show();

        return mainWindow;
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

        settings.Save();
    }
}
