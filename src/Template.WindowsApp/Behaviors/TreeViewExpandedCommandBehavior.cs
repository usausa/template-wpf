namespace Template.WindowsApp.Behaviors;

using System.Windows.Controls;

using BunnyTail.XamlProperty;

using Microsoft.Xaml.Behaviors;

public sealed partial class TreeViewExpandedCommandBehavior : Behavior<TreeView>
{
    [DependencyProperty]
    public partial ICommand? Command { get; set; }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.AddHandler(TreeViewItem.ExpandedEvent, new RoutedEventHandler(OnExpanded));
    }

    protected override void OnDetaching()
    {
        AssociatedObject.RemoveHandler(TreeViewItem.ExpandedEvent, new RoutedEventHandler(OnExpanded));
        base.OnDetaching();
    }

    private void OnExpanded(object sender, RoutedEventArgs e)
    {
        if ((Command is { } command) && (e.OriginalSource is TreeViewItem { DataContext: { } item }) && command.CanExecute(item))
        {
            command.Execute(item);
        }
    }
}
