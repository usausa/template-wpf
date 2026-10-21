namespace Template.WindowsApp.Controls;

using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using BunnyTail.XamlProperty;

[TemplatePart(Name = SecondaryButtonPartName, Type = typeof(ButtonBase))]
public sealed partial class SplitButton : ContentControl
{
    private const string SecondaryButtonPartName = "PART_SecondaryButton";

    private ButtonBase? secondaryButton;

    [DependencyProperty]
    public partial ICommand? Command { get; set; }

    [DependencyProperty]
    public partial object? CommandParameter { get; set; }

    static SplitButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SplitButton), new FrameworkPropertyMetadata(typeof(SplitButton)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (secondaryButton is not null)
        {
            secondaryButton.Click -= OnSecondaryButtonClick;
        }

        secondaryButton = GetTemplateChild(SecondaryButtonPartName) as ButtonBase;

        if (secondaryButton is not null)
        {
            secondaryButton.Click += OnSecondaryButtonClick;
        }
    }

    private void OnSecondaryButtonClick(object sender, RoutedEventArgs e)
    {
        if (ContextMenu is { } menu)
        {
            menu.PlacementTarget = this;
            menu.Placement = PlacementMode.Bottom;
            menu.IsOpen = true;
        }
    }
}
