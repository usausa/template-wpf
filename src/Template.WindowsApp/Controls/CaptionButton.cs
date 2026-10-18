namespace Template.WindowsApp.Controls;

using System.Windows.Controls;

using BunnyTail.XamlProperty;

public sealed partial class CaptionButton : Button
{
    [DependencyProperty]
    public partial bool IsNonClientMouseOver { get; set; }

    [DependencyProperty]
    public partial bool IsNonClientPressed { get; set; }

    static CaptionButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CaptionButton), new FrameworkPropertyMetadata(typeof(CaptionButton)));
    }
}
