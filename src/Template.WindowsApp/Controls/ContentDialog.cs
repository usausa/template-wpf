namespace Template.WindowsApp.Controls;

using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using BunnyTail.XamlProperty;

[TemplatePart(Name = PrimaryButtonPartName, Type = typeof(ButtonBase))]
[TemplatePart(Name = CloseButtonPartName, Type = typeof(ButtonBase))]
public sealed partial class ContentDialog : ContentControl
{
    private const string PrimaryButtonPartName = "PART_PrimaryButton";

    private const string CloseButtonPartName = "PART_CloseButton";

    private ButtonBase? primaryButton;

    private ButtonBase? closeButton;

    [DependencyProperty]
    public partial string? Title { get; set; }

    [DependencyProperty]
    public partial string? PrimaryButtonText { get; set; }

    [DependencyProperty]
    public partial string? SecondaryButtonText { get; set; }

    [DependencyProperty]
    public partial string? CloseButtonText { get; set; }

    [DependencyProperty]
    public partial ICommand? PrimaryButtonCommand { get; set; }

    [DependencyProperty]
    public partial ICommand? SecondaryButtonCommand { get; set; }

    [DependencyProperty]
    public partial ICommand? CloseButtonCommand { get; set; }

    static ContentDialog()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ContentDialog), new FrameworkPropertyMetadata(typeof(ContentDialog)));
    }

    public ContentDialog()
    {
        Loaded += OnLoaded;
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        primaryButton = GetTemplateChild(PrimaryButtonPartName) as ButtonBase;
        closeButton = GetTemplateChild(CloseButtonPartName) as ButtonBase;
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new ContentDialogAutomationPeer(this);

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (IsKeyboardFocusWithin)
        {
            return;
        }

        if (primaryButton is { IsVisible: true, IsEnabled: true } button)
        {
            button.Focus();
        }
        else
        {
            closeButton?.Focus();
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Handled)
        {
            return;
        }

        if ((e.Key == Key.Enter) && (PrimaryButtonText is not null))
        {
            Execute(PrimaryButtonCommand);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            Execute(CloseButtonCommand);
            e.Handled = true;
        }
    }

    private static void Execute(ICommand? command)
    {
        if ((command is not null) && command.CanExecute(null))
        {
            command.Execute(null);
        }
    }
}
