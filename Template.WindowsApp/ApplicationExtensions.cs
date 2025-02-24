namespace Template.WindowsApp;

using System.Runtime.InteropServices;
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

public static class ApplicationExtensions
{
    //--------------------------------------------------------------------------------
    // System
    //--------------------------------------------------------------------------------

    public static IHostApplicationBuilder ConfigureSystemDefaults(this IHostApplicationBuilder builder)
    {
        Thread.CurrentThread.SetApartmentState(ApartmentState.Unknown);
        Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

        Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        return builder;
    }

    //--------------------------------------------------------------------------------
    // Logging
    //--------------------------------------------------------------------------------

    public static IHostApplicationBuilder ConfigureLoggingDefaults(this IHostApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Services.AddSerilog(options =>
        {
            options.ReadFrom.Configuration(builder.Configuration);
        });

        return builder;
    }

    //--------------------------------------------------------------------------------
    // Components
    //--------------------------------------------------------------------------------

    public static IHostApplicationBuilder ConfigureComponents(this IHostApplicationBuilder builder)
    {
        builder.ConfigureContainer(new SmartServiceProviderFactory(), x => ConfigureContainer(builder.Configuration, x));

        RestConfig.Default.UseJsonSerializer(static config =>
        {
            config.Converters.Add(new Template.WindowsApp.Helpers.DateTimeConverter());
            config.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            config.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

        builder.Services.AddHttpClient();

        // TODO Navigation ?
        // TODO BunnyTail ?

        return builder;
    }

    private static void ConfigureContainer(IConfigurationManager configuration, ResolverConfig config)
    {
        config
            .UseAutoBinding()
            .UseArrayBinding()
            .UseAssignableBinding();

        config.BindConfig<ClientSettings>(configuration.GetSection("Client"));

        config.BindSingleton<IWindowManager, WindowManager>();
        config.BindSingleton<MainWindow>();
    }

    //--------------------------------------------------------------------------------
    // Startup
    //--------------------------------------------------------------------------------

    public static void LogStartupInformation(this IHost app)
    {
        var log = app.Services.GetRequiredService<ILogger<Program>>();
        var environment = app.Services.GetRequiredService<IHostEnvironment>();
        ThreadPool.GetMinThreads(out var workerThreads, out var completionPortThreads);

        log.InfoStartup();
        log.InfoStartupSettingsRuntime(RuntimeInformation.OSDescription, RuntimeInformation.FrameworkDescription, RuntimeInformation.RuntimeIdentifier);
        log.InfoStartupSettingsGC(GCSettings.IsServerGC, GCSettings.LatencyMode, GCSettings.LargeObjectHeapCompactionMode);
        log.InfoStartupSettingsThreadPool(workerThreads, completionPortThreads);
        log.InfoStartupApplication(environment.ApplicationName, typeof(Program).Assembly.GetName().Version);
        log.InfoStartupEnvironment(environment.EnvironmentName, environment.ContentRootPath);
    }

    public static void RunApplication(this IHost app)
    {
        ResolveProvider.Default.Provider = app.Services;

        app.Services.GetRequiredService<App>().Run();
    }
}
