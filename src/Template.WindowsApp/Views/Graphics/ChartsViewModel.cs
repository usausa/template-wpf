namespace Template.WindowsApp.Views.Graphics;

using System.Security.Cryptography;

using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class ChartsViewModel : AppViewModelBase, IShellCommandProvider
{
    public IReadOnlyList<string> MonthLabels { get; } = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

    public IReadOnlyList<string> CategoryLabels { get; } = ["Hardware", "Software", "Services", "Support", "Training"];

    public IReadOnlyList<string> ShareLabels { get; } = ["North", "South", "East", "West"];

    [ObservableProperty]
    public partial IReadOnlyList<double> MonthValues { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<double> CategoryValues { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<double> ShareValues { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<double> ShareRatios { get; set; }

    public IObserveCommand RandomizeCommand { get; }

    public ChartsViewModel()
    {
        MonthValues = [1200, 1350, 1280, 1520, 1690, 1610, 1840, 1960, 1780, 2050, 2210, 2380];
        CategoryValues = [120, 95, 70, 55, 30];
        ShareValues = [42, 28, 18, 12];
        ShareRatios = MakeRatios(ShareValues);

        RandomizeCommand = MakeDelegateCommand(Randomize);
    }

    private static double[] MakeValues(int count, int min, int max)
    {
        var values = new double[count];
        for (var i = 0; i < count; i++)
        {
            values[i] = RandomNumberGenerator.GetInt32(min, max + 1);
        }
        return values;
    }

    private static double[] MakeRatios(IReadOnlyList<double> values)
    {
        var total = values.Sum();
        return values.Select(x => x / total).ToArray();
    }

    private void Randomize()
    {
        MonthValues = MakeValues(MonthLabels.Count, 1000, 3000);
        CategoryValues = MakeValues(CategoryLabels.Count, 20, 150);
        ShareValues = MakeValues(ShareLabels.Count, 10, 100);
        ShareRatios = MakeRatios(ShareValues);
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("Charts:Randomize", "Randomize", "Charts", "\uE8B1", RandomizeCommand)
    ];
}
