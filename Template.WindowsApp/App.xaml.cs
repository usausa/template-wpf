namespace Template.WindowsApp;

using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Rester;

using Serilog;

using Smart.Resolver;
using Smart.Windows.Resolver;

using Template.WindowsApp.Settings;
using Template.WindowsApp.Views;

public sealed partial class App
{
    private readonly IHost host;

    public App()
    {
#if !DEBUG
        Current.DispatcherUnhandledException += (_, ea) => HandleException(ea.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, ea) => HandleException((Exception)ea.ExceptionObject);
#endif
        var builder = Host.CreateApplicationBuilder();

        // Logging
        builder.Logging.ClearProviders();
        builder.Services.AddSerilog(options =>
        {
            options.ReadFrom.Configuration(builder.Configuration);
        });

        // Container
        builder.ConfigureContainer(new SmartServiceProviderFactory(), x => ConfigureContainer(builder.Configuration, x));

        host = builder.Build();
    }

    private static void ConfigureContainer(ConfigurationManager configuration, ResolverConfig config)
    {
        config
            .UseAutoBinding()
            .UseArrayBinding()
            .UseAssignableBinding();

        config.BindConfig<ClientSettings>(configuration.GetSection("Client"));

        config.BindSingleton<IWindowManager, WindowManager>();
        config.BindSingleton<MainWindow>();
    }

#pragma warning disable IDE0001
    // ReSharper disable once AsyncVoidMethod
    protected override async void OnStartup(StartupEventArgs e)
    {
        await host.StartAsync().ConfigureAwait(false);

        ResolveProvider.Default.Provider = host.Services;

        RestConfig.Default.UseJsonSerializer(static config =>
        {
            config.Converters.Add(new Template.WindowsApp.Helpers.DateTimeConverter());
            config.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            config.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

        MainWindow = host.Services.GetRequiredService<IWindowManager>().Load();
    }
#pragma warning restore IDE0001

    // ReSharper disable once AsyncVoidMethod
    protected override async void OnExit(ExitEventArgs e)
    {
        await host.StopAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        host.Dispose();
    }

#if !DEBUG
    private static void HandleException(Exception ex)
    {
        MessageBox.Show(ex.ToString(), "Unknown error.", MessageBoxButton.OK, MessageBoxImage.Error);
    }
#endif
}
