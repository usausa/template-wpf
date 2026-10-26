namespace Template.WindowsApp.Controls;

using System.Windows.Controls;

using BunnyTail.XamlProperty;

public sealed partial class RemovableTag : Control
{
    [DependencyProperty]
    public partial string? Text { get; set; }

    [DependencyProperty]
    public partial ICommand? RemoveCommand { get; set; }

    [DependencyProperty]
    public partial object? RemoveCommandParameter { get; set; }

    static RemovableTag()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RemovableTag), new FrameworkPropertyMetadata(typeof(RemovableTag)));
    }
}
