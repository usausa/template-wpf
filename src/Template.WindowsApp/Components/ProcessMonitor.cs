namespace Template.WindowsApp.Components;

using System.Diagnostics;
using System.Windows.Threading;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ProcessMonitor : IDisposable
{
    public const int Capacity = 60;

    private const double BytesPerMegabyte = 1024 * 1024;

    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);

    private readonly Queue<double> history = new(Capacity + 1);

    private readonly Subject<double> sampled = new();

    private readonly DispatcherTimer timer;

    public DateTimeOffset StartTime { get; }

    public double Memory { get; private set; }

    public double PeakMemory { get; private set; }

    public IObservable<double> Sampled => sampled;

    public ProcessMonitor()
    {
        using (var process = Process.GetCurrentProcess())
        {
            StartTime = new DateTimeOffset(process.StartTime);
            Sample(process);
        }

        timer = new DispatcherTimer { Interval = Interval };
        timer.Tick += OnTick;
        timer.Start();
    }

    public void Dispose()
    {
        timer.Stop();
        timer.Tick -= OnTick;
        sampled.Dispose();
    }

    public IReadOnlyList<double> Snapshot() => history.ToArray();

    private void OnTick(object? sender, EventArgs e)
    {
        using var process = Process.GetCurrentProcess();
        Sample(process);
        sampled.OnNext(Memory);
    }

    private void Sample(Process process)
    {
        Memory = process.WorkingSet64 / BytesPerMegabyte;
        PeakMemory = process.PeakWorkingSet64 / BytesPerMegabyte;
        history.Enqueue(Memory);
        if (history.Count > Capacity)
        {
            history.Dequeue();
        }
    }
}
