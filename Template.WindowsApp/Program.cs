using System.Runtime.InteropServices;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Rester;

using Serilog;

using Smart.Windows.Resolver;

using Template.WindowsApp.Views;
using Template.WindowsApp;
using Template.WindowsApp.Settings;

//--------------------------------------------------------------------------------
// Setup environment
//--------------------------------------------------------------------------------

Thread.CurrentThread.SetApartmentState(ApartmentState.Unknown);
Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

//--------------------------------------------------------------------------------
// Configure builder
//--------------------------------------------------------------------------------

var builder = Host.CreateApplicationBuilder(args);

// Configuration
builder.Services.Configure<ClientSettings>(builder.Configuration.GetSection("Client"));
builder.Services.AddSingleton(static p => p.GetRequiredService<IOptions<ClientSettings>>().Value);

// Logging
builder.Logging.ClearProviders();
builder.Services.AddSerilog(options =>
{
    options.ReadFrom.Configuration(builder.Configuration);
});

// Json
RestConfig.Default.UseJsonSerializer(static config =>
{
    config.Converters.Add(new Template.WindowsApp.Helpers.DateTimeConverter());
    config.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
    config.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// Application
//builder.Services.AddWpf<App>();
builder.Services.AddSingleton<App>();
builder.Services.AddSingleton<IWindowManager, WindowManager>();
builder.Services.AddSingleton<MainWindow>();
builder.Services.AddSingleton<MainWindowViewModel>();
// TODO

//--------------------------------------------------------------------------------
// Build host
//--------------------------------------------------------------------------------

var app = builder.Build();

// Resolver
ResolveProvider.Default.Provider = app.Services;

// Log
var log = app.Services.GetRequiredService<ILogger<Program>>();
var environment = app.Services.GetRequiredService<IHostEnvironment>();
ThreadPool.GetMinThreads(out var workerThreads, out var completionPortThreads);

log.InfoStartup();
log.InfoStartupSettingsRuntime(RuntimeInformation.OSDescription, RuntimeInformation.FrameworkDescription, RuntimeInformation.RuntimeIdentifier);
log.InfoStartupSettingsGC(GCSettings.IsServerGC, GCSettings.LatencyMode, GCSettings.LargeObjectHeapCompactionMode);
log.InfoStartupSettingsThreadPool(workerThreads, completionPortThreads);
log.InfoStartupApplication(environment.ApplicationName, typeof(Program).Assembly.GetName().Version);
log.InfoStartupEnvironment(environment.EnvironmentName, environment.ContentRootPath);

// Run
app.Services.GetRequiredService<App>().Run();
