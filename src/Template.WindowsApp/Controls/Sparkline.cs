namespace Template.WindowsApp.Controls;

using BunnyTail.XamlProperty;

public sealed partial class Sparkline : FrameworkElement
{
    private const double LineThickness = 1.5;

    private const double DotRadius = 2.5;

    [DependencyProperty(Options = FrameworkPropertyMetadataOptions.AffectsRender)]
    public partial IReadOnlyList<double>? Values { get; set; }

    [DependencyProperty(Options = FrameworkPropertyMetadataOptions.AffectsRender)]
    public partial Brush? Stroke { get; set; }

    protected override void OnRender(DrawingContext drawingContext)
    {
        var values = Values;
        var width = ActualWidth - (DotRadius * 2);
        var height = ActualHeight - (DotRadius * 2);
        if ((values is null) || (values.Count < 2) || (width <= 0) || (height <= 0) || (Stroke is null))
        {
            return;
        }

        var min = values.Min();
        var max = values.Max();
        var points = new Point[values.Count];
        for (var i = 0; i < values.Count; i++)
        {
            var ratio = max > min ? (max - values[i]) / (max - min) : 0.5;
            points[i] = new Point(DotRadius + (width * i / (values.Count - 1)), DotRadius + (height * ratio));
        }

        var line = new StreamGeometry();
        using (var context = line.Open())
        {
            context.BeginFigure(points[0], false, false);
            context.PolyLineTo(points, true, true);
        }
        line.Freeze();
        drawingContext.DrawGeometry(null, new Pen(Stroke, LineThickness) { LineJoin = PenLineJoin.Round }, line);
        drawingContext.DrawEllipse(Stroke, null, points[^1], DotRadius, DotRadius);
    }
}
