namespace Template.WindowsApp.Models.Sample;

using Template.WindowsApp.Models.Entity;

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

    public static IReadOnlyList<ProductEntity> LoadProducts()
    {
        var list = new List<ProductEntity>();
        for (var c = 0; c < Categories.Length; c++)
        {
            for (var i = 0; i < ProductNames[c].Length; i++)
            {
                var index = list.Count;
                list.Add(new ProductEntity
                {
                    Code = $"P-{index + 1:D3}",
                    Name = ProductNames[c][i],
                    Category = Categories[c],
                    Supplier = Suppliers[((index * 5) + c) % Suppliers.Length],
                    Warehouse = Warehouses[index % Warehouses.Length],
                    Price = ((c + 1) * 20) + (((i * 7) % 10) * 12.5m),
                    Stock = c < 2 ? ((i * 37) + (c * 11)) % 120 : null,
                    Updated = LastUpdate.AddDays(-((index * 13) % 60)),
                    IsActive = index % 7 != 6
                });
            }
        }
        return list;
    }
}
