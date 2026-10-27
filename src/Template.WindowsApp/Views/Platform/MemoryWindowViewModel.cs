namespace Template.WindowsApp.Views.Platform;

using Template.WindowsApp.Components;

[ObservableGeneratorOption(Reactive = true, ViewModel = true)]
public sealed partial class MemoryWindowViewModel : ExtendViewModelBase
{
    private readonly ProcessMonitor processMonitor;

    [ObservableProperty]
    public partial string Memory { get; set; }

    [ObservableProperty]
    public partial string PeakMemory { get; set; }

    public IReadOnlyList<string> MemoryLabels { get; } =
        Enumerable.Range(0, ProcessMonitor.Capacity)
            .Select(static x => x == 0 ? "60 s ago" : x == ProcessMonitor.Capacity - 1 ? "Now" : string.Empty)
            .ToArray();

    public ObservableCollection<double> MemoryValues { get; } = [];

    public MemoryWindowViewModel(ProcessMonitor processMonitor)
    {
        this.processMonitor = processMonitor;

        Memory = string.Empty;
        PeakMemory = string.Empty;

        foreach (var value in processMonitor.Snapshot())
        {
            MemoryValues.Add(value);
        }
        Update();
        Disposables.Add(processMonitor.Sampled.Subscribe(OnSampled));
    }

    private void OnSampled(double value)
    {
        MemoryValues.Add(value);
        if (MemoryValues.Count > ProcessMonitor.Capacity)
        {
            MemoryValues.RemoveAt(0);
        }
        Update();
    }

    private void Update()
    {
        Memory = String.Create(CultureInfo.CurrentCulture, $"{processMonitor.Memory:0} MB");
        PeakMemory = String.Create(CultureInfo.CurrentCulture, $"Peak {processMonitor.PeakMemory:0} MB");
    }
}
