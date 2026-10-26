namespace Template.WindowsApp.Behaviors;

using BunnyTail.XamlProperty;

using Microsoft.Xaml.Behaviors;

public sealed partial class MouseBackForwardBehavior : Behavior<UIElement>
{
    [DependencyProperty]
    public partial ICommand? BackCommand { get; set; }

    [DependencyProperty]
    public partial ICommand? ForwardCommand { get; set; }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.MouseUp += OnMouseUp;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.MouseUp -= OnMouseUp;
        base.OnDetaching();
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        var command = e.ChangedButton switch
        {
            MouseButton.XButton1 => BackCommand,
            MouseButton.XButton2 => ForwardCommand,
            _ => null
        };
        if ((command is not null) && command.CanExecute(null))
        {
            command.Execute(null);
            e.Handled = true;
        }
    }
}
