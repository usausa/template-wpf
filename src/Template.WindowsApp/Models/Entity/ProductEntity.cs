namespace Template.WindowsApp.Models.Entity;

using Smart.Data.Accessor.Attributes;

[Name("Product")]
public sealed class ProductEntity
{
    [Key]
    public string Code { get; set; } = default!;

    public string Name { get; set; } = default!;

    public string Category { get; set; } = default!;

    public string Supplier { get; set; } = default!;

    public string Warehouse { get; set; } = default!;

    public decimal Price { get; set; }

    public int? Stock { get; set; }

    public DateTime Updated { get; set; }

    public bool IsActive { get; set; }
}
