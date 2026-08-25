namespace Template.WindowsApp;

using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;

using BunnyTail.ServiceRegistration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Rester;

using Serilog;

using Smart.Resolver;

using Template.WindowsApp.Settings;
using Template.WindowsApp.Views;

public static partial class ApplicationExtensions
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
        builder.ConfigureContainer(new SmartServiceProviderFactory(), ConfigureContainer);

        RestConfig.Default.UseJsonSerializer(static config =>
        {
            config.Converters.Add(new Template.WindowsApp.Helpers.DateTimeConverter());
            config.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            config.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

        builder.Services.AddHttpClient();

        // System
        builder.Services.AddSingleton(TimeProvider.System);

        // Setting
        builder.Services.AddOptions<ClientSettings>().BindConfiguration("Client").ValidateDataAnnotations().ValidateOnStart();
        builder.Services.AddSingleton(static p => p.GetRequiredService<IOptions<ClientSettings>>().Value);

        // Service
        builder.Services.AddServices();

        return builder;
    }

    private static void ConfigureContainer(ResolverConfig config)
    {
        config
            .UseAutoBinding()
            .UseArrayBinding()
            .UseAssignableBinding();

        // Messenger
        config.BindSingleton<IReactiveMessenger>(ReactiveMessenger.Default);

        // Navigation
        config.BindSingleton<Navigator>(static resolver =>
        {
            var navigator = new NavigatorConfig()
                .UseWindowsNavigationProvider()
                .UseServiceProvider(resolver)
                .UseIdViewMapper(static m => m.AutoRegister(ViewSource()))
                .ToNavigator();
#if DEBUG
            navigator.Navigated += static (_, args) =>
            {
                // for debug
                System.Diagnostics.Debug.WriteLine($"Navigated: [{args.Context.FromId}]->[{args.Context.ToId}]");
            };
#endif

            return navigator;
        });

        // Window
        config.BindSingleton<IWindowManager, WindowManager>();
        config.BindSingleton<MainWindow>();
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    [ViewSource]
    public static partial IEnumerable<KeyValuePair<ViewId, Type>> ViewSource();

    //--------------------------------------------------------------------------------
    // Service
    //--------------------------------------------------------------------------------

    [ServiceRegistration(Lifetime.Singleton, "Service$")]
    public static partial IServiceCollection AddServices(this IServiceCollection services);
}
