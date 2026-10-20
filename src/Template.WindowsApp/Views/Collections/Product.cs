namespace Template.WindowsApp.Views.Collections;

public sealed partial class Product : ObservableObject
{
    public string Code { get; }

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

    public Product(string code, string name, string category, decimal price, int? stock, bool isActive)
    {
        Code = code;
        Name = name;
        Category = category;
        Price = price;
        Stock = stock;
        IsActive = isActive;
    }
}
