namespace Template.WindowsApp.Views.Collections;

using Template.WindowsApp.Behaviors;
using Template.WindowsApp.Settings;
using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class DataGridViewModel : AppViewModelBase, IShellCommandProvider
{
    private const int PageSize = 15;

    private const string LayoutKey = "Products";

    private const int FrozenCount = 2;

    private static readonly string[][] ProductNames =
    [
        ["Keyboard", "Mouse", "Monitor", "Headset", "Webcam", "Docking station", "Router", "Printer", "Scanner", "Speaker"],
        ["Office suite", "Antivirus", "Photo editor", "Video editor", "Backup tool", "PDF editor", "Password manager", "VPN client", "Code editor", "Database tool"],
        ["Installation", "Data migration", "Cloud backup", "Network setup", "Security audit", "Repair", "Recycling", "Consulting", "Hosting", "Monitoring"],
        ["Basic support", "Standard support", "Premium support", "Extended warranty", "Remote assistance", "On-site visit", "Priority line", "Health check", "Upgrade plan", "Replacement plan"],
        ["Excel course", "Word course", "Security course", "Cloud course", "Network course", "Python course", "SQL course", "Design course", "Project course", "Leadership course"]
    ];

    private static readonly string[] Suppliers = ["Contoso", "Fabrikam", "Northwind Traders", "Litware", "Proseware", "Adventure Works"];

    private static readonly string[] Warehouses = ["Tokyo", "Osaka", "Nagoya", "Fukuoka"];

    private static readonly DateTime LastUpdate = new(2026, 10, 1);

    private readonly IWindowManager windowManager;

    private List<Product> products;

    private int pageIndex;

    public static IReadOnlyList<string> Categories { get; } = ["Hardware", "Software", "Services", "Support", "Training"];

    public ObservableCollection<Product> Items { get; } = [];

    public IReadOnlyList<DataGridColumnSetting> Columns { get; } =
    [
        new("Code", false),
        new("Name", false),
        new("Category", true),
        new("Supplier", true),
        new("Warehouse", true),
        new("Price", true),
        new("Stock", true),
        new("Updated", true),
        new("Active", true)
    ];

    [ObservableProperty]
    public partial string SearchText { get; set; }

    [ObservableProperty]
    public partial SortDescription? Sort { get; set; }

    [ObservableProperty]
    public partial int RangeStart { get; set; }

    [ObservableProperty]
    public partial int RangeEnd { get; set; }

    [ObservableProperty]
    public partial int TotalCount { get; set; }

    [ObservableProperty(NotifyAlso = [nameof(FrozenColumnCount)])]
    public partial bool IsFrozen { get; set; }

    public int FrozenColumnCount => IsFrozen ? FrozenCount : 0;

    public IObserveCommand PreviousCommand { get; }

    public IObserveCommand NextCommand { get; }

    public IObserveCommand ResetCommand { get; }

    public IObserveCommand ResetColumnsCommand { get; }

    public DataGridViewModel(IWindowManager windowManager)
    {
        this.windowManager = windowManager;
        products = MakeProducts();
        SearchText = string.Empty;
        IsFrozen = true;
        RestoreLayout();

        SubscribeSearchText(_ => ShowPage(0));
        SubscribeSort(_ => ShowPage(0));
        SubscribeIsFrozen(_ => SaveLayout());
        foreach (var column in Columns)
        {
            column.PropertyChanged += OnColumnChanged;
        }

        PreviousCommand = MakeDelegateCommand(() => ShowPage(pageIndex - 1), () => pageIndex > 0);
        NextCommand = MakeDelegateCommand(() => ShowPage(pageIndex + 1), () => RangeEnd < TotalCount);
        ResetCommand = MakeDelegateCommand(Reset);
        ResetColumnsCommand = MakeDelegateCommand(ResetColumns);

        ShowPage(0);
    }

    private static List<Product> MakeProducts()
    {
        var list = new List<Product>();
        for (var c = 0; c < Categories.Count; c++)
        {
            for (var i = 0; i < ProductNames[c].Length; i++)
            {
                var index = list.Count;
                list.Add(new Product(
                    $"P-{index + 1:D3}",
                    ProductNames[c][i],
                    Categories[c],
                    Suppliers[((index * 5) + c) % Suppliers.Length],
                    Warehouses[index % Warehouses.Length],
                    ((c + 1) * 20) + (((i * 7) % 10) * 12.5m),
                    c < 2 ? ((i * 37) + (c * 11)) % 120 : null,
                    LastUpdate.AddDays(-((index * 13) % 60)),
                    index % 7 != 6));
            }
        }
        return list;
    }

    private void ShowPage(int index)
    {
        var rows = Query().ToList();
        pageIndex = index;
        Items.Clear();
        foreach (var product in rows.Skip(index * PageSize).Take(PageSize))
        {
            Items.Add(product);
        }

        TotalCount = rows.Count;
        RangeStart = Items.Count > 0 ? (index * PageSize) + 1 : 0;
        RangeEnd = (index * PageSize) + Items.Count;
    }

    private IEnumerable<Product> Query()
    {
        var query = products.Where(Matches);
        if (Sort is not { } sort)
        {
            return query;
        }

        Func<Product, object?> key = sort.PropertyName switch
        {
            nameof(Product.Name) => static x => x.Name,
            nameof(Product.Category) => static x => x.Category,
            nameof(Product.Supplier) => static x => x.Supplier,
            nameof(Product.Warehouse) => static x => x.Warehouse,
            nameof(Product.Updated) => static x => x.Updated,
            nameof(Product.Price) => static x => x.Price,
            nameof(Product.Stock) => static x => x.Stock,
            nameof(Product.IsActive) => static x => x.IsActive,
            _ => static x => x.Code
        };
        return sort.Direction == ListSortDirection.Ascending ? query.OrderBy(key) : query.OrderByDescending(key);
    }

    private bool Matches(Product product) =>
        (SearchText.Length == 0) ||
        product.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
        product.Code.Contains(SearchText, StringComparison.OrdinalIgnoreCase);

    private void RestoreLayout()
    {
        if (!windowManager.GridLayouts.TryGetValue(LayoutKey, out var layout))
        {
            return;
        }

        IsFrozen = layout.FrozenColumnCount > 0;
        foreach (var saved in layout.Columns)
        {
            if (Columns.FirstOrDefault(x => x.Header == saved.Key) is { } column)
            {
                column.IsVisible = saved.IsVisible || !column.CanHide;
                column.Width = saved.Width;
            }
        }
    }

    private void SaveLayout() =>
        windowManager.GridLayouts[LayoutKey] = new GridLayout
        {
            FrozenColumnCount = FrozenColumnCount,
            Columns = Columns.Select(static x => new GridColumnLayout { Key = x.Header, IsVisible = x.IsVisible, Width = x.Width }).ToList()
        };

    private void OnColumnChanged(object? sender, PropertyChangedEventArgs e) => SaveLayout();

    private void ResetColumns()
    {
        foreach (var column in Columns)
        {
            column.IsVisible = true;
            column.Width = null;
        }
        IsFrozen = true;
    }

    private void Reset()
    {
        products = MakeProducts();
        SearchText = string.Empty;
        Sort = null;
        ShowPage(0);
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("DataGrid:Reset", "Reset", "Data grid", "\uE72C", ResetCommand),
        new("DataGrid:ResetColumns", "Reset columns", "Data grid", "\uE72C", ResetColumnsCommand)
    ];
}
