namespace Template.WindowsApp.Controls;

using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

using BunnyTail.XamlProperty;

[TemplatePart(Name = CloseButtonPartName, Type = typeof(ButtonBase))]
public sealed partial class TabViewItem : TabItem
{
    private const string CloseButtonPartName = "PART_CloseButton";

    private ButtonBase? closeButton;

    [DependencyProperty]
    public partial string? Glyph { get; set; }

    static TabViewItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TabViewItem), new FrameworkPropertyMetadata(typeof(TabViewItem)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (closeButton is not null)
        {
            closeButton.Click -= OnCloseClick;
        }

        closeButton = GetTemplateChild(CloseButtonPartName) as ButtonBase;

        if (closeButton is not null)
        {
            closeButton.Click += OnCloseClick;
        }
    }

    protected override void OnSelected(RoutedEventArgs e)
    {
        base.OnSelected(e);
        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(BringIntoView));
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.ChangedButton == MouseButton.Middle)
        {
            RequestClose();
            e.Handled = true;
        }
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => RequestClose();

    private void RequestClose()
    {
        if (ItemsControl.ItemsControlFromItemContainer(this) is TabView owner)
        {
            owner.RequestClose(this);
        }
    }
}
