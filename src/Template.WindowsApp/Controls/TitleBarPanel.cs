namespace Template.WindowsApp.Controls;

using System.Windows.Controls;

public sealed class TitleBarPanel : Panel
{
    private const int LeftIndex = 0;

    private const int TitleIndex = 1;

    private const int CenterIndex = 2;

    private const int RightIndex = 3;

    protected override Size MeasureOverride(Size availableSize)
    {
        if (InternalChildren.Count <= RightIndex)
        {
            return default;
        }

        var left = InternalChildren[LeftIndex];
        var title = InternalChildren[TitleIndex];
        var center = InternalChildren[CenterIndex];
        var right = InternalChildren[RightIndex];

        var unlimited = new Size(double.PositiveInfinity, availableSize.Height);
        left.Measure(unlimited);
        center.Measure(unlimited);
        right.Measure(unlimited);

        var space = Math.Max(0, availableSize.Width - right.DesiredSize.Width);
        var centerWidth = IsCenterFit(left, center, space) ? center.DesiredSize.Width : 0;
        title.Measure(new Size(Math.Max(0, space - left.DesiredSize.Width - centerWidth), availableSize.Height));

        var width = left.DesiredSize.Width + title.DesiredSize.Width + centerWidth + right.DesiredSize.Width;
        var height = Math.Max(Math.Max(left.DesiredSize.Height, title.DesiredSize.Height), Math.Max(center.DesiredSize.Height, right.DesiredSize.Height));
        return new Size(Math.Min(width, availableSize.Width), Math.Min(height, availableSize.Height));
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (InternalChildren.Count <= RightIndex)
        {
            return finalSize;
        }

        var left = InternalChildren[LeftIndex];
        var title = InternalChildren[TitleIndex];
        var center = InternalChildren[CenterIndex];
        var right = InternalChildren[RightIndex];

        var rightWidth = right.DesiredSize.Width;
        right.Arrange(new Rect(finalSize.Width - rightWidth, 0, rightWidth, finalSize.Height));

        var space = Math.Max(0, finalSize.Width - rightWidth);
        var leftWidth = Math.Min(left.DesiredSize.Width, space);
        left.Arrange(new Rect(0, 0, leftWidth, finalSize.Height));

        var centerWidth = IsCenterFit(left, center, space) ? center.DesiredSize.Width : 0;
        var titleWidth = Math.Min(title.DesiredSize.Width, space - leftWidth - centerWidth);
        title.Arrange(new Rect(leftWidth, 0, titleWidth, finalSize.Height));
        center.Arrange(new Rect(Math.Clamp((finalSize.Width - centerWidth) / 2, leftWidth + titleWidth, space - centerWidth), 0, centerWidth, finalSize.Height));

        return finalSize;
    }

    private static bool IsCenterFit(UIElement left, UIElement center, double space) =>
        left.DesiredSize.Width + center.DesiredSize.Width <= space;
}
