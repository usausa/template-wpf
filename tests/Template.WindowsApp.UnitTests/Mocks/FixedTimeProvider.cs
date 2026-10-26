namespace Template.WindowsApp.Mocks;

public sealed class FixedTimeProvider : TimeProvider
{
    public DateTimeOffset Current { get; set; }

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;

    public FixedTimeProvider(DateTimeOffset current)
    {
        Current = current;
    }

    public override DateTimeOffset GetUtcNow() => Current;
}
