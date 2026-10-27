namespace Template.WindowsApp.Views.Graphics;

using Template.WindowsApp.Models.Sample;
using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class LiveDataViewModel : AppViewModelBase, IShellCommandProvider
{
    private static readonly TimeSpan TickInterval = TimeSpan.FromMilliseconds(50);

    private static readonly TimeSpan DrawInterval = TimeSpan.FromMilliseconds(250);

    private static readonly TimeSpan StaleTime = TimeSpan.FromSeconds(3);

    private static readonly TimeSpan DropTime = TimeSpan.FromSeconds(5);

    private readonly LiveSensor humidity;

    private double budget;

    private long received;

    private long drawn;

    public IReadOnlyList<int> Rates { get; } = [10, 100, 1000];

    public IReadOnlyList<LiveSensor> Sensors { get; }

    [ObservableProperty]
    public partial int SelectedRate { get; set; }

    [ObservableProperty]
    public partial bool IsLimited { get; set; }

    [ObservableProperty]
    public partial bool IsPaused { get; set; }

    [ObservableProperty]
    public partial bool IsDropping { get; set; }

    [ObservableProperty]
    public partial long ReceivedCount { get; set; }

    [ObservableProperty]
    public partial long DrawnCount { get; set; }

    public IObserveCommand PauseCommand { get; }

    public IObserveCommand DropCommand { get; }

    public LiveDataViewModel()
    {
        humidity = SensorSample.LoadHumidity();
        Sensors =
        [
            SensorSample.LoadTemperature(),
            humidity,
            SensorSample.LoadPressure(),
            SensorSample.LoadVibration()
        ];
        SelectedRate = 100;
        IsLimited = true;

        PauseCommand = MakeDelegateCommand(() => IsPaused = !IsPaused);
        DropCommand = MakeDelegateCommand(Drop, () => !IsDropping);

        var values = Observable.Interval(TickInterval).SelectMany(_ => Generate()).Publish();
        Disposables.Add(ObserveIsLimited()
            .StartWith(IsLimited)
            .Select(x => x ? values.GroupBy(static y => y.Sensor).SelectMany(static y => y.Sample(DrawInterval)) : values)
            .Switch()
            .ObserveOnDispatcher()
            .Subscribe(x => Draw(x.Sensor, x.Value)));
        Disposables.Add(values
            .GroupBy(static x => x.Sensor)
            .SelectMany(static x => x.Sample(DrawInterval).Throttle(StaleTime))
            .ObserveOnDispatcher()
            .Subscribe(static x => x.Sensor.IsStale = true));
        Disposables.Add(values.Connect());

        Disposables.Add(Observable.Interval(DrawInterval).ObserveOnDispatcher().Subscribe(_ => UpdateCounts()));
        Disposables.Add(ObserveIsDropping()
            .Where(static x => x)
            .Delay(DropTime)
            .ObserveOnDispatcher()
            .Subscribe(_ => Restore()));
    }

    private List<(LiveSensor Sensor, double Value)> Generate()
    {
        budget += SelectedRate * TickInterval.TotalSeconds;
        var count = (int)budget;
        budget -= count;

        var values = new List<(LiveSensor Sensor, double Value)>();
        foreach (var sensor in Sensors)
        {
            if (sensor.IsStopped)
            {
                continue;
            }

            for (var i = 0; i < count; i++)
            {
                values.Add((sensor, sensor.Next()));
            }
        }
        Interlocked.Add(ref received, values.Count);
        return values;
    }

    private void Draw(LiveSensor sensor, double value)
    {
        sensor.IsStale = false;
        if (IsPaused)
        {
            return;
        }

        sensor.Show(value);
        drawn++;
    }

    private void UpdateCounts()
    {
        ReceivedCount = Interlocked.Read(ref received);
        DrawnCount = drawn;
    }

    private void Drop()
    {
        humidity.IsStopped = true;
        IsDropping = true;
    }

    private void Restore()
    {
        humidity.IsStopped = false;
        IsDropping = false;
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("LiveData:Pause", IsPaused ? "Resume" : "Pause", "Live data", IsPaused ? "\uE768" : "\uE769", PauseCommand),
        new("LiveData:StopHumidity", "Stop Humidity for 5 s", "Live data", "\uE71A", DropCommand)
    ];
}
