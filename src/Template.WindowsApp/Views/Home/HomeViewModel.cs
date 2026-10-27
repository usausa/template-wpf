namespace Template.WindowsApp.Views.Home;

using System.Runtime.InteropServices;

using Serilog.Events;

using Template.WindowsApp.Extender;
using Template.WindowsApp.Services;
using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class HomeViewModel : AppViewModelBase
{
    private const int DebugIndex = 0;

    private const int InformationIndex = 1;

    private const int WarningIndex = 2;

    private const int ErrorIndex = 3;

    private readonly TimeProvider timeProvider;

    private readonly ProcessMonitorService monitor;

    private readonly Summary uptime;

    private readonly Summary memory;

    private readonly Summary logEntries;

    private readonly int[] levelCounts = new int[4];

    private long lastSequence;

    [ObservableProperty]
    public partial string Message { get; set; }

    public IReadOnlyList<Summary> Summaries { get; }

    public IReadOnlyList<string> MemoryLabels { get; } =
        Enumerable.Range(0, ProcessMonitorService.Capacity)
            .Select(static x => x == 0 ? "60 s ago" : x == ProcessMonitorService.Capacity - 1 ? "Now" : string.Empty)
            .ToArray();

    public ObservableCollection<double> MemoryValues { get; } = [];

    public IReadOnlyList<string> LevelLabels { get; } = ["Debug", "Information", "Warning", "Error"];

    public ObservableCollection<double> LevelValues { get; } = [0, 0, 0, 0];

    public IReadOnlyList<NavigationItem> Favorites { get; }

    public IReadOnlyList<NavigationItem> RecentPages { get; }

    public IReadOnlyList<SampleEntry> Samples { get; }

    public IObserveCommand ForwardCommand { get; }

    public HomeViewModel(
        TimeProvider timeProvider,
        GreetService greetService,
        MemoryLogService logService,
        ProcessMonitorService monitor,
        NavigationMenu menu)
    {
        this.timeProvider = timeProvider;
        this.monitor = monitor;

        Message = greetService.MakeMessage();

        uptime = new Summary("\u23F1\uFE0F", "Uptime", string.Empty, String.Create(CultureInfo.CurrentCulture, $"Since {monitor.StartTime:HH:mm}"));
        memory = new Summary("\U0001F9E0", "Memory", string.Empty, string.Empty);
        logEntries = new Summary("\U0001F4DC", "Log entries", string.Empty, string.Empty);
        Summaries =
        [
            new Summary("\U0001F3F7\uFE0F", "Version", typeof(HomeViewModel).Assembly.GetName().Version?.ToString(3) ?? string.Empty, RuntimeInformation.FrameworkDescription),
            uptime,
            memory,
            logEntries
        ];

        foreach (var value in monitor.Snapshot())
        {
            MemoryValues.Add(value);
        }
        UpdateProcess();
        Disposables.Add(monitor.Sampled.Subscribe(OnSampled));

        Disposables.Add(logService.Added.ObserveOnDispatcher().Subscribe(OnAdded));
        foreach (var entry in logService.Snapshot())
        {
            levelCounts[IndexOf(entry.Level)]++;
            lastSequence = entry.Sequence;
        }
        for (var i = 0; i < levelCounts.Length; i++)
        {
            LevelValues[i] = levelCounts[i];
        }
        UpdateLogEntries();

        Favorites = menu.Favorites.Items;
        RecentPages = menu.RecentPages;
        Samples = menu.Groups
            .Select(static x => new SampleEntry(x.Title, x.Emoji, string.Join(" · ", x.Items.Select(static y => y.Title)), x.Items[0].Id))
            .ToList();
        ForwardCommand = MakeAsyncCommand<ViewId>(x => Navigator.ForwardAsync(x, new NavigationParameter().WithEffect(EntranceEffect.Name)));
    }

    private static int IndexOf(LogEventLevel level) =>
        level switch
        {
            LogEventLevel.Verbose or LogEventLevel.Debug => DebugIndex,
            LogEventLevel.Information => InformationIndex,
            LogEventLevel.Warning => WarningIndex,
            _ => ErrorIndex
        };

    private static string Plural(int count, string word) =>
        String.Create(CultureInfo.CurrentCulture, $"{count:N0} {word}{(count == 1 ? string.Empty : "s")}");

    private void OnSampled(double value)
    {
        MemoryValues.Add(value);
        if (MemoryValues.Count > ProcessMonitorService.Capacity)
        {
            MemoryValues.RemoveAt(0);
        }
        UpdateProcess();
    }

    private void OnAdded(LogEntry entry)
    {
        if (entry.Sequence <= lastSequence)
        {
            return;
        }

        lastSequence = entry.Sequence;
        var index = IndexOf(entry.Level);
        levelCounts[index]++;
        LevelValues[index] = levelCounts[index];
        UpdateLogEntries();
    }

    private void UpdateProcess()
    {
        var elapsed = timeProvider.GetLocalNow() - monitor.StartTime;
        uptime.Value = String.Create(CultureInfo.CurrentCulture, $"{(int)elapsed.TotalHours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}");
        memory.Value = String.Create(CultureInfo.CurrentCulture, $"{monitor.Memory:0} MB");
        memory.Detail = String.Create(CultureInfo.CurrentCulture, $"Peak {monitor.PeakMemory:0} MB");
    }

    private void UpdateLogEntries()
    {
        logEntries.Value = levelCounts.Sum().ToString("N0", CultureInfo.CurrentCulture);
        logEntries.Detail = $"{Plural(levelCounts[WarningIndex], "warning")} · {Plural(levelCounts[ErrorIndex], "error")}";
    }
}
