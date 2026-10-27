namespace Template.WindowsApp.Behaviors;

using System.Windows.Controls;
using System.Windows.Threading;

using BunnyTail.XamlProperty;

using Microsoft.Xaml.Behaviors;

public sealed partial class DataGridRowCommitBehavior : Behavior<DataGrid>
{
    [DependencyProperty]
    public partial ICommand? Command { get; set; }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.RowEditEnding += OnRowEditEnding;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.RowEditEnding -= OnRowEditEnding;
        base.OnDetaching();
    }

    private void OnRowEditEnding(object? sender, DataGridRowEditEndingEventArgs e)
    {
        if (e.EditAction != DataGridEditAction.Commit)
        {
            return;
        }

        var item = e.Row.Item;
        Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
        {
            if ((Command is { } command) && command.CanExecute(item))
            {
                command.Execute(item);
            }
        });
    }
}
