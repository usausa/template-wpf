namespace Template.WindowsApp.Views.BasicInput;

using Template.WindowsApp.Models;
using Template.WindowsApp.Shell;

public sealed record CustomerEntry(string Name, string Company, string City)
{
    public override string ToString() => Name;
}

public sealed record CustomerSuggestion(CustomerEntry Customer, IReadOnlyList<HighlightRun> NameRuns, IReadOnlyList<HighlightRun> CompanyRuns)
{
    public override string ToString() => Customer.Name;
}

public sealed record CitySuggestion(string Name, IReadOnlyList<HighlightRun> NameRuns)
{
    public override string ToString() => Name;
}

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class TextViewModel : AppViewModelBase, IShellCommandProvider
{
    private const double DefaultNumber = 10;

    private const CompareOptions MatchOptions = CompareOptions.IgnoreCase | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth;

    private static readonly CompareInfo Compare = CultureInfo.InvariantCulture.CompareInfo;

    private static readonly string[] Cities =
    [
        "Amsterdam", "Athens", "Bangkok", "Barcelona", "Beijing", "Berlin", "Boston", "Brussels", "Buenos Aires",
        "Cairo", "Chicago", "Copenhagen", "Dubai", "Dublin", "Fukuoka", "Hong Kong", "Istanbul", "Kyoto", "Lisbon",
        "London", "Los Angeles", "Madrid", "Melbourne", "Mexico City", "Milan", "Montreal", "Mumbai", "Munich",
        "Nagoya", "New York", "Osaka", "Paris", "Prague", "Rome", "San Francisco", "Sapporo", "Seattle", "Seoul",
        "Shanghai", "Singapore", "Stockholm", "Sydney", "Taipei", "Tokyo", "Toronto", "Vancouver", "Vienna",
        "Yokohama", "Zurich"
    ];

    private static readonly CustomerEntry[] Customers =
    [
        new("Ava Brown", "Contoso", "Seattle"),
        new("Ben Carter", "Fabrikam", "Chicago"),
        new("Chloe Davis", "Northwind Traders", "London"),
        new("Daniel Evans", "Adventure Works", "Toronto"),
        new("Emma Wilson", "Tailspin Toys", "Sydney"),
        new("Hana Sato", "Contoso", "Tokyo"),
        new("Ivan Petrov", "Wide World Importers", "Berlin"),
        new("Julia Novak", "Litware", "Prague"),
        new("Kenji Tanaka", "Fabrikam", "Osaka"),
        new("Liam Murphy", "Woodgrove Bank", "Dublin"),
        new("Maria Garcia", "Proseware", "Madrid"),
        new("Noah Kim", "Fourth Coffee", "Seoul"),
        new("Olivia Martin", "Alpine Ski House", "Vancouver"),
        new("Priya Patel", "Northwind Traders", "Mumbai"),
        new("Sofia Rossi", "Coho Winery", "Milan"),
        new("Yuki Mori", "Trey Research", "Sapporo")
    ];

    [ObservableProperty]
    public partial string SingleLineText { get; set; }

    [ObservableProperty]
    public partial string MultiLineText { get; set; }

    [ObservableProperty]
    public partial int LineCount { get; set; }

    [ObservableProperty]
    public partial double? Number { get; set; }

    [ObservableProperty]
    public partial string CityText { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<CitySuggestion> CitySuggestions { get; set; }

    [ObservableProperty]
    public partial string? SelectedCity { get; set; }

    [ObservableProperty]
    public partial string CustomerText { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<CustomerSuggestion> CustomerSuggestions { get; set; }

    [ObservableProperty]
    public partial CustomerEntry? SelectedCustomer { get; set; }

    [ObservableProperty]
    public partial string? CustomerQuery { get; set; }

    public EventRequest ClearPasswordRequest { get; } = new();

    public IObserveCommand SubmitCityCommand { get; }

    public IObserveCommand SubmitCustomerCommand { get; }

    public IObserveCommand ResetCommand { get; }

    public TextViewModel()
    {
        SingleLineText = string.Empty;
        MultiLineText = string.Empty;
        LineCount = CountLines(MultiLineText);
        Number = DefaultNumber;
        CityText = string.Empty;
        CitySuggestions = [];
        CustomerText = string.Empty;
        CustomerSuggestions = [];

        SubscribeMultiLineText(x => LineCount = CountLines(x));
        SubscribeCityText(x => CitySuggestions = SuggestCities(x));
        SubscribeCustomerText(x => CustomerSuggestions = SuggestCustomers(x));

        SubmitCityCommand = MakeDelegateCommand<AutoSuggestBoxQuery>(SubmitCity);
        SubmitCustomerCommand = MakeDelegateCommand<AutoSuggestBoxQuery>(SubmitCustomer);
        ResetCommand = MakeDelegateCommand(Reset);
    }

    private static int CountLines(string text) => text.AsSpan().Count('\n') + 1;

    private void SubmitCity(AutoSuggestBoxQuery query)
    {
        var city = query.ChosenSuggestion as CitySuggestion ?? (CitySuggestions.Count > 0 ? CitySuggestions[0] : null);
        if (city is not null)
        {
            CityText = city.Name;
        }
        SelectedCity = city?.Name;
    }

    private void SubmitCustomer(AutoSuggestBoxQuery query)
    {
        var customer = (query.ChosenSuggestion as CustomerSuggestion ?? (CustomerSuggestions.Count > 0 ? CustomerSuggestions[0] : null))?.Customer;
        if (customer is not null)
        {
            CustomerText = customer.Name;
        }
        CustomerQuery = query.QueryText;
        SelectedCustomer = customer;
    }

    private void Reset()
    {
        SingleLineText = string.Empty;
        MultiLineText = string.Empty;
        Number = DefaultNumber;
        CityText = string.Empty;
        SelectedCity = null;
        CustomerText = string.Empty;
        SelectedCustomer = null;
        CustomerQuery = null;
        ClearPasswordRequest.Request();
    }

    private static List<CitySuggestion> SuggestCities(string text)
    {
        var words = SplitWords(text);
        if (words.Length == 0)
        {
            return [];
        }

        var list = new List<(int Rank, CitySuggestion Suggestion)>();
        foreach (var city in Cities)
        {
            var marks = new bool[city.Length];
            if (words.All(x => Mark(city, x, marks)))
            {
                list.Add((Rank(city, words[0]), new CitySuggestion(city, HighlightRun.Split(city, marks))));
            }
        }

        return list.OrderBy(static x => x.Rank).ThenBy(static x => x.Suggestion.Name, StringComparer.CurrentCulture).Select(static x => x.Suggestion).ToList();
    }

    private static List<CustomerSuggestion> SuggestCustomers(string text)
    {
        var words = SplitWords(text);
        if (words.Length == 0)
        {
            return [];
        }

        var list = new List<(int Rank, CustomerSuggestion Suggestion)>();
        foreach (var customer in Customers)
        {
            var nameMarks = new bool[customer.Name.Length];
            var companyMarks = new bool[customer.Company.Length];
            var matched = words.All(x =>
            {
                var inName = Mark(customer.Name, x, nameMarks);
                var inCompany = Mark(customer.Company, x, companyMarks);
                return inName || inCompany;
            });
            if (matched)
            {
                var rank = Math.Min(Rank(customer.Name, words[0]), Rank(customer.Company, words[0]));
                list.Add((rank, new CustomerSuggestion(customer, HighlightRun.Split(customer.Name, nameMarks), HighlightRun.Split(customer.Company, companyMarks))));
            }
        }

        return list.OrderBy(static x => x.Rank).ThenBy(static x => x.Suggestion.Customer.Name, StringComparer.CurrentCulture).Select(static x => x.Suggestion).ToList();
    }

    private static string[] SplitWords(string text) =>
        text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

    private static bool Mark(string text, string word, bool[] marks)
    {
        var index = Compare.IndexOf(text.AsSpan(), word.AsSpan(), MatchOptions, out var length);
        if (index < 0)
        {
            return false;
        }

        marks.AsSpan(index, length).Fill(true);
        return true;
    }

    private static int Rank(string text, string word)
    {
        if (Compare.IsPrefix(text, word, MatchOptions))
        {
            return 0;
        }

        var index = Compare.IndexOf(text, word, MatchOptions);
        return (index > 0) && !Char.IsLetterOrDigit(text[index - 1]) ? 1 : 2;
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("Text:Reset", "Reset", "Text", "\uE72C", ResetCommand)
    ];
}
