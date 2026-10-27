namespace Template.WindowsApp.Views.Forms;

using Template.WindowsApp.Models.Sample;

public enum ServerGrouping
{
    None,
    Role,
    Status
}

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class GridVisualsViewModel : AppViewModelBase
{
    private const int Hours = 24;

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
        servers = [.. ServerSample.LoadServers()];
        GroupBy = ServerGrouping.Role;
        ServerCount = servers.Count;
        WarningCount = servers.Count(static x => x.Status == ServerState.Warning);
        DownCount = servers.Count(static x => x.Status == ServerState.Down);
        AverageCpu = servers.Where(static x => x.Cpu is not null).Average(static x => x.Cpu!.Value);
        ErrorTotal = servers.Sum(static x => x.Errors ?? 0);

        SubscribeSort(_ => ShowServers());

        ShowServers();
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
