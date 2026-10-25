namespace Template.WindowsApp.Views.Forms;

public sealed class ServerRow
{
    private const double CautionUsage = 80;

    private const double CriticalUsage = 90;

    private const double CautionUptime = 99.9;

    private const double CriticalUptime = 99;

    public required string Name { get; init; }

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
