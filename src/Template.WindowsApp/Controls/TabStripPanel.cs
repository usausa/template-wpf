namespace Template.WindowsApp.Controls;

using System.Windows.Controls;

using BunnyTail.XamlProperty;

public sealed partial class TabStripPanel : Panel
{
    private const double MinEqualWidth = 100;

    private const double MaxEqualWidth = 200;

    private double itemWidth;

    [DependencyProperty(Options = FrameworkPropertyMetadataOptions.AffectsMeasure)]
    public partial bool IsEqualWidth { get; set; }

    [DependencyProperty(Options = FrameworkPropertyMetadataOptions.AffectsMeasure)]
    public partial double AvailableWidth { get; set; }

    [DependencyProperty(Options = FrameworkPropertyMetadataOptions.AffectsMeasure)]
    public partial double ReservedWidth { get; set; }

    static TabStripPanel()
    {
        KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(TabStripPanel), new FrameworkPropertyMetadata(KeyboardNavigationMode.Once));
        KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(TabStripPanel), new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var count = InternalChildren.Count;
        itemWidth = IsEqualWidth && (count > 0) ? Math.Floor(Math.Clamp((AvailableWidth - ReservedWidth) / count, MinEqualWidth, MaxEqualWidth)) : Double.PositiveInfinity;

        var width = 0d;
        var height = 0d;
        foreach (UIElement child in InternalChildren)
        {
            child.Measure(new Size(itemWidth, availableSize.Height));
            width += IsEqualWidth ? itemWidth : child.DesiredSize.Width;
            height = Math.Max(height, child.DesiredSize.Height);
        }

        return new Size(width, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var x = 0d;
        foreach (UIElement child in InternalChildren)
        {
            var width = IsEqualWidth ? itemWidth : child.DesiredSize.Width;
            child.Arrange(new Rect(x, 0, width, finalSize.Height));
            x += width;
        }

        return finalSize;
    }
}
