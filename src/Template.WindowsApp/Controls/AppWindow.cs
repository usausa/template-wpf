namespace Template.WindowsApp.Controls;

using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Shell;

using BunnyTail.XamlProperty;

public partial class AppWindow : Window
{
    private Button? minimizeButton;

    private CaptionButton? maximizeButton;

    private Button? closeButton;

    private FrameworkElement? captionButtons;

    private FrameworkElement? overlayTitleBar;

    private NativeMethods.Rect? lockedBounds;

    private bool lockedZoomed;

    [DependencyProperty]
    public partial object? LeftHeader { get; set; }

    [DependencyProperty]
    public partial object? TitleBarContent { get; set; }

    [DependencyProperty]
    public partial object? RightHeader { get; set; }

    [DependencyProperty(DefaultValue = true)]
    public partial bool ShowIconOnTitleBar { get; set; }

    [DependencyProperty]
    public partial object? OverlayContent { get; set; }

    [DependencyProperty(PropertyChanged = nameof(OnIsLockedChanged))]
    public partial bool IsLocked { get; set; }

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
            if (IsLocked)
            {
                Lock(source.Handle);
            }
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
        if (captionButtons is not null)
        {
            captionButtons.SizeChanged -= OnCaptionButtonsSizeChanged;
        }

        minimizeButton = GetTemplateChild("PART_MinimizeButton") as Button;
        maximizeButton = GetTemplateChild("PART_MaximizeButton") as CaptionButton;
        closeButton = GetTemplateChild("PART_CloseButton") as Button;
        captionButtons = GetTemplateChild("PART_CaptionButtons") as FrameworkElement;
        overlayTitleBar = GetTemplateChild("PART_OverlayTitleBar") as FrameworkElement;

        if (captionButtons is not null)
        {
            captionButtons.SizeChanged += OnCaptionButtonsSizeChanged;
        }

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
            case NativeMethods.WmSysCommand when IsLocked && IsLockedCommand(NativeMethods.ToSystemCommand(wParam)):
                handled = true;
                break;
            case NativeMethods.WmInitMenuPopup when IsLocked && NativeMethods.IsSystemMenu(hwnd, wParam):
                NativeMethods.GraySystemMenu(hwnd, WindowState == WindowState.Maximized);
                break;
            case NativeMethods.WmWindowPosChanging when IsLocked && (lockedBounds is { } bounds) && !NativeMethods.IsIconic(hwnd) && (NativeMethods.IsZoomed(hwnd) == lockedZoomed):
                NativeMethods.KeepWindowPos(lParam, bounds);
                break;
            case NativeMethods.WmWindowPosChanged when IsLocked && !NativeMethods.IsIconic(hwnd) && ((lockedBounds is null) || (NativeMethods.IsZoomed(hwnd) != lockedZoomed)):
                StoreLockedBounds(hwnd);
                break;
            case NativeMethods.WmNcHitTest when IsLocked && IsOnResizeBorder(hwnd, lParam):
                handled = true;
                return NativeMethods.HtBorder;
            case NativeMethods.WmNcHitTest when !IsLocked && IsOverMaximizeButton(lParam):
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

    private void OnIsLockedChanged()
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        if (IsLocked)
        {
            Lock(hwnd);
        }
        else
        {
            lockedBounds = null;
            NativeMethods.ResetSystemMenu(hwnd);
        }
    }

    private void Lock(IntPtr hwnd)
    {
        StoreLockedBounds(hwnd);
        NativeMethods.GraySystemMenu(hwnd, WindowState == WindowState.Maximized);
    }

    private void StoreLockedBounds(IntPtr hwnd)
    {
        lockedBounds = !NativeMethods.IsIconic(hwnd) && NativeMethods.GetWindowRect(hwnd, out var rect) ? rect : null;
        lockedZoomed = NativeMethods.IsZoomed(hwnd);
    }

    private bool IsLockedCommand(int command) =>
        command switch
        {
            NativeMethods.ScClose or NativeMethods.ScMaximize or NativeMethods.ScMove or NativeMethods.ScSize => true,
            NativeMethods.ScRestore => WindowState == WindowState.Maximized,
            _ => false
        };

    private bool IsOnResizeBorder(IntPtr hwnd, IntPtr lParam)
    {
        if ((WindowState != WindowState.Normal) || (WindowChrome.GetWindowChrome(this) is not { } chrome) || !NativeMethods.GetWindowRect(hwnd, out var rect))
        {
            return false;
        }

        var dpi = VisualTreeHelper.GetDpi(this);
        var border = chrome.ResizeBorderThickness;
        var value = lParam.ToInt64();
        var x = unchecked((short)value);
        var y = unchecked((short)(value >> 16));
        return (x < rect.Left + (border.Left * dpi.DpiScaleX)) ||
               (x >= rect.Right - (border.Right * dpi.DpiScaleX)) ||
               (y < rect.Top + (border.Top * dpi.DpiScaleY)) ||
               (y >= rect.Bottom - (border.Bottom * dpi.DpiScaleY));
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

    private void OnCaptionButtonsSizeChanged(object sender, SizeChangedEventArgs e) =>
        overlayTitleBar?.Margin = new Thickness(0, 0, e.NewSize.Width, 0);
}
