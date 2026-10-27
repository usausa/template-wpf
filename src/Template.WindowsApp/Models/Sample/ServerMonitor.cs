namespace Template.WindowsApp.Models.Sample;

//--------------------------------------------------------------------------------
// Enum
//--------------------------------------------------------------------------------

public enum ServerState
{
    Healthy,
    Warning,
    Down
}

public enum MetricLevel
{
    Normal,
    Caution,
    Critical
}

public enum ChangeDirection
{
    Same,
    Up,
    Down
}

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed class ServerRow
{
    private const double CautionUsage = 80;

    private const double CriticalUsage = 90;

    private const double CautionUptime = 99.9;

    private const double CriticalUptime = 99;

    public required string Name { get; init; }

    public string Role => Name.Split('-')[0];

    public ServerState Status { get; init; }

    public double? Cpu { get; init; }

    public double? MemoryUsed { get; init; }

    public double MemoryTotal { get; init; }

    public int? Errors { get; init; }

    public int? ErrorChange { get; init; }

    public IReadOnlyList<double>? Latency { get; init; }

    public double Uptime { get; init; }

    public MetricLevel CpuLevel => UsageLevel(Cpu);

    public double? MemoryPercent => MemoryUsed * 100 / MemoryTotal;

    public MetricLevel MemoryLevel => UsageLevel(MemoryPercent);

    public ChangeDirection ErrorDirection => ErrorChange switch
    {
        > 0 => ChangeDirection.Up,
        < 0 => ChangeDirection.Down,
        _ => ChangeDirection.Same
    };

    public int ErrorChangeAmount => Math.Abs(ErrorChange ?? 0);

    public int? ErrorsYesterday => Errors - ErrorChange;

    public double? LatencyNow => Latency?[^1];

    public double? LatencyMin => Latency?.Min();

    public double? LatencyMax => Latency?.Max();

    public MetricLevel UptimeLevel => Uptime switch
    {
        < CriticalUptime => MetricLevel.Critical,
        < CautionUptime => MetricLevel.Caution,
        _ => MetricLevel.Normal
    };

    public override string ToString() => Name;

    private static MetricLevel UsageLevel(double? value) => value switch
    {
        >= CriticalUsage => MetricLevel.Critical,
        >= CautionUsage => MetricLevel.Caution,
        _ => MetricLevel.Normal
    };
}

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

public static class ServerSample
{
    private const int Hours = 24;

    private const int RiseHours = 6;

    public static IReadOnlyList<ServerRow> LoadServers() =>
    [
        Make("web-01", ServerState.Healthy, 42, 9.6, 16, 3, -2, 38, 0, 99.99),
        Make("web-02", ServerState.Healthy, 57, 11.2, 16, 5, 1, 41, 0.1, 99.98),
        Make("web-03", ServerState.Warning, 86, 13.1, 16, 12, 7, 64, 0.5, 99.92),
        Make("api-01", ServerState.Healthy, 35, 6.4, 16, 0, -1, 22, 0, 99.99),
        Make("api-02", ServerState.Healthy, 48, 7.9, 16, 2, 0, 25, 0, 99.97),
        Make("api-03", ServerState.Warning, 93, 14.6, 16, 18, 11, 88, 0.8, 99.81),
        Make("db-01", ServerState.Healthy, 61, 52.3, 64, 1, -3, 6, 0, 99.99),
        Make("db-02", ServerState.Healthy, 38, 41.0, 64, 0, 0, 5, 0, 99.99),
        Make("cache-01", ServerState.Healthy, 21, 24.8, 32, 0, -1, 2, 0, 99.99),
        Make("queue-01", ServerState.Warning, 74, 29.5, 32, 9, 4, 15, 0.3, 99.95),
        Make("batch-01", ServerState.Healthy, 12, 4.1, 16, 4, -6, 120, 0, 99.6),
        new() { Name = "mail-01", Status = ServerState.Down, MemoryTotal = 16, Uptime = 97.8 }
    ];

    private static ServerRow Make(string name, ServerState status, double cpu, double memoryUsed, double memoryTotal, int errors, int errorChange, double latency, double rise, double uptime)
    {
        var seed = name.Sum(static x => x);
        var values = new double[Hours];
        for (var i = 0; i < values.Length; i++)
        {
            var wave = Math.Sin(((i + seed) % Hours) * Math.PI / 6) * 0.12;
            var noise = ((((seed * 31) + (i * 17)) % 11) - 5) / 50.0;
            var tail = i >= Hours - RiseHours ? rise * (i - (Hours - RiseHours - 1)) / RiseHours : 0;
            values[i] = Math.Round(latency * (1 + wave + noise + tail), 1);
        }

        return new ServerRow
        {
            Name = name,
            Status = status,
            Cpu = cpu,
            MemoryUsed = memoryUsed,
            MemoryTotal = memoryTotal,
            Errors = errors,
            ErrorChange = errorChange,
            Latency = values,
            Uptime = uptime
        };
    }
}
