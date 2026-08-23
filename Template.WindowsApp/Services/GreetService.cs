namespace Template.WindowsApp.Services;

using Template.WindowsApp.Settings;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class GreetService
{
    private readonly TimeProvider timeProvider;

    private readonly ClientSettings settings;

    public GreetService(TimeProvider timeProvider, ClientSettings settings)
    {
        this.timeProvider = timeProvider;
        this.settings = settings;
    }

    public string MakeMessage() => $"{settings.Greeting} ({timeProvider.GetLocalNow():HH:mm:ss})";
}
