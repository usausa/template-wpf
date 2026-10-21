namespace Template.WindowsApp.Behaviors;

using System.Collections.Specialized;
using System.Windows.Controls;

using BunnyTail.XamlProperty;

using Microsoft.Xaml.Behaviors;

public sealed partial class AutoScrollBehavior : Behavior<DataGrid>
{
    [DependencyProperty(DefaultValue = true, PropertyChanged = nameof(OnIsEnabledChanged))]
    public partial bool IsEnabled { get; set; }

    protected override void OnAttached()
    {
        base.OnAttached();
        ((INotifyCollectionChanged)AssociatedObject.Items).CollectionChanged += OnCollectionChanged;
    }

    protected override void OnDetaching()
    {
        ((INotifyCollectionChanged)AssociatedObject.Items).CollectionChanged -= OnCollectionChanged;
        base.OnDetaching();
    }

    private void OnIsEnabledChanged() => ScrollToEnd();

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            ScrollToEnd();
        }
    }

    private void ScrollToEnd()
    {
        if (IsEnabled && (AssociatedObject is { Items.Count: > 0 } grid))
        {
            grid.ScrollIntoView(grid.Items[^1]);
        }
    }
}
