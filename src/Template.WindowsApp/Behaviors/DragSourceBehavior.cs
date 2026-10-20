namespace Template.WindowsApp.Behaviors;

using BunnyTail.XamlProperty;

using Microsoft.Xaml.Behaviors;

public sealed partial class DragSourceBehavior : Behavior<FrameworkElement>
{
    private Point? start;

    [DependencyProperty]
    public partial string? Text { get; set; }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
        AssociatedObject.PreviewMouseMove += OnMouseMove;
        AssociatedObject.PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.PreviewMouseLeftButtonDown -= OnMouseLeftButtonDown;
        AssociatedObject.PreviewMouseMove -= OnMouseMove;
        AssociatedObject.PreviewMouseLeftButtonUp -= OnMouseLeftButtonUp;
        base.OnDetaching();
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) =>
        start = e.GetPosition(AssociatedObject);

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) =>
        start = null;

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if ((start is not { } origin) || (e.LeftButton != MouseButtonState.Pressed) || String.IsNullOrEmpty(Text))
        {
            return;
        }

        var position = e.GetPosition(AssociatedObject);
        if ((Math.Abs(position.X - origin.X) < SystemParameters.MinimumHorizontalDragDistance) &&
            (Math.Abs(position.Y - origin.Y) < SystemParameters.MinimumVerticalDragDistance))
        {
            return;
        }

        start = null;
        DragDrop.DoDragDrop(AssociatedObject, Text, DragDropEffects.Copy);
    }
}
