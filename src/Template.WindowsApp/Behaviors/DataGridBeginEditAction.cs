namespace Template.WindowsApp.Behaviors;

using System.Windows.Controls;
using System.Windows.Threading;

using Microsoft.Xaml.Behaviors;

public sealed class DataGridBeginEditAction : TriggerAction<DataGrid>
{
    protected override void Invoke(object? parameter)
    {
        var grid = AssociatedObject;
        var column = grid.Columns
            .Where(static x => !x.IsReadOnly && (x.Visibility == Visibility.Visible))
            .MinBy(static x => x.DisplayIndex);
        if ((parameter is null) || (column is null))
        {
            return;
        }

        var cell = new DataGridCellInfo(parameter, column);
        grid.ScrollIntoView(parameter, column);
        grid.UnselectAllCells();
        grid.CurrentCell = cell;
        grid.SelectedCells.Add(cell);
        grid.Dispatcher.InvokeAsync(() =>
        {
            grid.Focus();
            grid.BeginEdit();
        }, DispatcherPriority.Background);
    }
}
