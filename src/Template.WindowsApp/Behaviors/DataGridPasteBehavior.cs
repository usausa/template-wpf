namespace Template.WindowsApp.Behaviors;

using System.Windows.Controls;

using BunnyTail.XamlProperty;

using Microsoft.Xaml.Behaviors;

public sealed partial class DataGridPasteBehavior : Behavior<DataGrid>
{
    private readonly CommandBinding binding;

    [DependencyProperty]
    public partial ICommand? Command { get; set; }

    public DataGridPasteBehavior()
    {
        binding = new CommandBinding(ApplicationCommands.Paste, OnPaste, OnCanPaste);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.CommandBindings.Add(binding);
    }

    protected override void OnDetaching()
    {
        AssociatedObject.CommandBindings.Remove(binding);
        base.OnDetaching();
    }

    private void OnCanPaste(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = Command is not null;
        e.Handled = true;
    }

    private void OnPaste(object sender, ExecutedRoutedEventArgs e)
    {
        e.Handled = true;
        var grid = AssociatedObject;
        if (!grid.CommitEdit(DataGridEditingUnit.Row, true))
        {
            return;
        }

        var items = grid.Items;
        var start = grid.SelectedCells.Count > 0
            ? grid.SelectedCells.OrderBy(x => items.IndexOf(x.Item)).ThenBy(static x => x.Column.DisplayIndex).First()
            : grid.CurrentCell;
        if (start.Column is null)
        {
            return;
        }

        grid.CurrentCell = start;
        var columns = grid.Columns
            .Where(x => (x.DisplayIndex >= start.Column.DisplayIndex) && (x.Visibility == Visibility.Visible))
            .OrderBy(static x => x.DisplayIndex)
            .Select(static x => x.SortMemberPath)
            .ToList();
        if (Command?.CanExecute(columns) == true)
        {
            Command.Execute(columns);
        }
    }
}
