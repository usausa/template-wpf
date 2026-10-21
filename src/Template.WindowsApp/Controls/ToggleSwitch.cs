namespace Template.WindowsApp.Controls;

using System.Windows.Controls.Primitives;

using BunnyTail.XamlProperty;

public sealed partial class ToggleSwitch : ToggleButton
{
    private const string OnStateName = "On";

    private const string OffStateName = "Off";

    [DependencyProperty(DefaultValue = "On")]
    public partial object? OnContent { get; set; }

    [DependencyProperty(DefaultValue = "Off")]
    public partial object? OffContent { get; set; }

    static ToggleSwitch()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleSwitch), new FrameworkPropertyMetadata(typeof(ToggleSwitch)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        UpdateState(false);
    }

    protected override void OnChecked(RoutedEventArgs e)
    {
        base.OnChecked(e);

        UpdateState(true);
    }

    protected override void OnUnchecked(RoutedEventArgs e)
    {
        base.OnUnchecked(e);

        UpdateState(true);
    }

    private void UpdateState(bool useTransitions) =>
        VisualStateManager.GoToState(this, IsChecked == true ? OnStateName : OffStateName, useTransitions);
}
