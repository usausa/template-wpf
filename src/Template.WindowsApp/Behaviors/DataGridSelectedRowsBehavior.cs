namespace Template.WindowsApp.Behaviors;

using System.Windows.Controls;

using BunnyTail.XamlProperty;

using Microsoft.Xaml.Behaviors;

public sealed partial class DataGridSelectedRowsBehavior : Behavior<DataGrid>
{
    [DependencyProperty(Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)]
    public partial IReadOnlyList<object>? SelectedRows { get; set; }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.SelectedCellsChanged += OnSelectedCellsChanged;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.SelectedCellsChanged -= OnSelectedCellsChanged;
        base.OnDetaching();
    }

    private void OnSelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
    {
        var items = AssociatedObject.Items;
        SelectedRows = AssociatedObject.SelectedCells
            .Select(static x => x.Item)
            .Distinct()
            .OrderBy(items.IndexOf)
            .ToList();
    }
}
