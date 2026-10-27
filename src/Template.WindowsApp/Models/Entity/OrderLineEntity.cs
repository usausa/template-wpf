namespace Template.WindowsApp.Models.Entity;

using Smart.Data.Accessor.Attributes;

using Template.WindowsApp.Models.Sample;

[Name("OrderLine")]
public sealed class OrderLineEntity
{
    [Key]
    public int LineNo { get; set; }

    public string Code { get; set; } = default!;

    public string Description { get; set; } = default!;

    public int? Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public int? Discount { get; set; }

    public DateTime? Delivery { get; set; }

    public OrderStatus Status { get; set; }

    public string Note { get; set; } = default!;
}
