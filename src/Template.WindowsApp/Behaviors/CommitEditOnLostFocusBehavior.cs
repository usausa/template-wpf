namespace Template.WindowsApp.Behaviors;

using System.Windows.Controls;

using Microsoft.Xaml.Behaviors;

public sealed class CommitEditOnLostFocusBehavior : Behavior<DataGrid>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.IsKeyboardFocusWithinChanged += OnIsKeyboardFocusWithinChanged;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.IsKeyboardFocusWithinChanged -= OnIsKeyboardFocusWithinChanged;
        base.OnDetaching();
    }

    private void OnIsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (!AssociatedObject.IsKeyboardFocusWithin)
        {
            AssociatedObject.CommitEdit(DataGridEditingUnit.Row, true);
        }
    }
}
