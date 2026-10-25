namespace Template.WindowsApp.Controls;

using BunnyTail.XamlProperty;

public sealed partial class DataBar : FrameworkElement
{
    [DependencyProperty(Options = FrameworkPropertyMetadataOptions.AffectsRender)]
    public partial double Value { get; set; }

    [DependencyProperty(DefaultValue = 100d, Options = FrameworkPropertyMetadataOptions.AffectsRender)]
    public partial double Maximum { get; set; }

    [DependencyProperty(Options = FrameworkPropertyMetadataOptions.AffectsRender)]
    public partial Brush? Fill { get; set; }

    protected override void OnRender(DrawingContext drawingContext)
    {
        if ((Fill is null) || (ActualHeight <= 0) || (Maximum <= 0))
        {
            return;
        }

        var width = ActualWidth * Math.Clamp(Value / Maximum, 0, 1);
        if (width > 0)
        {
            drawingContext.DrawRectangle(Fill, null, new Rect(0, 0, width, ActualHeight));
        }
    }
}
