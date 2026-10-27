namespace Template.WindowsApp.Converters;

using Template.WindowsApp.Settings;

public sealed class ThemeTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value switch
        {
            AppTheme.Light => "Light",
            AppTheme.Dark => "Dark",
            AppTheme.System => "Use system setting",
            _ => value
        };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
