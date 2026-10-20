namespace Template.WindowsApp.Controls;

using System.Windows.Documents;

using BunnyTail.XamlProperty;

public sealed partial class DonutChart : ChartBase
{
    private const double StartAngle = -90;

    [DependencyProperty(Options = FrameworkPropertyMetadataOptions.AffectsRender)]
    public partial IReadOnlyList<Brush>? Fills { get; set; }

    [DependencyProperty(DefaultValue = 32d, Options = FrameworkPropertyMetadataOptions.AffectsRender)]
    public partial double Thickness { get; set; }

    [DependencyProperty(DefaultValue = 20d, Options = FrameworkPropertyMetadataOptions.AffectsRender)]
    public partial double TotalFontSize { get; set; }

    protected override void OnRender(DrawingContext drawingContext)
    {
        var values = Values;
        var fills = Fills;
        if ((values is null) || (values.Count == 0) || (fills is null) || (fills.Count == 0))
        {
            return;
        }

        var total = 0d;
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < values.Count; i++)
        {
            total += values[i];
        }

        var outer = Math.Min(ActualWidth, ActualHeight) / 2;
        var inner = Math.Max(outer - Thickness, 0);
        if ((outer <= 0) || (total <= 0))
        {
            return;
        }

        var center = new Point(ActualWidth / 2, ActualHeight / 2);
        var start = StartAngle;
        for (var i = 0; i < values.Count; i++)
        {
            var sweep = 360 * values[i] / total;
            if (sweep > 0)
            {
                drawingContext.DrawGeometry(fills[i % fills.Count], null, MakeSegment(center, outer, inner, start, sweep));
            }
            start += sweep;
        }

        var text = MakeText(total.ToString("N0", CultureInfo.CurrentCulture), TotalFontSize, TextElement.GetForeground(this));
        drawingContext.DrawText(text, new Point(center.X - (text.Width / 2), center.Y - (text.Height / 2)));
    }

    private static StreamGeometry MakeSegment(Point center, double outer, double inner, double start, double sweep)
    {
        var middle = start + (sweep / 2);
        var end = start + sweep;
        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            context.BeginFigure(PointAt(center, outer, start), true, true);
            context.ArcTo(PointAt(center, outer, middle), new Size(outer, outer), 0, false, SweepDirection.Clockwise, true, true);
            context.ArcTo(PointAt(center, outer, end), new Size(outer, outer), 0, false, SweepDirection.Clockwise, true, true);
            context.LineTo(PointAt(center, inner, end), true, true);
            context.ArcTo(PointAt(center, inner, middle), new Size(inner, inner), 0, false, SweepDirection.Counterclockwise, true, true);
            context.ArcTo(PointAt(center, inner, start), new Size(inner, inner), 0, false, SweepDirection.Counterclockwise, true, true);
        }
        geometry.Freeze();
        return geometry;
    }

    private static Point PointAt(Point center, double radius, double angle)
    {
        var radian = angle * Math.PI / 180;
        return new Point(center.X + (radius * Math.Cos(radian)), center.Y + (radius * Math.Sin(radian)));
    }
}
