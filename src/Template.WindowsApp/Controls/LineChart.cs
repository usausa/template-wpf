namespace Template.WindowsApp.Controls;

using BunnyTail.XamlProperty;

public sealed partial class LineChart : ChartBase
{
    private const int GridLineCount = 3;

    private const double LineThickness = 2;

    [DependencyProperty(Options = FrameworkPropertyMetadataOptions.AffectsRender)]
    public partial Brush? Stroke { get; set; }

    [DependencyProperty(Options = FrameworkPropertyMetadataOptions.AffectsRender)]
    public partial Brush? Fill { get; set; }

    [DependencyProperty(Options = FrameworkPropertyMetadataOptions.AffectsRender)]
    public partial Brush? GridBrush { get; set; }

    [DependencyProperty(Options = FrameworkPropertyMetadataOptions.AffectsRender)]
    public partial int Capacity { get; set; }

    [DependencyProperty(DefaultValue = double.NaN, Options = FrameworkPropertyMetadataOptions.AffectsRender)]
    public partial double Minimum { get; set; }

    [DependencyProperty(DefaultValue = double.NaN, Options = FrameworkPropertyMetadataOptions.AffectsRender)]
    public partial double Maximum { get; set; }

    protected override void OnRender(DrawingContext drawingContext)
    {
        var values = Values;
        if ((values is null) || (values.Count == 0))
        {
            return;
        }

        var plotHeight = ActualHeight - MakeText("0").Height - LabelSpacing;
        var low = Double.IsNaN(Minimum) ? 0 : Minimum;
        var high = Maximum;
        if (Double.IsNaN(high))
        {
            var max = MaxOf(values);
            high = max > 0 ? NiceMax(max) : 0;
        }
        if ((ActualWidth <= 0) || (plotHeight <= 0) || (high <= low))
        {
            return;
        }

        if (GridBrush is not null)
        {
            var pen = new Pen(GridBrush, 1);
            for (var i = 0; i < GridLineCount; i++)
            {
                var y = Math.Round(plotHeight * i / (GridLineCount - 1)) + 0.5;
                drawingContext.DrawLine(pen, new Point(0, y), new Point(ActualWidth, y));
            }
        }

        var count = Math.Max(Capacity, values.Count);
        var offset = count - values.Count;
        var step = ActualWidth / count;
        var points = new Point[values.Count];
        for (var i = 0; i < values.Count; i++)
        {
            points[i] = new Point(step * (offset + i + 0.5), plotHeight - (plotHeight * (Math.Clamp(values[i], low, high) - low) / (high - low)));
        }

        if (Fill is not null)
        {
            var area = new StreamGeometry();
            using (var context = area.Open())
            {
                context.BeginFigure(new Point(points[0].X, plotHeight), true, true);
                context.PolyLineTo(points, true, true);
                context.LineTo(new Point(points[^1].X, plotHeight), true, true);
            }
            area.Freeze();
            drawingContext.DrawGeometry(Fill, null, area);
        }

        if (Stroke is not null)
        {
            var line = new StreamGeometry();
            using (var context = line.Open())
            {
                context.BeginFigure(points[0], false, false);
                context.PolyLineTo(points, true, true);
            }
            line.Freeze();
            drawingContext.DrawGeometry(null, new Pen(Stroke, LineThickness) { LineJoin = PenLineJoin.Round }, line);
        }

        var labels = Labels;
        if (labels is not null)
        {
            for (var i = 0; (i < labels.Count) && (i < count); i++)
            {
                if (labels[i].Length == 0)
                {
                    continue;
                }

                var text = MakeText(labels[i]);
                var x = Math.Clamp((step * (i + 0.5)) - (text.Width / 2), 0, Math.Max(0, ActualWidth - text.Width));
                drawingContext.DrawText(text, new Point(x, plotHeight + LabelSpacing));
            }
        }
    }
}
