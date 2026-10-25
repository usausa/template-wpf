namespace Template.WindowsApp.Behaviors;

using System.Windows.Controls;

using Microsoft.Xaml.Behaviors;

public sealed class DataGridTypeToEditBehavior : Behavior<DataGrid>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.PreviewTextInput += OnPreviewTextInput;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.PreviewTextInput -= OnPreviewTextInput;
        base.OnDetaching();
    }

    private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if ((e.OriginalSource is not DataGridCell { IsEditing: false, Column: DataGridTextColumn } cell) ||
            String.IsNullOrEmpty(e.Text) ||
            e.Text.Any(Char.IsControl) ||
            !AssociatedObject.BeginEdit())
        {
            return;
        }

        e.Handled = true;
        if (cell.Content is TextBox textBox)
        {
            TextCompositionManager.StartComposition(new TextComposition(InputManager.Current, textBox, e.Text));
        }
    }
}
