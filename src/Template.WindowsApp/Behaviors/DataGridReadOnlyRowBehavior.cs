namespace Template.WindowsApp.Behaviors;

using System.Windows.Controls;

using BunnyTail.XamlProperty;

using Microsoft.Xaml.Behaviors;

public sealed partial class DataGridReadOnlyRowBehavior : Behavior<DataGrid>
{
    [AttachedProperty]
    public static partial bool GetIsReadOnly(DataGridRow row);

    public static partial void SetIsReadOnly(DataGridRow row, bool value);

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.BeginningEdit += OnBeginningEdit;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.BeginningEdit -= OnBeginningEdit;
        base.OnDetaching();
    }

    private static void OnBeginningEdit(object? sender, DataGridBeginningEditEventArgs e)
    {
        if (GetIsReadOnly(e.Row))
        {
            e.Cancel = true;
        }
    }
}
