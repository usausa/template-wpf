namespace Template.WindowsApp.Converters;

public sealed class CurrencyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is decimal amount ? amount.ToString("C2", culture) : value;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        (value is string text) && Decimal.TryParse(text, NumberStyles.Currency, culture, out var amount) ? amount : DependencyProperty.UnsetValue;
}
