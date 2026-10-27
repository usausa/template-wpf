namespace Template.WindowsApp.Controls;

using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using BunnyTail.XamlProperty;

using Template.WindowsApp.Models;

[TemplatePart(Name = ScrollViewerPartName, Type = typeof(ScrollViewer))]
[TemplatePart(Name = AddButtonPartName, Type = typeof(ButtonBase))]
public sealed partial class TabView : TabControl
{
    private const string ScrollViewerPartName = "PART_ScrollViewer";

    private const string AddButtonPartName = "PART_AddButton";

    private ScrollViewer? scrollViewer;

    [DependencyProperty]
    public partial TabViewWidthMode TabWidthMode { get; set; }

    [DependencyProperty]
    public partial TabViewCloseButtonOverlayMode CloseButtonOverlayMode { get; set; }

    [DependencyProperty]
    public partial ICommand? AddTabButtonCommand { get; set; }

    [DependencyProperty]
    public partial ICommand? CloseTabCommand { get; set; }

    internal ButtonBase? AddTabButton { get; private set; }

    static TabView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TabView), new FrameworkPropertyMetadata(typeof(TabView)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (scrollViewer is not null)
        {
            scrollViewer.PreviewMouseWheel -= OnPreviewMouseWheel;
        }

        scrollViewer = GetTemplateChild(ScrollViewerPartName) as ScrollViewer;
        AddTabButton = GetTemplateChild(AddButtonPartName) as ButtonBase;

        if (scrollViewer is not null)
        {
            scrollViewer.PreviewMouseWheel += OnPreviewMouseWheel;
        }
    }

    protected override DependencyObject GetContainerForItemOverride() => new TabViewItem();

    protected override bool IsItemItsOwnContainerOverride(object item) => item is TabViewItem;

    protected override AutomationPeer OnCreateAutomationPeer() => new TabViewAutomationPeer(this);

    internal void RequestClose(TabViewItem container)
    {
        var item = ItemContainerGenerator.ItemFromContainer(container);
        if ((CloseTabCommand is { } command) && command.CanExecute(item))
        {
            command.Execute(item);
        }
    }

    private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if ((scrollViewer is null) || (scrollViewer.ExtentWidth <= scrollViewer.ViewportWidth))
        {
            return;
        }

        scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta);
        e.Handled = true;
    }
}
