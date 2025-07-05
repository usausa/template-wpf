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

using Template.WindowsApp.Settings;
using Template.WindowsApp.Views;

public static class ApplicationExtensions
{
    //--------------------------------------------------------------------------------
    // Logging
    //--------------------------------------------------------------------------------

    public static IHostApplicationBuilder ConfigureLogging(this IHostApplicationBuilder builder)
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

        config.BindSingleton<IReactiveMessenger>(ReactiveMessenger.Default);

        config.BindConfig<ClientSettings>(configuration.GetSection("Client"));

        config.BindSingleton<IWindowManager, WindowManager>();
        config.BindSingleton<MainWindow>();
    }
}
