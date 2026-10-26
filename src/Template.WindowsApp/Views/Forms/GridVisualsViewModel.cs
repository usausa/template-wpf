namespace Template.WindowsApp.Views.Forms;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class GridVisualsViewModel : AppViewModelBase
{
    private const int Hours = 24;

    private const int RiseHours = 6;

    private readonly List<ServerRow> servers;

    public static IReadOnlyList<string> HourLabels { get; } = MakeHourLabels();

    public ObservableCollection<ServerRow> Items { get; } = [];

    [ObservableProperty]
    public partial SortDescription? Sort { get; set; }

    [ObservableProperty]
    public partial ServerGrouping GroupBy { get; set; }

    public int ServerCount { get; }

    public int WarningCount { get; }

    public int DownCount { get; }

    public double AverageCpu { get; }

    public int ErrorTotal { get; }

    public GridVisualsViewModel()
    {
        servers = MakeServers();
        GroupBy = ServerGrouping.Role;
        ServerCount = servers.Count;
        WarningCount = servers.Count(static x => x.Status == ServerState.Warning);
        DownCount = servers.Count(static x => x.Status == ServerState.Down);
        AverageCpu = servers.Where(static x => x.Cpu is not null).Average(static x => x.Cpu!.Value);
        ErrorTotal = servers.Sum(static x => x.Errors ?? 0);

        SubscribeSort(_ => ShowServers());

        ShowServers();
    }

    private static List<ServerRow> MakeServers() =>
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

    private static string[] MakeHourLabels()
    {
        var labels = new string[Hours];
        for (var i = 0; i < Hours; i++)
        {
            labels[i] = (i % 6) == 5 ? String.Create(CultureInfo.InvariantCulture, $"-{Hours - 1 - i}h") : string.Empty;
        }
        labels[^1] = "now";
        return labels;
    }

    private void ShowServers()
    {
        Items.Clear();
        foreach (var server in Query())
        {
            Items.Add(server);
        }
    }

    private IEnumerable<ServerRow> Query()
    {
        if (Sort is not { } sort)
        {
            return servers;
        }

        Func<ServerRow, object?> key = sort.PropertyName switch
        {
            nameof(ServerRow.Status) => static x => x.Status,
            nameof(ServerRow.Cpu) => static x => x.Cpu,
            nameof(ServerRow.MemoryPercent) => static x => x.MemoryPercent,
            nameof(ServerRow.Errors) => static x => x.Errors,
            nameof(ServerRow.LatencyNow) => static x => x.LatencyNow,
            nameof(ServerRow.Uptime) => static x => x.Uptime,
            _ => static x => x.Name
        };
        var query = servers.OrderBy(x => key(x) is null);
        return sort.Direction == ListSortDirection.Ascending ? query.ThenBy(key) : query.ThenByDescending(key);
    }
}
