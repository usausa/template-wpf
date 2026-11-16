namespace Template.WindowsApp.Services;

public sealed class GreetOption
{
    [Required]
    public string Message { get; set; } = default!;
}

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class GreetService
{
    private readonly TimeProvider timeProvider;

    private readonly GreetOption option;

    public GreetService(TimeProvider timeProvider, GreetOption option)
    {
        this.timeProvider = timeProvider;
        this.option = option;
    }

    public string MakeMessage() => $"{option.Message} ({timeProvider.GetLocalNow():HH:mm:ss})";
}
