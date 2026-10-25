namespace Template.WindowsApp.Behaviors;

using System.Windows.Controls;

using BunnyTail.XamlProperty;

using Microsoft.Xaml.Behaviors;

public sealed partial class DataGridSortBehavior : Behavior<DataGrid>
{
    [DependencyProperty(Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PropertyChanged = nameof(OnSortChanged))]
    public partial SortDescription? Sort { get; set; }

    [AttachedProperty]
    public static partial ListSortDirection GetFirstDirection(DataGridColumn column);

    public static partial void SetFirstDirection(DataGridColumn column, ListSortDirection value);

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.Sorting += OnSorting;
        UpdateColumns();
    }

    protected override void OnDetaching()
    {
        AssociatedObject.Sorting -= OnSorting;
        base.OnDetaching();
    }

    private void OnSortChanged() => UpdateColumns();

    private void OnSorting(object sender, DataGridSortingEventArgs e)
    {
        e.Handled = true;
        var direction = e.Column.SortDirection switch
        {
            ListSortDirection.Ascending => ListSortDirection.Descending,
            ListSortDirection.Descending => ListSortDirection.Ascending,
            _ => GetFirstDirection(e.Column)
        };
        Sort = new SortDescription(e.Column.SortMemberPath, direction);
    }

    private void UpdateColumns()
    {
        if (AssociatedObject is null)
        {
            return;
        }

        foreach (var column in AssociatedObject.Columns)
        {
            column.SortDirection = (Sort is { } sort) && (column.SortMemberPath == sort.PropertyName) ? sort.Direction : null;
        }
    }
}
