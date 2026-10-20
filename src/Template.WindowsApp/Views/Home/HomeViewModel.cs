namespace Template.WindowsApp.Views.Home;

using Template.WindowsApp.Services;
using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class HomeViewModel : AppViewModelBase
{
    [ObservableProperty]
    public partial string Message { get; set; }

    public IReadOnlyList<Summary> Summaries { get; } =
    [
        new("Orders", "128", "+12% from last week"),
        new("Sales", "$12,340", "+8% from last week"),
        new("Customers", "56", "+4 this week"),
        new("Alerts", "3", "1 new today")
    ];

    public IReadOnlyList<string> SalesLabels { get; } = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];

    public IReadOnlyList<double> SalesValues { get; } = [1320, 1540, 1480, 1910, 1750, 2290, 2050];

    public IReadOnlyList<string> CategoryLabels { get; } = ["Hardware", "Software", "Services", "Support", "Training"];

    public IReadOnlyList<double> CategoryValues { get; } = [120, 95, 70, 55, 30];

    public IReadOnlyList<SampleEntry> Samples { get; }

    public IObserveCommand ForwardCommand { get; }

    public HomeViewModel(GreetService greetService, NavigationMenu menu)
    {
        Message = greetService.MakeMessage();
        Samples = menu.Groups
            .Select(static x => new SampleEntry(x.Title, x.Icon, string.Join(" · ", x.Items.Select(static y => y.Title)), x.Items[0].Id))
            .ToList();
        ForwardCommand = MakeAsyncCommand<ViewId>(x => Navigator.ForwardAsync(x));
    }
}
