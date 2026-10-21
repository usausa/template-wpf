namespace Template.WindowsApp.Controls;

using System.Windows.Controls;
using System.Windows.Controls.Primitives;

[TemplatePart(Name = ButtonPartName, Type = typeof(ButtonBase))]
public sealed class DropDownButton : ContentControl
{
    private const string ButtonPartName = "PART_Button";

    private ButtonBase? button;

    static DropDownButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DropDownButton), new FrameworkPropertyMetadata(typeof(DropDownButton)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (button is not null)
        {
            button.Click -= OnButtonClick;
        }

        button = GetTemplateChild(ButtonPartName) as ButtonBase;

        if (button is not null)
        {
            button.Click += OnButtonClick;
        }
    }

    private void OnButtonClick(object sender, RoutedEventArgs e)
    {
        if (ContextMenu is { } menu)
        {
            menu.PlacementTarget = this;
            menu.Placement = PlacementMode.Bottom;
            menu.IsOpen = true;
        }
    }
}
