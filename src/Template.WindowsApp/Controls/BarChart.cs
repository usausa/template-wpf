namespace Template.WindowsApp.Controls;

using BunnyTail.XamlProperty;

public sealed partial class BarChart : ChartBase
{
    private const double BarRatio = 0.5;

    private const double MaxBarWidth = 48;

    [DependencyProperty(Options = FrameworkPropertyMetadataOptions.AffectsRender)]
    public partial Brush? Fill { get; set; }

    protected override void OnRender(DrawingContext drawingContext)
    {
        var values = Values;
        if ((values is null) || (values.Count == 0))
        {
            return;
        }

        var textHeight = MakeText("0").Height;
        var plotTop = textHeight + LabelSpacing;
        var plotHeight = ActualHeight - ((textHeight + LabelSpacing) * 2);
        var max = MaxOf(values);
        if ((ActualWidth <= 0) || (plotHeight <= 0) || (max <= 0))
        {
            return;
        }

        var step = ActualWidth / values.Count;
        var barWidth = Math.Min(step * BarRatio, MaxBarWidth);
        var labels = Labels;
        for (var i = 0; i < values.Count; i++)
        {
            var center = step * (i + 0.5);
            var height = plotHeight * values[i] / max;
            var top = plotTop + plotHeight - height;
            if (Fill is not null)
            {
                drawingContext.DrawRectangle(Fill, null, new Rect(center - (barWidth / 2), top, barWidth, height));
            }

            var value = MakeText(values[i].ToString("N0", CultureInfo.CurrentCulture));
            drawingContext.DrawText(value, new Point(center - (value.Width / 2), top - LabelSpacing - value.Height));

            if ((labels is not null) && (i < labels.Count))
            {
                var label = MakeText(labels[i]);
                drawingContext.DrawText(label, new Point(center - (label.Width / 2), plotTop + plotHeight + LabelSpacing));
            }
        }
    }
}
