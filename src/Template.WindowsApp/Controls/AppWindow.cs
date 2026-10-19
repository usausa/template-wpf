namespace Template.WindowsApp.Controls;

using System.Windows.Controls;
using System.Windows.Interop;

using BunnyTail.XamlProperty;

public partial class AppWindow : Window
{
    private Button? minimizeButton;

    private CaptionButton? maximizeButton;

    private Button? closeButton;

    [DependencyProperty]
    public partial object? LeftHeader { get; set; }

    [DependencyProperty]
    public partial object? TitleBarContent { get; set; }

    [DependencyProperty]
    public partial object? RightHeader { get; set; }

    [DependencyProperty(DefaultValue = true)]
    public partial bool ShowIconOnTitleBar { get; set; }

    static AppWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(AppWindow), new FrameworkPropertyMetadata(typeof(AppWindow)));
    }

    public AppWindow()
    {
        SetResourceReference(StyleProperty, typeof(AppWindow));
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        if (PresentationSource.FromVisual(this) is HwndSource source)
        {
            source.AddHook(WndProc);
        }
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (minimizeButton is not null)
        {
            minimizeButton.Click -= OnMinimizeClick;
        }
        if (maximizeButton is not null)
        {
            maximizeButton.Click -= OnMaximizeClick;
        }
        if (closeButton is not null)
        {
            closeButton.Click -= OnCloseClick;
        }

        minimizeButton = GetTemplateChild("PART_MinimizeButton") as Button;
        maximizeButton = GetTemplateChild("PART_MaximizeButton") as CaptionButton;
        closeButton = GetTemplateChild("PART_CloseButton") as Button;

        if (minimizeButton is not null)
        {
            minimizeButton.Click += OnMinimizeClick;
        }
        if (maximizeButton is not null)
        {
            maximizeButton.Click += OnMaximizeClick;
        }
        if (closeButton is not null)
        {
            closeButton.Click += OnCloseClick;
        }
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        switch (msg)
        {
            case NativeMethods.WmNcHitTest when IsOverMaximizeButton(lParam):
                handled = true;
                return NativeMethods.HtMaxButton;
            case NativeMethods.WmNcMouseMove when maximizeButton is { } button:
                button.IsNonClientMouseOver = wParam == NativeMethods.HtMaxButton;
                if (button.IsNonClientMouseOver)
                {
                    NativeMethods.TrackNonClientMouseLeave(hwnd);
                }
                else
                {
                    button.IsNonClientPressed = false;
                }
                break;
            case NativeMethods.WmNcMouseLeave when maximizeButton is { } button:
                button.IsNonClientMouseOver = false;
                button.IsNonClientPressed = false;
                break;
            case NativeMethods.WmNcLButtonDown or NativeMethods.WmNcLButtonDoubleClick when (maximizeButton is { } button) && (wParam == NativeMethods.HtMaxButton):
                button.IsNonClientPressed = true;
                handled = true;
                break;
            case NativeMethods.WmNcLButtonUp when (maximizeButton is { } button) && (wParam == NativeMethods.HtMaxButton):
                if (button.IsNonClientPressed)
                {
                    button.IsNonClientPressed = false;
                    ToggleMaximize();
                }
                handled = true;
                break;
        }

        return IntPtr.Zero;
    }

    private bool IsOverMaximizeButton(IntPtr lParam)
    {
        if ((maximizeButton is not { IsVisible: true } button) || (PresentationSource.FromVisual(button) is null))
        {
            return false;
        }

        var value = lParam.ToInt64();
        var point = button.PointFromScreen(new Point(unchecked((short)value), unchecked((short)(value >> 16))));
        return (point.X >= 0) && (point.X < button.ActualWidth) && (point.Y >= 0) && (point.Y < button.ActualHeight);
    }

    private void ToggleMaximize()
    {
        if (WindowState == WindowState.Maximized)
        {
            SystemCommands.RestoreWindow(this);
        }
        else
        {
            SystemCommands.MaximizeWindow(this);
        }
    }

    private void OnMinimizeClick(object sender, RoutedEventArgs e) => SystemCommands.MinimizeWindow(this);

    private void OnMaximizeClick(object sender, RoutedEventArgs e) => ToggleMaximize();

    private void OnCloseClick(object sender, RoutedEventArgs e) => SystemCommands.CloseWindow(this);
}
