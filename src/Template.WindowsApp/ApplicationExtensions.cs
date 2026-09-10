namespace Template.WindowsApp;

using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;

using BunnyTail.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Rester;

using Serilog;

using Template.WindowsApp.Services;
using Template.WindowsApp.Settings;
using Template.WindowsApp.Views;

public static partial class ApplicationExtensions
{
    //--------------------------------------------------------------------------------
    // Container
    //--------------------------------------------------------------------------------

    public static IHostApplicationBuilder ConfigureContainer(this IHostApplicationBuilder builder)
    {
        builder.ConfigureContainer(new GeneratedServiceProviderFactory(static options => options.TrackTransientDisposables = false));

        return builder;
    }

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
        // System
        builder.Services.AddSingleton(TimeProvider.System);

        // Setting
        builder.Services.AddOptions<ClientSettings>().BindConfiguration("Client").ValidateDataAnnotations().ValidateOnStart();
        builder.Services.AddSingleton(static p => p.GetRequiredService<IOptions<ClientSettings>>().Value);

        // Messenger
        builder.Services.AddSingleton<IReactiveMessenger>(ReactiveMessenger.Default);

        // Navigation
        builder.Services.AddNavigator(static (_, config) =>
        {
            config.UseWindowsNavigationProvider();
            config.UseIdViewMapper(static m => m.AutoRegister(ViewSource()));
        });

        // Rest
        RestConfig.Default.UseJsonSerializer(static config =>
        {
            config.Converters.Add(new Template.WindowsApp.Helpers.DateTimeConverter());
            config.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            config.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

        builder.Services.AddHttpClient();

        // Service
        builder.Services.AddServices();
        builder.Services.AddSingleton<IDialogService, DialogService>();

        // Window
        builder.Services.AddSingleton<IWindowManager, WindowManager>();
        builder.Services.AddSingleton<MainWindow>();
        // View & ViewModel
        builder.Services.AddViews();
        builder.Services.AddViewModels();

        return builder;
    }

    //--------------------------------------------------------------------------------
    // Navigation
    //--------------------------------------------------------------------------------

    [ViewSource]
    public static partial IEnumerable<KeyValuePair<ViewId, Type>> ViewSource();

    //--------------------------------------------------------------------------------
    // Service
    //--------------------------------------------------------------------------------

    [ComponentRegistration(Lifetime.Singleton, "Service$")]
    public static partial IServiceCollection AddServices(this IServiceCollection services);

    //--------------------------------------------------------------------------------
    // View & ViewModel
    //--------------------------------------------------------------------------------

    [ComponentRegistration(Lifetime.Transient, "View$")]
    public static partial IServiceCollection AddViews(this IServiceCollection services);

    [ComponentRegistration(Lifetime.Transient, "ViewModel$")]
    public static partial IServiceCollection AddViewModels(this IServiceCollection services);
}
