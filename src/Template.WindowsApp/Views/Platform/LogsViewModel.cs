namespace Template.WindowsApp.Views.Platform;

using Serilog.Events;

using Template.WindowsApp.Services;
using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class LogsViewModel : AppViewModelBase, IShellCommandProvider
{
    private const string AllLevels = "All levels";

    private readonly ListCollectionView view;

    private long lastSequence;

    private LogEventLevel minimumLevel = LogEventLevel.Verbose;

    private string filterText = string.Empty;

    public ObservableCollection<LogEntry> Items { get; } = [];

    public IReadOnlyList<string> Levels { get; } =
    [
        AllLevels,
        nameof(LogEventLevel.Debug),
        nameof(LogEventLevel.Information),
        nameof(LogEventLevel.Warning),
        nameof(LogEventLevel.Error)
    ];

    [ObservableProperty]
    public partial string Level { get; set; }

    [ObservableProperty]
    public partial string SearchText { get; set; }

    [ObservableProperty]
    public partial bool AutoScroll { get; set; }

    [ObservableProperty]
    public partial LogEntry? SelectedEntry { get; set; }

    [ObservableProperty]
    public partial string? Details { get; set; }

    [ObservableProperty]
    public partial int ShownCount { get; set; }

    [ObservableProperty]
    public partial int TotalCount { get; set; }

    public IObserveCommand CopyCommand { get; }

    public IObserveCommand ClearCommand { get; }

    public IObserveCommand WriteEntryCommand { get; }

    public LogsViewModel(
        ILogger<LogsViewModel> logger,
        IClipboardService clipboardService,
        MemoryLogService logService)
    {
        Level = AllLevels;
        SearchText = string.Empty;
        AutoScroll = true;

        Disposables.Add(logService.Added.ObserveOnDispatcher().Subscribe(Add));
        foreach (var entry in logService.Snapshot())
        {
            Items.Add(entry);
            lastSequence = entry.Sequence;
        }
        view = (ListCollectionView)CollectionViewSource.GetDefaultView(Items);
        view.Filter = IsVisible;
        UpdateCounts();

        SubscribeLevel(_ => Refresh());
        SubscribeSearchText(_ => Refresh());
        SubscribeSelectedEntry(x => Details = x is null ? null : x.Exception is null ? x.Message : $"{x.Message}{Environment.NewLine}{x.Exception}");

        CopyCommand = MakeDelegateCommand(() => clipboardService.SetText(MakeText()), () => ShownCount > 0);
        ClearCommand = MakeDelegateCommand(() =>
        {
            logService.Clear();
            Items.Clear();
            UpdateCounts();
        }, () => TotalCount > 0);
        WriteEntryCommand = MakeDelegateCommand<LogLevel>(logger.TestEntry);
    }

    private void Add(LogEntry entry)
    {
        if (entry.Sequence <= lastSequence)
        {
            return;
        }

        lastSequence = entry.Sequence;
        Items.Add(entry);
        if (Items.Count > MemoryLogService.Capacity)
        {
            Items.RemoveAt(0);
        }
        UpdateCounts();
    }

    private void Refresh()
    {
        minimumLevel = Enum.TryParse<LogEventLevel>(Level, out var level) ? level : LogEventLevel.Verbose;
        filterText = SearchText.Trim();
        view.Refresh();
        UpdateCounts();
    }

    private bool IsVisible(object item) =>
        (item is LogEntry entry) &&
        (entry.Level >= minimumLevel) &&
        ((filterText.Length == 0) ||
         entry.Message.Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
         entry.Source.Contains(filterText, StringComparison.OrdinalIgnoreCase));

    private void UpdateCounts()
    {
        ShownCount = view.Count;
        TotalCount = Items.Count;
    }

    private string MakeText() =>
        String.Join(Environment.NewLine, view.Cast<LogEntry>().Select(static x => String.Create(CultureInfo.InvariantCulture, $"{x.Timestamp:HH:mm:ss.fff} {x.Level} {x.Source} {x.Message}")));

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("Logs:Copy", "Copy", "Logs", "\uE8C8", CopyCommand),
        new("Logs:Clear", "Clear", "Logs", "\uE74D", ClearCommand),
        new("Logs:WriteEntry", "Write entry", "Logs", "\uE70F", WriteEntryCommand, LogLevel.Information)
    ];
}
