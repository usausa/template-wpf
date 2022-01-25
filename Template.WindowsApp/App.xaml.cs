namespace Template.WindowsApp;

using System.Windows;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Smart.Resolver;
using Smart.Windows.Resolver;

public partial class App
{
    private readonly IHost host;

    public App()
    {
        host = Host.CreateDefaultBuilder()
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

        //config.BindConfig<Settings>(context.Configuration.GetSection("Setting"));
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await host.StartAsync().ConfigureAwait(false);

        ResolveProvider.Default.UseServiceProvider(host.Services);

        MainWindow = (MainWindow)host.Services.GetRequiredService(typeof(MainWindow));
        MainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await host.StopAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        host.Dispose();
    }
}
