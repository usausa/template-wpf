namespace Template.WindowsApp;

using System.Runtime.InteropServices;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Smart.Windows.Resolver;

using Template.WindowsApp.Views;

public sealed partial class App
{
    private readonly IHost host;

    private readonly ILogger<App> log;

    private readonly IWindowManager windowManager;

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public App()
    {
        InitializeComponent();

        Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        host = CreateHost();

        log = host.Services.GetRequiredService<ILogger<App>>();
        this.windowManager = host.Services.GetRequiredService<IWindowManager>();
        ResolveProvider.Default.Provider = host.Services;

        var environment = host.Services.GetRequiredService<IHostEnvironment>();
        ThreadPool.GetMinThreads(out var workerThreads, out var completionPortThreads);

        log.InfoStartup();
        log.InfoStartupSettingsRuntime(RuntimeInformation.OSDescription, RuntimeInformation.FrameworkDescription, RuntimeInformation.RuntimeIdentifier);
        log.InfoStartupSettingsGC(GCSettings.IsServerGC, GCSettings.LatencyMode, GCSettings.LargeObjectHeapCompactionMode);
        log.InfoStartupSettingsThreadPool(workerThreads, completionPortThreads);
        log.InfoStartupApplication(environment.ApplicationName, typeof(App).Assembly.GetName().Version);
        log.InfoStartupEnvironment(environment.EnvironmentName, environment.ContentRootPath);

        Current.DispatcherUnhandledException += (_, ea) => HandleException(ea.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, ea) => HandleException((Exception)ea.ExceptionObject);
    }

    private static IHost CreateHost()
    {
        var builder = Host.CreateApplicationBuilder(Environment.GetCommandLineArgs());

        // Log
        builder.ConfigureLogging();
        // Components
        builder.ConfigureComponents();

        return builder.Build();
    }

    //--------------------------------------------------------------------------------
    // Lifecycle
    //--------------------------------------------------------------------------------

    // ReSharper disable once AsyncVoidMethod
    protected override async void OnStartup(StartupEventArgs e)
    {
        MainWindow = windowManager.Load();

        await host.StartAsync().ConfigureAwait(false);
    }

    // ReSharper disable once AsyncVoidMethod
    protected override async void OnExit(ExitEventArgs e)
    {
        await host.StopAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        host.Dispose();
    }

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private void HandleException(Exception ex)
    {
        log.ErrorUnknownException(ex);

        MessageBox.Show(ex.ToString(), "Unknown error.", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
