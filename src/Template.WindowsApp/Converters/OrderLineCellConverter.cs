namespace Template.WindowsApp.Converters;

using Template.WindowsApp.Models.Sample;

public sealed class OrderLineCellConverter : IMultiValueConverter
{
    public object? Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if ((values.Length < 2) || (values[0] is not OrderLine line) || (values[1] is not string key))
        {
            return parameter is "Error" ? null : false;
        }

        return parameter switch
        {
            "Changed" => line.IsCellChanged(key),
            "HasError" => line.GetCellError(key) is not null,
            _ => line.GetCellError(key)
        };
    }

    public object?[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
