namespace Template.WindowsApp.Views.Layout;

using System.Diagnostics;
using System.Windows.Threading;

using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class MenusToolbarsViewModel : AppViewModelBase, IShellCommandProvider
{
    private const string On = "On";

    private const string Off = "Off";

    private const string DefaultZoom = "100";

    private const string DefaultSortBy = "Name";

    private const int DefaultTextSize = 14;

    private const double BytesPerMegabyte = 1024 * 1024;

    private static readonly TimeSpan UpdateInterval = TimeSpan.FromSeconds(1);

    private readonly TimeProvider timeProvider;

    private readonly DispatcherTimer timer;

    private TimeSpan lastProcessorTime;

    private long lastTimestamp;

    [ObservableProperty]
    public partial string? LastMenuCommand { get; set; }

    [ObservableProperty]
    public partial bool ShowStatusBar { get; set; }

    [ObservableProperty]
    public partial bool WordWrap { get; set; }

    [ObservableProperty]
    public partial string Zoom { get; set; }

    [ObservableProperty]
    public partial string? LastContextCommand { get; set; }

    [ObservableProperty]
    public partial string SortBy { get; set; }

    [ObservableProperty]
    public partial bool ShowHiddenItems { get; set; }

    [ObservableProperty]
    public partial string? LastToolBarCommand { get; set; }

    [ObservableProperty]
    public partial bool IsBold { get; set; }

    [ObservableProperty]
    public partial bool IsItalic { get; set; }

    [ObservableProperty]
    public partial bool IsUnderline { get; set; }

    [ObservableProperty]
    public partial int TextSize { get; set; }

    [ObservableProperty]
    public partial double CpuUsage { get; set; }

    [ObservableProperty]
    public partial double MemoryUsage { get; set; }

    [ObservableProperty]
    public partial DateTimeOffset Now { get; set; }

    public IReadOnlyList<int> TextSizes { get; } = [12, DefaultTextSize, 16];

    public IObserveCommand MenuItemCommand { get; }

    public IObserveCommand ZoomCommand { get; }

    public IObserveCommand ContextItemCommand { get; }

    public IObserveCommand SortByCommand { get; }

    public IObserveCommand ToolBarItemCommand { get; }

    public MenusToolbarsViewModel(TimeProvider timeProvider)
    {
        this.timeProvider = timeProvider;

        ShowStatusBar = true;
        Zoom = DefaultZoom;
        SortBy = DefaultSortBy;
        TextSize = DefaultTextSize;

        SubscribeShowStatusBar(x => LastMenuCommand = $"Status bar ({ToText(x)})");
        SubscribeWordWrap(x => LastMenuCommand = $"Word wrap ({ToText(x)})");
        SubscribeShowHiddenItems(x => LastContextCommand = $"Show hidden items ({ToText(x)})");

        MenuItemCommand = MakeDelegateCommand<string>(x => LastMenuCommand = x);
        ZoomCommand = MakeDelegateCommand<string>(SelectZoom);
        ContextItemCommand = MakeDelegateCommand<string>(x => LastContextCommand = x);
        SortByCommand = MakeDelegateCommand<string>(SelectSortBy);
        ToolBarItemCommand = MakeDelegateCommand<string>(x => LastToolBarCommand = x);

        using (var process = Process.GetCurrentProcess())
        {
            lastProcessorTime = process.TotalProcessorTime;
            MemoryUsage = process.WorkingSet64 / BytesPerMegabyte;
        }
        lastTimestamp = timeProvider.GetTimestamp();
        Now = timeProvider.GetLocalNow();

        timer = new DispatcherTimer { Interval = UpdateInterval };
        timer.Tick += OnTick;
        timer.Start();
        Disposables.Add(new DelegateDisposable(() =>
        {
            timer.Stop();
            timer.Tick -= OnTick;
        }));
    }

    private static string ToText(bool value) => value ? On : Off;

    private void SelectZoom(string zoom)
    {
        Zoom = zoom;
        RaisePropertyChanged(nameof(Zoom));
        LastMenuCommand = $"Zoom {zoom} %";
    }

    private void SelectSortBy(string sortBy)
    {
        SortBy = sortBy;
        RaisePropertyChanged(nameof(SortBy));
        LastContextCommand = $"Sort by {sortBy}";
    }

    private void OnTick(object? sender, EventArgs e)
    {
        using var process = Process.GetCurrentProcess();
        var processorTime = process.TotalProcessorTime;
        var timestamp = timeProvider.GetTimestamp();
        var elapsed = timeProvider.GetElapsedTime(lastTimestamp, timestamp);
        if (elapsed > TimeSpan.Zero)
        {
            CpuUsage = (processorTime - lastProcessorTime).TotalMilliseconds * 100 / (elapsed.TotalMilliseconds * Environment.ProcessorCount);
        }

        lastProcessorTime = processorTime;
        lastTimestamp = timestamp;
        MemoryUsage = process.WorkingSet64 / BytesPerMegabyte;
        Now = timeProvider.GetLocalNow();
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("MenusToolbars:New", "New", "Menus & toolbars", "\uE8A5", MenuItemCommand, "New"),
        new("MenusToolbars:Open", "Open", "Menus & toolbars", "\uE8E5", MenuItemCommand, "Open"),
        new("MenusToolbars:Save", "Save", "Menus & toolbars", "\uE74E", MenuItemCommand, "Save")
    ];
}
