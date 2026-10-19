namespace Template.WindowsApp.Behaviors;

using System.Windows.Controls;

using Microsoft.Xaml.Behaviors;

public sealed class ScrollIntoSelectedItemBehavior : Behavior<ListBox>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.SelectionChanged += OnSelectionChanged;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.SelectionChanged -= OnSelectionChanged;
        base.OnDetaching();
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AssociatedObject.SelectedItem is { } item)
        {
            AssociatedObject.ScrollIntoView(item);
        }
    }
}
