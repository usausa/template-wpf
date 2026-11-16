namespace Template.WindowsApp.Settings;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

public sealed class ClientSettingTests
{
    [Fact]
    public void BindsGreetMessage()
    {
        // Arrange
        using var provider = MakeProvider("Hello");

        // Act
        var setting = provider.GetRequiredService<IOptions<ClientSetting>>().Value;

        // Assert
        Assert.Equal("Hello", setting.Greet.Message);
    }

    [Fact]
    public void ValidatesGreetMessage()
    {
        // Arrange
        using var provider = MakeProvider(string.Empty);

        // Act & Assert
        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<ClientSetting>>().Value);
    }

    private static ServiceProvider MakeProvider(string message)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Client:Greet:Message"] = message })
            .Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddOptions<ClientSetting>().BindConfiguration("Client").ValidateDataAnnotations();
        return services.BuildServiceProvider();
    }
}
