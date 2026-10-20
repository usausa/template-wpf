namespace Template.WindowsApp.Behaviors;

using System.Windows.Controls;

using BunnyTail.XamlProperty;

using Microsoft.Xaml.Behaviors;

public sealed partial class TreeViewSelectedItemBehavior : Behavior<TreeView>
{
    [DependencyProperty(Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)]
    public partial object? SelectedItem { get; set; }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.SelectedItemChanged += OnSelectedItemChanged;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.SelectedItemChanged -= OnSelectedItemChanged;
        base.OnDetaching();
    }

    private void OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) => SelectedItem = e.NewValue;
}
