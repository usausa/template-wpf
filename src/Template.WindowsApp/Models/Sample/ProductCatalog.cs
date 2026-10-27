namespace Template.WindowsApp.Models.Sample;

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed partial class Product : ObservableObject
{
    public string Code { get; }

    public string Supplier { get; }

    public string Warehouse { get; }

    public DateTime Updated { get; }

    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial string Category { get; set; }

    [ObservableProperty]
    public partial decimal Price { get; set; }

    [ObservableProperty]
    public partial int? Stock { get; set; }

    [ObservableProperty]
    public partial bool IsActive { get; set; }

    public Product(string code, string name, string category, string supplier, string warehouse, decimal price, int? stock, DateTime updated, bool isActive)
    {
        Code = code;
        Supplier = supplier;
        Warehouse = warehouse;
        Updated = updated;
        Name = name;
        Category = category;
        Price = price;
        Stock = stock;
        IsActive = isActive;
    }
}

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

public static class ProductSample
{
    private static readonly string[] Categories = ["Hardware", "Software", "Services", "Support", "Training"];

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

    public static IReadOnlyList<string> LoadCategories() => Categories;

    public static IReadOnlyList<Product> LoadProducts()
    {
        var list = new List<Product>();
        for (var c = 0; c < Categories.Length; c++)
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
}
