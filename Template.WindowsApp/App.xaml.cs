namespace Template.WindowsApp;

using Template.WindowsApp.Views;

public sealed partial class App
{
    private readonly ILogger<App> log;

    private readonly IWindowManager windowManager;

    public App(
        ILogger<App> log,
        IWindowManager windowManager)
    {
        this.log = log;
        this.windowManager = windowManager;

        Current.DispatcherUnhandledException += (_, ea) => HandleException(ea.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, ea) => HandleException((Exception)ea.ExceptionObject);

        InitializeComponent();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        MainWindow = windowManager.Load();
    }

    private void HandleException(Exception ex)
    {
        log.ErrorUnknownException(ex);

        MessageBox.Show(ex.ToString(), "Unknown error.", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
