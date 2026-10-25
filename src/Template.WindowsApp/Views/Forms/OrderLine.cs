namespace Template.WindowsApp.Views.Forms;

using System.Text.RegularExpressions;

public sealed partial class OrderLine : ObservableObject, INotifyDataErrorInfo, IEditableObject
{
    private const int CodeLength = 10;

    private const int DescriptionLength = 40;

    private const int NoteLength = 100;

    private const int MinQuantity = 1;

    private const int MaxQuantity = 999;

    private const decimal MinUnitPrice = 0.01m;

    private const decimal MaxUnitPrice = 99999.99m;

    private const int PriceDecimals = 2;

    private const int MaxDiscount = 50;

    private const int NoteDiscount = 20;

    private readonly TimeProvider timeProvider;

    private readonly HashSet<string> touched = [];

    private Dictionary<string, string> errors = [];

    private OrderLineValues? saved;

    private OrderLineValues? editing;

    private bool committed;

    private bool revealed;

    private readonly bool initialized;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public event EventHandler? Discarded;

    [ObservableProperty]
    public partial string Code { get; set; }

    [ObservableProperty]
    public partial string Description { get; set; }

    [ObservableProperty(NotifyAlso = [nameof(Amount)])]
    public partial int? Quantity { get; set; }

    [ObservableProperty(NotifyAlso = [nameof(Amount)])]
    public partial decimal? UnitPrice { get; set; }

    [ObservableProperty(NotifyAlso = [nameof(Amount)])]
    public partial int? Discount { get; set; }

    [ObservableProperty]
    public partial DateTime? Delivery { get; set; }

    [ObservableProperty]
    public partial OrderStatus Status { get; set; }

    [ObservableProperty]
    public partial string Note { get; set; }

    [ObservableProperty]
    public partial int Revision { get; set; }

    public decimal? Amount => Quantity * UnitPrice * (100 - Discount) / 100 is { } amount ? Math.Round(amount, PriceDecimals) : null;

    public OrderLineValues Values => new(Code, Description, Quantity, UnitPrice, Discount, Delivery, Status, Note);

    public bool IsNew => saved is null;

    public bool IsLocked => saved?.Status == OrderStatus.Shipped;

    public bool IsEditing => editing is not null;

    public bool IsChanged => saved != Values;

    public bool HasErrors => errors.Count > 0;

    public string ErrorText => String.Join(Environment.NewLine, errors.Values.Distinct());

    private DateTime Today => timeProvider.GetLocalNow().Date;

    private OrderLine(OrderLineValues values, OrderLineValues? saved, TimeProvider timeProvider)
    {
        this.timeProvider = timeProvider;
        this.saved = saved;
        committed = saved is not null;
        Code = values.Code;
        Description = values.Description;
        Quantity = values.Quantity;
        UnitPrice = values.UnitPrice;
        Discount = values.Discount;
        Delivery = values.Delivery;
        Status = values.Status;
        Note = values.Note;
        initialized = true;
        Validate();
    }

    public static OrderLine Load(OrderLineValues values, TimeProvider timeProvider) => new(values, values, timeProvider);

    public static OrderLine Create(OrderLineValues values, TimeProvider timeProvider) => new(values, null, timeProvider);

    public void BeginEdit() => editing ??= Values;

    public void EndEdit()
    {
        editing = null;
        committed = true;
    }

    public void CancelEdit()
    {
        if (editing is not { } values)
        {
            return;
        }

        editing = null;
        Apply(values);
        if (!committed)
        {
            Discarded?.Invoke(this, EventArgs.Empty);
        }
    }

    public void AcceptChanges()
    {
        saved = Values;
        committed = true;
        revealed = false;
        touched.Clear();
        Update();
    }

    public void RevealErrors()
    {
        if (!revealed)
        {
            revealed = true;
            Update();
        }
    }

    public override string ToString() => Code;

    public IEnumerable GetErrors(string? propertyName)
    {
        string[] result = String.IsNullOrEmpty(propertyName) ? [.. errors.Values] : errors.TryGetValue(propertyName, out var message) ? [message] : [];
        return result;
    }

    public string? GetCellError(string key) => errors.GetValueOrDefault(key);

    public bool IsCellChanged(string key) =>
        saved is { } values && key switch
        {
            nameof(Code) => values.Code != Code,
            nameof(Description) => values.Description != Description,
            nameof(Quantity) => values.Quantity != Quantity,
            nameof(UnitPrice) => values.UnitPrice != UnitPrice,
            nameof(Discount) => values.Discount != Discount,
            nameof(Delivery) => values.Delivery != Delivery,
            nameof(Status) => values.Status != Status,
            nameof(Note) => values.Note != Note,
            _ => false
        };

    public bool TryPaste(string key, string text)
    {
        var value = text.Trim();
        var culture = CultureInfo.CurrentCulture;
        switch (key)
        {
            case nameof(Code) when CodeError(value.ToUpperInvariant()) is null:
                Code = value.ToUpperInvariant();
                return true;
            case nameof(Description) when DescriptionError(value) is null:
                Description = value;
                return true;
            case nameof(Quantity) when Int32.TryParse(value, NumberStyles.Integer, culture, out var quantity) && (QuantityError(quantity) is null):
                Quantity = quantity;
                return true;
            case nameof(UnitPrice) when Decimal.TryParse(value, NumberStyles.Number, culture, out var price) && (UnitPriceError(price) is null):
                UnitPrice = price;
                return true;
            case nameof(Discount) when Int32.TryParse(value, NumberStyles.Integer, culture, out var discount) && (DiscountError(discount) is null):
                Discount = discount;
                return true;
            case nameof(Delivery) when DateTime.TryParse(value, culture, DateTimeStyles.None, out var delivery) && (DeliveryError(delivery.Date, true) is null):
                Delivery = delivery.Date;
                return true;
            case nameof(Status) when TryParseStatus(value, out var status):
                Status = status;
                return true;
            case nameof(Note) when NoteError(value) is null:
                Note = value;
                return true;
            default:
                return false;
        }
    }

    protected override void RaisePropertyChanged(PropertyChangedEventArgs args)
    {
        base.RaisePropertyChanged(args);

        if (!initialized || !IsValueProperty(args.PropertyName))
        {
            return;
        }

        touched.Add(args.PropertyName!);
        Update();
    }

    [GeneratedRegex("^[A-Z0-9-]+$")]
    private static partial Regex CodeRegex();

    private static bool IsValueProperty(string? name) =>
        name is nameof(Code) or nameof(Description) or nameof(Quantity) or nameof(UnitPrice) or nameof(Discount) or nameof(Delivery) or nameof(Status) or nameof(Note);

    private static bool TryParseStatus(string text, out OrderStatus status)
    {
        foreach (var value in Enum.GetValues<OrderStatus>())
        {
            if (String.Equals(value.ToString(), text, StringComparison.OrdinalIgnoreCase))
            {
                status = value;
                return true;
            }
        }

        status = default;
        return false;
    }

    private static string? CodeError(string value) => value switch
    {
        "" => "Enter an item code.",
        { Length: > CodeLength } => "Use up to 10 characters.",
        _ when !CodeRegex().IsMatch(value) => "Use capital letters, digits and hyphens.",
        _ => null
    };

    private static string? DescriptionError(string value) => value switch
    {
        "" => "Enter a description.",
        { Length: > DescriptionLength } => "Use up to 40 characters.",
        _ => null
    };

    private static string? QuantityError(int? value) =>
        value is >= MinQuantity and <= MaxQuantity ? null : "Enter a quantity from 1 to 999.";

    private static string? UnitPriceError(decimal? value) => value switch
    {
        not (>= MinUnitPrice and <= MaxUnitPrice) => "Enter a unit price from 0.01 to 99,999.99.",
        { Scale: > PriceDecimals } => "Use up to 2 decimal places.",
        _ => null
    };

    private static string? DiscountError(int? value) =>
        value is >= 0 and <= MaxDiscount ? null : "Enter a discount from 0 to 50.";

    private static string? NoteError(string value) =>
        value.Length > NoteLength ? "Use up to 100 characters." : null;

    private static string? NoteRuleError(int? discount, string note) =>
        (discount > NoteDiscount) && (note.Length == 0) ? "A discount over 20% needs a note." : null;

    private string? DeliveryError(DateTime? value, bool checkDate) => value switch
    {
        null => "Choose a delivery date.",
        { } date when checkDate && (date.Date < Today) => "Choose today or a later date.",
        _ => null
    };

    private void Apply(OrderLineValues values)
    {
        Code = values.Code;
        Description = values.Description;
        Quantity = values.Quantity;
        UnitPrice = values.UnitPrice;
        Discount = values.Discount;
        Delivery = values.Delivery;
        Status = values.Status;
        Note = values.Note;
    }

    private void Update()
    {
        Validate();
        Revision++;
        RaisePropertyChanged(nameof(IsNew));
        RaisePropertyChanged(nameof(IsChanged));
        RaisePropertyChanged(nameof(IsLocked));
        RaisePropertyChanged(nameof(HasErrors));
        RaisePropertyChanged(nameof(ErrorText));
    }

    private void Validate()
    {
        var next = new Dictionary<string, string>();
        if (!IsLocked)
        {
            AddError(next, nameof(Code), CodeError(Code));
            AddError(next, nameof(Description), DescriptionError(Description));
            AddError(next, nameof(Quantity), QuantityError(Quantity));
            AddError(next, nameof(UnitPrice), UnitPriceError(UnitPrice));
            AddError(next, nameof(Discount), DiscountError(Discount));
            AddError(next, nameof(Delivery), DeliveryError(Delivery, IsNew || (saved?.Delivery != Delivery)));
            AddError(next, nameof(Note), NoteError(Note) ?? NoteRuleError(Discount, Note), nameof(Discount));
        }

        var changed = errors.Keys.Union(next.Keys).Where(x => errors.GetValueOrDefault(x) != next.GetValueOrDefault(x)).ToList();
        errors = next;
        foreach (var key in changed)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(key));
        }
    }

    private void AddError(Dictionary<string, string> target, string key, string? message, string? relatedKey = null)
    {
        if ((message is not null) && (!IsNew || revealed || touched.Contains(key) || ((relatedKey is not null) && touched.Contains(relatedKey))))
        {
            target.Add(key, message);
        }
    }
}
