namespace Template.WindowsApp.Behaviors;

using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using BunnyTail.XamlProperty;

using Microsoft.Xaml.Behaviors;

using Template.WindowsApp.Models;

public sealed partial class DataGridColumnLayoutBehavior : Behavior<DataGrid>
{
    private const string LeftGripperName = "PART_LeftHeaderGripper";

    private readonly Dictionary<DataGridColumn, DataGridLength> defaultWidths = [];

    [DependencyProperty(PropertyChanged = nameof(OnColumnsChanged))]
    public partial IReadOnlyList<DataGridColumnSetting>? Columns { get; set; }

    protected override void OnAttached()
    {
        base.OnAttached();
        var grid = AssociatedObject;
        grid.Columns.CollectionChanged += OnGridColumnsChanged;
        grid.AddHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler(OnDragCompleted), true);
        Subscribe(Columns);
        foreach (var column in grid.Columns)
        {
            AddColumn(column);
        }
    }

    protected override void OnDetaching()
    {
        var grid = AssociatedObject;
        Unsubscribe(Columns);
        grid.Columns.CollectionChanged -= OnGridColumnsChanged;
        grid.RemoveHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler(OnDragCompleted));
        defaultWidths.Clear();
        base.OnDetaching();
    }

    private void OnColumnsChanged(IReadOnlyList<DataGridColumnSetting>? oldValue, IReadOnlyList<DataGridColumnSetting>? newValue)
    {
        if (AssociatedObject is null)
        {
            return;
        }

        Unsubscribe(oldValue);
        Subscribe(newValue);
        foreach (var column in AssociatedObject.Columns)
        {
            Apply(column);
        }
    }

    private void Subscribe(IReadOnlyList<DataGridColumnSetting>? settings)
    {
        foreach (var setting in settings ?? [])
        {
            setting.PropertyChanged += OnSettingChanged;
        }
    }

    private void Unsubscribe(IReadOnlyList<DataGridColumnSetting>? settings)
    {
        foreach (var setting in settings ?? [])
        {
            setting.PropertyChanged -= OnSettingChanged;
        }
    }

    private void OnGridColumnsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        foreach (var column in e.NewItems?.OfType<DataGridColumn>() ?? [])
        {
            AddColumn(column);
        }
    }

    private void OnSettingChanged(object? sender, PropertyChangedEventArgs e)
    {
        if ((sender is DataGridColumnSetting setting) && (FindColumn(setting) is { } column))
        {
            Apply(column);
        }
    }

    private void OnDragCompleted(object sender, DragCompletedEventArgs e)
    {
        if ((e.OriginalSource is not Thumb { TemplatedParent: DataGridColumnHeader { Column: { } column } } thumb) ||
            ((thumb.Name == LeftGripperName ? PreviousColumn(column) : column) is not { } target) ||
            (Find(target) is not { } setting))
        {
            return;
        }

        setting.Width = target.ActualWidth;
    }

    private void AddColumn(DataGridColumn column)
    {
        defaultWidths.TryAdd(column, column.Width);
        Apply(column);
    }

    private void Apply(DataGridColumn column)
    {
        if (Find(column) is not { } setting)
        {
            return;
        }

        column.Visibility = setting.IsVisible ? Visibility.Visible : Visibility.Collapsed;
        column.Width = setting.Width is { } width ? new DataGridLength(width) : defaultWidths.GetValueOrDefault(column, column.Width);
    }

    private DataGridColumnSetting? Find(DataGridColumn column) =>
        Columns?.FirstOrDefault(x => Equals(x.Header, column.Header));

    private DataGridColumn? FindColumn(DataGridColumnSetting setting) =>
        AssociatedObject?.Columns.FirstOrDefault(x => Equals(x.Header, setting.Header));

    private DataGridColumn? PreviousColumn(DataGridColumn column) =>
        AssociatedObject.Columns
            .Where(x => (x.Visibility == Visibility.Visible) && (x.DisplayIndex < column.DisplayIndex))
            .MaxBy(static x => x.DisplayIndex);
}
