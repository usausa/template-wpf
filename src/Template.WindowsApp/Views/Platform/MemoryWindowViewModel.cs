namespace Template.WindowsApp.Views.Platform;

using Template.WindowsApp.Services;

[ObservableGeneratorOption(Reactive = true, ViewModel = true)]
public sealed partial class MemoryWindowViewModel : ExtendViewModelBase
{
    private readonly ProcessMonitorService monitor;

    [ObservableProperty]
    public partial string Memory { get; set; }

    [ObservableProperty]
    public partial string PeakMemory { get; set; }

    public IReadOnlyList<string> MemoryLabels { get; } =
        Enumerable.Range(0, ProcessMonitorService.Capacity)
            .Select(static x => x == 0 ? "60 s ago" : x == ProcessMonitorService.Capacity - 1 ? "Now" : string.Empty)
            .ToArray();

    public ObservableCollection<double> MemoryValues { get; } = [];

    public MemoryWindowViewModel(ProcessMonitorService monitor)
    {
        this.monitor = monitor;

        Memory = string.Empty;
        PeakMemory = string.Empty;

        foreach (var value in monitor.Snapshot())
        {
            MemoryValues.Add(value);
        }
        Update();
        Disposables.Add(monitor.Sampled.Subscribe(OnSampled));
    }

    private void OnSampled(double value)
    {
        MemoryValues.Add(value);
        if (MemoryValues.Count > ProcessMonitorService.Capacity)
        {
            MemoryValues.RemoveAt(0);
        }
        Update();
    }

    private void Update()
    {
        Memory = String.Create(CultureInfo.CurrentCulture, $"{monitor.Memory:0} MB");
        PeakMemory = String.Create(CultureInfo.CurrentCulture, $"Peak {monitor.PeakMemory:0} MB");
    }
}
