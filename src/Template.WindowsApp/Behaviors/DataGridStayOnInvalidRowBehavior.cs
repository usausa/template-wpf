namespace Template.WindowsApp.Behaviors;

using System.Windows.Controls;

using Microsoft.Xaml.Behaviors;

public sealed class DataGridStayOnInvalidRowBehavior : Behavior<DataGrid>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
        AssociatedObject.LoadingRow += OnLoadingRow;
        AssociatedObject.UnloadingRow += OnUnloadingRow;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
        AssociatedObject.LoadingRow -= OnLoadingRow;
        AssociatedObject.UnloadingRow -= OnUnloadingRow;
        base.OnDetaching();
    }

    private void OnLoadingRow(object? sender, DataGridRowEventArgs e)
    {
        e.Row.KeyDown -= OnRowKeyDown;
        e.Row.KeyDown += OnRowKeyDown;
    }

    private void OnUnloadingRow(object? sender, DataGridRowEventArgs e)
    {
        e.Row.KeyDown -= OnRowKeyDown;
    }

    private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var grid = AssociatedObject;
        if ((e.OriginalSource is DependencyObject source) &&
            (ItemsControl.ContainerFromElement(grid, source) is DataGridRow row) &&
            (EditingItem() is { } item) &&
            !Equals(row.Item, item) &&
            !grid.CommitEdit(DataGridEditingUnit.Row, true))
        {
            e.Handled = true;
        }
    }

    private void OnRowKeyDown(object sender, KeyEventArgs e)
    {
        if (LeavesRow(e) && (EditingItem() is not null) && !AssociatedObject.CommitEdit(DataGridEditingUnit.Row, true))
        {
            e.Handled = true;
        }
    }

    private object? EditingItem()
    {
        IEditableCollectionView view = AssociatedObject.Items;
        return view.IsAddingNew ? view.CurrentAddItem : view.IsEditingItem ? view.CurrentEditItem : null;
    }

    private bool LeavesRow(KeyEventArgs e)
    {
        var modifiers = e.KeyboardDevice.Modifiers;
        return e.Key switch
        {
            Key.Up or Key.Down or Key.PageUp or Key.PageDown => true,
            Key.Home or Key.End => modifiers.HasFlag(ModifierKeys.Control),
            Key.Tab => IsEdgeColumn(modifiers.HasFlag(ModifierKeys.Shift)),
            _ => false
        };
    }

    private bool IsEdgeColumn(bool backward)
    {
        var grid = AssociatedObject;
        if (grid.CurrentColumn is not { } current)
        {
            return false;
        }

        var indexes = grid.Columns.Where(static x => x.Visibility == Visibility.Visible).Select(static x => x.DisplayIndex).ToList();
        return current.DisplayIndex == (backward ? indexes.Min() : indexes.Max());
    }
}
