namespace Template.WindowsApp;

using System.Runtime.InteropServices;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Smart.Mvvm.Resolver;

using Template.WindowsApp.Settings;
using Template.WindowsApp.Shell;
using Template.WindowsApp.Views;
using Template.WindowsApp.Views.Dialogs;

public sealed partial class App
{
    private readonly IHost host;

    private readonly ILogger<App> log;

    private readonly IWindowManager windowManager;

    private bool showingError;

    public SingleInstance? SingleInstance { get; init; }

    //--------------------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------------------

    public App()
    {
        InitializeComponent();

        Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        host = CreateHost();

        log = host.Services.GetRequiredService<ILogger<App>>();
        windowManager = host.Services.GetRequiredService<IWindowManager>();
        ResolveProvider.Default.Provider = host.Services;

        var environment = host.Services.GetRequiredService<IHostEnvironment>();
        ThreadPool.GetMinThreads(out var workerThreads, out var completionPortThreads);

        log.InfoStartup();
        log.InfoStartupSettingsRuntime(RuntimeInformation.OSDescription, RuntimeInformation.FrameworkDescription, RuntimeInformation.RuntimeIdentifier);
        log.InfoStartupSettingsGC(GCSettings.IsServerGC, GCSettings.LatencyMode, GCSettings.LargeObjectHeapCompactionMode);
        log.InfoStartupSettingsThreadPool(workerThreads, completionPortThreads);
        log.InfoStartupApplication(environment.ApplicationName, typeof(App).Assembly.GetName().Version);
        log.InfoStartupEnvironment(environment.EnvironmentName, environment.ContentRootPath);

        Current.DispatcherUnhandledException += (_, ea) =>
        {
            ShowError(ea.Exception);
            ea.Handled = true;
        };
        AppDomain.CurrentDomain.UnhandledException += (_, ea) => Dispatcher.Invoke(() => ShowError((Exception)ea.ExceptionObject));
        TaskScheduler.UnobservedTaskException += (_, ea) => log.ErrorUnobservedTaskException(ea.Exception);
    }

    private static IHost CreateHost()
    {
        var builder = Host.CreateApplicationBuilder(Environment.GetCommandLineArgs());

        // Container
        builder.ConfigureContainer();
        // Log
        builder.ConfigureLogging();
        // Components
        builder.ConfigureComponents();

        var host = builder.Build();
#if DEBUG
        if (host.Services is BunnyTail.DependencyInjection.GeneratedServiceProvider generatedProvider)
        {
            foreach (var line in BunnyTail.DependencyInjection.Diagnostics.ServiceFactoryReportExtensions.DescribeRuntimeFallbacks(generatedProvider).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
            {
                System.Diagnostics.Debug.WriteLine(line);
            }
        }

        // Setup navigator
        var navigator = host.Services.GetRequiredService<INavigator>();
        navigator.Navigated += (_, args) =>
        {
            // for debug
            System.Diagnostics.Debug.WriteLine($"Navigated: [{args.Context.FromId}]->[{args.Context.ToId}] : stacked=[{navigator.StackedCount}]");
        };
#endif
        return host;
    }

    //--------------------------------------------------------------------------------
    // Lifecycle
    //--------------------------------------------------------------------------------

    // ReSharper disable once AsyncVoidEventHandlerMethod
    protected override async void OnStartup(StartupEventArgs e)
    {
        var mainWindow = windowManager.Load();
        mainWindow.Closed += OnMainWindowClosed;
        MainWindow = mainWindow;

        await host.StartAsync().ConfigureAwait(true);

        await host.Services.GetRequiredService<INavigator>().ForwardAsync(FindView(e.Args) ?? StartView()).ConfigureAwait(true);

        SingleInstance?.Listen(OnSecondInstance);
    }

    // ReSharper disable once AsyncVoidEventHandlerMethod
    private async void OnMainWindowClosed(object? sender, EventArgs e)
    {
        try
        {
            await host.StopAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(true);
        }
        finally
        {
            Shutdown();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        host.Dispose();
    }

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    // ReSharper disable once AsyncVoidMethod
    private async void OnSecondInstance(string[] args)
    {
        log.InfoSecondInstance(String.Join(' ', args));

        if (MainWindow is { } window)
        {
            if (window.WindowState == WindowState.Minimized)
            {
                SystemCommands.RestoreWindow(window);
            }
            window.Activate();
        }

        var navigator = host.Services.GetRequiredService<INavigator>();
        if ((FindView(args) is { } id) && !Equals(navigator.CurrentViewId, id))
        {
            await navigator.ForwardAsync(id).ConfigureAwait(true);
        }
    }

    private void ShowError(Exception ex)
    {
        log.ErrorUnknownException(ex);

        if (showingError)
        {
            return;
        }

        showingError = true;
        try
        {
            var viewModel = host.Services.GetRequiredService<ErrorDialogViewModel>();
            viewModel.Details = MakeDetails(ex);
            viewModel.LogFolder = FindLogFolder();
            var dialog = new ErrorDialog
            {
                DataContext = viewModel
            };
            if (MainWindow is { IsLoaded: true } owner)
            {
                dialog.Owner = owner;
            }
            else
            {
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                dialog.ShowInTaskbar = true;
            }
            dialog.ShowDialog();
        }
        finally
        {
            showingError = false;
        }
    }

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private ViewId StartView() =>
        (windowManager.StartPage == StartPage.LastPage) && (windowManager.LastView is { } id) ? id : ViewId.Home;

    private ViewId? FindView(string[] args)
    {
        var value = new ConfigurationBuilder().AddCommandLine(args).Build()["view"];
        if (String.IsNullOrEmpty(value))
        {
            return null;
        }

        foreach (var id in Enum.GetValues<ViewId>())
        {
            if (String.Equals(id.ToString(), value, StringComparison.OrdinalIgnoreCase))
            {
                return id;
            }
        }

        log.WarnUnknownView(value);
        return null;
    }

    private string MakeDetails(Exception ex)
    {
        var time = host.Services.GetRequiredService<TimeProvider>().GetLocalNow();
        return $"""
            Time: {time:yyyy-MM-dd HH:mm:ss}
            Version: {typeof(App).Assembly.GetName().Version}
            OS: {RuntimeInformation.OSDescription}
            .NET: {RuntimeInformation.FrameworkDescription}

            {ex}
            """;
    }

    private string? FindLogFolder()
    {
        var path = host.Services.GetRequiredService<IConfiguration>().GetSection("Serilog:WriteTo").GetChildren()
            .Where(static x => String.Equals(x["Name"], "File", StringComparison.OrdinalIgnoreCase))
            .Select(static x => x["Args:path"])
            .FirstOrDefault(static x => !String.IsNullOrEmpty(x));
        return path is null ? null : Path.GetDirectoryName(Path.GetFullPath(path));
    }
}
