namespace Template.WindowsApp.Behaviors;

using System.Windows.Controls;

using Microsoft.Xaml.Behaviors;

public sealed class DataGridEndEditAction : TriggerAction<DataGrid>
{
    public bool Cancel { get; set; }

    protected override void Invoke(object parameter)
    {
        if (Cancel)
        {
            AssociatedObject.CancelEdit(DataGridEditingUnit.Row);
        }
        else
        {
            AssociatedObject.CommitEdit(DataGridEditingUnit.Row, true);
        }
    }
}
