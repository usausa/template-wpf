namespace Template.WindowsApp.Converters;

public sealed class CountBadgeImageConverter : IValueConverter
{
    private const double Size = 16;

    private const int MaxCount = 99;

    private const double OneDigitFontSize = 11;

    private const double TwoDigitFontSize = 10;

    private const double OverflowFontSize = 8;

    private const double PixelsPerDip = 1;

    private static readonly Brush BackgroundBrush = MakeBrush(Color.FromRgb(0xC4, 0x2B, 0x1C));

    private static readonly Typeface Face = new(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal);

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if ((value is not int count) || (count <= 0))
        {
            return null;
        }

        var text = count > MaxCount ? String.Create(culture, $"{MaxCount}+") : count.ToString(culture);
        var fontSize = text.Length switch
        {
            1 => OneDigitFontSize,
            2 => TwoDigitFontSize,
            _ => OverflowFontSize
        };
        var geometry = new FormattedText(text, culture, FlowDirection.LeftToRight, Face, fontSize, Brushes.White, PixelsPerDip).BuildGeometry(default);
        var bounds = geometry.Bounds;
        geometry.Transform = new TranslateTransform(Math.Round(((Size - bounds.Width) / 2) - bounds.X), Math.Round(((Size - bounds.Height) / 2) - bounds.Y));

        var drawing = new DrawingGroup();
        using (var context = drawing.Open())
        {
            context.DrawRectangle(BackgroundBrush, null, new Rect(0, 0, Size, Size));
            context.DrawGeometry(Brushes.White, null, geometry);
        }

        var image = new DrawingImage(drawing);
        image.Freeze();
        return image;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();

    private static SolidColorBrush MakeBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}
