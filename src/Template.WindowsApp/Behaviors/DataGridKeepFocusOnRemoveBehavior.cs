namespace Template.WindowsApp.Behaviors;

using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Threading;

using Microsoft.Xaml.Behaviors;

public sealed class DataGridKeepFocusOnRemoveBehavior : Behavior<DataGrid>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        ((INotifyCollectionChanged)AssociatedObject.Items).CollectionChanged += OnCollectionChanged;
    }

    protected override void OnDetaching()
    {
        ((INotifyCollectionChanged)AssociatedObject.Items).CollectionChanged -= OnCollectionChanged;
        base.OnDetaching();
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var grid = AssociatedObject;
        if ((e.Action != NotifyCollectionChangedAction.Remove) || !grid.IsKeyboardFocusWithin)
        {
            return;
        }

        var index = e.OldStartingIndex;
        var column = grid.CurrentColumn;
        grid.Dispatcher.InvokeAsync(() => Restore(index, column), DispatcherPriority.Background);
    }

    private void Restore(int index, DataGridColumn? column)
    {
        var grid = AssociatedObject;
        if ((grid is null) || (grid.Items.Count == 0) || (Keyboard.FocusedElement is not (null or Window)))
        {
            return;
        }

        var target = column ?? grid.Columns
            .Where(static x => x.Visibility == Visibility.Visible)
            .MinBy(static x => x.DisplayIndex);
        if ((target is null) || (grid.Items[Math.Min(index, grid.Items.Count - 1)] is not { } item))
        {
            return;
        }

        var cell = new DataGridCellInfo(item, target);
        grid.ScrollIntoView(item, target);
        grid.Focus();
        grid.UnselectAllCells();
        grid.CurrentCell = cell;
        grid.SelectedCells.Add(cell);
    }
}
