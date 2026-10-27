namespace Template.WindowsApp.Models.Sample;

using System.Security.Cryptography;

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed partial class LiveSensor : ObservableObject
{
    public const int Capacity = 60;

    private const double Reversion = 0.02;

    private const int Resolution = 1000;

    private readonly double center;

    private readonly double step;

    private readonly string format;

    private double current;

    private double? shown;

    private volatile bool stopped;

    public string Name { get; }

    public string Unit { get; }

    public double Minimum { get; }

    public double Maximum { get; }

    public bool IsStopped
    {
        get => stopped;
        set => stopped = value;
    }

    public ObservableCollection<double> Values { get; } = [];

    [ObservableProperty]
    public partial string ValueText { get; set; }

    [ObservableProperty]
    public partial string DeltaText { get; set; }

    [ObservableProperty]
    public partial bool IsRising { get; set; }

    [ObservableProperty]
    public partial bool IsStale { get; set; }

    public LiveSensor(string name, string unit, double center, double step, double minimum, double maximum, string format)
    {
        Name = name;
        Unit = unit;
        this.center = center;
        this.step = step;
        Minimum = minimum;
        Maximum = maximum;
        this.format = format;
        current = center;
        ValueText = string.Empty;
        DeltaText = string.Empty;
    }

    public double Next()
    {
        var noise = RandomNumberGenerator.GetInt32(-Resolution, Resolution + 1) * step / Resolution;
        current = Math.Clamp(current + ((center - current) * Reversion) + noise, Minimum, Maximum);
        return current;
    }

    public void Show(double value)
    {
        ValueText = value.ToString(format, CultureInfo.CurrentCulture);
        if (shown is { } previous)
        {
            IsRising = value >= previous;
            DeltaText = Math.Abs(value - previous).ToString(format, CultureInfo.CurrentCulture);
        }
        shown = value;

        Values.Add(value);
        if (Values.Count > Capacity)
        {
            Values.RemoveAt(0);
        }
    }
}

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

public static class SensorSample
{
    public static LiveSensor LoadTemperature() => new("Temperature", "°C", 23, 0.05, 20, 26, "0.0");

    public static LiveSensor LoadHumidity() => new("Humidity", "%", 45, 0.2, 30, 60, "0.0");

    public static LiveSensor LoadPressure() => new("Pressure", "hPa", 1013, 0.1, 1005, 1020, "0.0");

    public static LiveSensor LoadVibration() => new("Vibration", "mm/s", 2, 0.05, 0, 4, "0.00");
}
