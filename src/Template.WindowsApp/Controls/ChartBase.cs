namespace Template.WindowsApp.Controls;

using System.Collections.Specialized;
using System.Windows.Documents;

using BunnyTail.XamlProperty;

public abstract partial class ChartBase : FrameworkElement
{
    protected const double LabelSpacing = 4;

    [DependencyProperty(Options = FrameworkPropertyMetadataOptions.AffectsRender, PropertyChanged = nameof(OnValuesChanged))]
    public partial IReadOnlyList<double>? Values { get; set; }

    [DependencyProperty(Options = FrameworkPropertyMetadataOptions.AffectsRender, PropertyChanged = nameof(OnLabelsChanged))]
    public partial IReadOnlyList<string>? Labels { get; set; }

    [DependencyProperty(Options = FrameworkPropertyMetadataOptions.AffectsRender)]
    public partial Brush? LabelBrush { get; set; }

    [DependencyProperty(DefaultValue = 12d, Options = FrameworkPropertyMetadataOptions.AffectsRender)]
    public partial double LabelFontSize { get; set; }

    protected static double MaxOf(IReadOnlyList<double> values)
    {
        var max = 0d;
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < values.Count; i++)
        {
            max = Math.Max(max, values[i]);
        }
        return max;
    }

    protected static double NiceMax(double max)
    {
        var unit = Math.Pow(10, Math.Floor(Math.Log10(max))) / 2;
        return Math.Ceiling(max / unit) * unit;
    }

    protected FormattedText MakeText(string text) =>
        MakeText(text, LabelFontSize, LabelBrush ?? Brushes.Black);

    protected FormattedText MakeText(string text, double fontSize, Brush brush) =>
        new(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(TextElement.GetFontFamily(this), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
            fontSize,
            brush,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);

    private void OnValuesChanged(IReadOnlyList<double>? oldValue, IReadOnlyList<double>? newValue) =>
        Resubscribe(oldValue, newValue);

    private void OnLabelsChanged(IReadOnlyList<string>? oldValue, IReadOnlyList<string>? newValue) =>
        Resubscribe(oldValue, newValue);

    private void Resubscribe(object? oldValue, object? newValue)
    {
        if (oldValue is INotifyCollectionChanged oldCollection)
        {
            oldCollection.CollectionChanged -= OnCollectionChanged;
        }
        if (newValue is INotifyCollectionChanged newCollection)
        {
            newCollection.CollectionChanged += OnCollectionChanged;
        }
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => InvalidateVisual();
}
