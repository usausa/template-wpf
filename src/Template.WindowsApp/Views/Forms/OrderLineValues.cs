namespace Template.WindowsApp.Views.Forms;

public readonly record struct OrderLineValues(
    string Code,
    string Description,
    int? Quantity,
    decimal? UnitPrice,
    int? Discount,
    DateTime? Delivery,
    OrderStatus Status,
    string Note);
