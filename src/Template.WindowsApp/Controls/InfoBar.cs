namespace Template.WindowsApp.Controls;

using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using BunnyTail.XamlProperty;

using Template.WindowsApp.Models;

public sealed partial class InfoBar : Control
{
    private ButtonBase? closeButton;

    [DependencyProperty]
    public partial InfoBarSeverity Severity { get; set; }

    [DependencyProperty]
    public partial string? Title { get; set; }

    [DependencyProperty]
    public partial string? Message { get; set; }

    [DependencyProperty(DefaultValue = true, Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)]
    public partial bool IsOpen { get; set; }

    [DependencyProperty(DefaultValue = true)]
    public partial bool IsClosable { get; set; }

    [DependencyProperty]
    public partial ButtonBase? ActionButton { get; set; }

    [DependencyProperty]
    public partial ICommand? CloseButtonCommand { get; set; }

    [DependencyProperty]
    public partial object? CloseButtonCommandParameter { get; set; }

    static InfoBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(InfoBar), new FrameworkPropertyMetadata(typeof(InfoBar)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (closeButton is not null)
        {
            closeButton.Click -= OnCloseClick;
        }

        closeButton = GetTemplateChild("PART_CloseButton") as ButtonBase;

        if (closeButton is not null)
        {
            closeButton.Click += OnCloseClick;
        }
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new InfoBarAutomationPeer(this);

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        IsOpen = false;
        if ((CloseButtonCommand is { } command) && command.CanExecute(CloseButtonCommandParameter))
        {
            command.Execute(CloseButtonCommandParameter);
        }
    }
}
