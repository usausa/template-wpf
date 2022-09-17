namespace Template.WindowsApp;

using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Rester;

using Serilog;

using Smart.Resolver;
using Smart.Windows.Resolver;

using Template.WindowsApp.Settings;
using Template.WindowsApp.Views;

public partial class App
{
    private readonly IHost host;

    public App()
    {
#if !DEBUG
        Current.DispatcherUnhandledException += (_, ea) => HandleException(ea.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, ea) => HandleException((Exception)ea.ExceptionObject);
#endif
        host = Host.CreateDefaultBuilder()
            .UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration))
            .UseServiceProviderFactory(new SmartServiceProviderFactory())
            .ConfigureContainer<ResolverConfig>(ConfigureContainer)
            .Build();
    }

    private static void ConfigureContainer(HostBuilderContext context, ResolverConfig config)
    {
        config
            .UseAutoBinding()
            .UseArrayBinding()
            .UseAssignableBinding();

        config.BindConfig<ClientSettings>(context.Configuration.GetSection("Client"));

        config.BindSingleton<IWindowManager, WindowManager>();
        config.BindSingleton<MainWindow>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await host.StartAsync().ConfigureAwait(false);

        ResolveProvider.Default.Provider = host.Services;

        RestConfig.Default.UseJsonSerializer(config =>
        {
            config.Converters.Add(new Template.WindowsApp.Helpers.DateTimeConverter());
            config.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            config.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

        MainWindow = host.Services.GetRequiredService<IWindowManager>().Load();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await host.StopAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        host.Dispose();
    }

#if !DEBUG
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "ex", Justification = "Debug only.")]
    private static void HandleException(Exception ex)
    {
        MessageBox.Show(ex.ToString(), "Unknown error.", MessageBoxButton.OK, MessageBoxImage.Error);
    }
#endif
}
