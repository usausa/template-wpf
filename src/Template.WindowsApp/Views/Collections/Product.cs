namespace Template.WindowsApp.Views.Collections;

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
