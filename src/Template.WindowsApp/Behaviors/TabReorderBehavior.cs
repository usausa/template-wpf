namespace Template.WindowsApp.Behaviors;

using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using BunnyTail.XamlProperty;

using Microsoft.Xaml.Behaviors;

using Template.WindowsApp.Models;

public sealed partial class TabReorderBehavior : Behavior<TabControl>
{
    private const string IndexFormat = "Template.WindowsApp.TabIndex";

    private Point? start;

    private int sourceIndex = -1;

    [DependencyProperty]
    public partial ICommand? MoveCommand { get; set; }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.AllowDrop = true;
        AssociatedObject.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
        AssociatedObject.PreviewMouseMove += OnMouseMove;
        AssociatedObject.PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;
        AssociatedObject.DragOver += OnDragOver;
        AssociatedObject.Drop += OnDrop;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.PreviewMouseLeftButtonDown -= OnMouseLeftButtonDown;
        AssociatedObject.PreviewMouseMove -= OnMouseMove;
        AssociatedObject.PreviewMouseLeftButtonUp -= OnMouseLeftButtonUp;
        AssociatedObject.DragOver -= OnDragOver;
        AssociatedObject.Drop -= OnDrop;
        base.OnDetaching();
    }

    private T? FindAncestor<T>(object source)
        where T : DependencyObject
    {
        var element = source as DependencyObject;
        while ((element is not null) && (element != AssociatedObject))
        {
            if (element is T found)
            {
                return found;
            }

            element = VisualTreeHelper.GetParent(element);
        }
        return null;
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var tab = FindAncestor<ButtonBase>(e.OriginalSource) is null ? FindAncestor<TabItem>(e.OriginalSource) : null;
        sourceIndex = tab is null ? -1 : AssociatedObject.ItemContainerGenerator.IndexFromContainer(tab);
        start = sourceIndex >= 0 ? e.GetPosition(AssociatedObject) : null;
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) =>
        start = null;

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if ((start is not { } origin) || (e.LeftButton != MouseButtonState.Pressed))
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
        DragDrop.DoDragDrop(AssociatedObject, new DataObject(IndexFormat, sourceIndex), DragDropEffects.Move);
    }

    private static void OnDragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(IndexFormat) ? DragDropEffects.Move : DragDropEffects.None;
        e.Handled = true;
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        e.Handled = true;
        if ((e.Data.GetData(IndexFormat) is not int oldIndex) || (FindAncestor<TabItem>(e.OriginalSource) is not { } target) || (MoveCommand is not { } command))
        {
            return;
        }

        var targetIndex = AssociatedObject.ItemContainerGenerator.IndexFromContainer(target);
        var insertIndex = e.GetPosition(target).X < (target.ActualWidth / 2) ? targetIndex : targetIndex + 1;
        var newIndex = insertIndex > oldIndex ? insertIndex - 1 : insertIndex;
        var move = new ItemMove(oldIndex, newIndex);
        if ((newIndex != oldIndex) && command.CanExecute(move))
        {
            command.Execute(move);
        }
    }
}
