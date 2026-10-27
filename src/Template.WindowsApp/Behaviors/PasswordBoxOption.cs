namespace Template.WindowsApp.Behaviors;

using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;

using BunnyTail.XamlProperty;

public static partial class PasswordBoxOption
{
    private const string CapsLockMessage = "Caps Lock is on";

    private static readonly DependencyProperty AdornerProperty = DependencyProperty.RegisterAttached(
        "Adorner",
        typeof(RevealAdorner),
        typeof(PasswordBoxOption),
        new PropertyMetadata(null));

    private static readonly DependencyProperty CapsLockToolTipProperty = DependencyProperty.RegisterAttached(
        "CapsLockToolTip",
        typeof(ToolTip),
        typeof(PasswordBoxOption),
        new PropertyMetadata(null));

    [AttachedProperty(PropertyChanged = nameof(OnIsRevealButtonEnabledChanged))]
    public static partial bool GetIsRevealButtonEnabled(PasswordBox passwordBox);

    public static partial void SetIsRevealButtonEnabled(PasswordBox passwordBox, bool value);

    [AttachedProperty(PropertyChanged = nameof(OnIsCapsLockWarningEnabledChanged))]
    public static partial bool GetIsCapsLockWarningEnabled(PasswordBox passwordBox);

    public static partial void SetIsCapsLockWarningEnabled(PasswordBox passwordBox, bool value);

    //--------------------------------------------------------------------------------
    // Reveal button
    //--------------------------------------------------------------------------------

    private static void OnIsRevealButtonEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PasswordBox passwordBox)
        {
            return;
        }

        passwordBox.Loaded -= OnRevealLoaded;
        passwordBox.Unloaded -= OnRevealUnloaded;
        if (!(bool)e.NewValue)
        {
            DetachReveal(passwordBox);
            return;
        }

        passwordBox.Loaded += OnRevealLoaded;
        passwordBox.Unloaded += OnRevealUnloaded;
        if (passwordBox.IsLoaded)
        {
            AttachReveal(passwordBox);
        }
    }

    private static void OnRevealLoaded(object sender, RoutedEventArgs e) => AttachReveal((PasswordBox)sender);

    private static void OnRevealUnloaded(object sender, RoutedEventArgs e) => DetachReveal((PasswordBox)sender);

    private static void AttachReveal(PasswordBox passwordBox)
    {
        if ((passwordBox.GetValue(AdornerProperty) is null) && (AdornerLayer.GetAdornerLayer(passwordBox) is { } layer))
        {
            var adorner = new RevealAdorner(passwordBox);
            layer.Add(adorner);
            passwordBox.SetValue(AdornerProperty, adorner);
        }
    }

    private static void DetachReveal(PasswordBox passwordBox)
    {
        if (passwordBox.GetValue(AdornerProperty) is RevealAdorner adorner)
        {
            adorner.Release();
            (VisualTreeHelper.GetParent(adorner) as AdornerLayer)?.Remove(adorner);
            passwordBox.ClearValue(AdornerProperty);
        }
    }

    //--------------------------------------------------------------------------------
    // Caps Lock warning
    //--------------------------------------------------------------------------------

    private static void OnIsCapsLockWarningEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PasswordBox passwordBox)
        {
            return;
        }

        passwordBox.GotKeyboardFocus -= OnCapsLockGotFocus;
        passwordBox.LostKeyboardFocus -= OnCapsLockLostFocus;
        passwordBox.PreviewKeyUp -= OnCapsLockKeyUp;
        passwordBox.Unloaded -= OnCapsLockUnloaded;
        if (!(bool)e.NewValue)
        {
            CloseCapsLockWarning(passwordBox);
            return;
        }

        passwordBox.GotKeyboardFocus += OnCapsLockGotFocus;
        passwordBox.LostKeyboardFocus += OnCapsLockLostFocus;
        passwordBox.PreviewKeyUp += OnCapsLockKeyUp;
        passwordBox.Unloaded += OnCapsLockUnloaded;
        UpdateCapsLockWarning(passwordBox);
    }

    private static void OnCapsLockGotFocus(object sender, KeyboardFocusChangedEventArgs e) => UpdateCapsLockWarning((PasswordBox)sender);

    private static void OnCapsLockLostFocus(object sender, KeyboardFocusChangedEventArgs e) => CloseCapsLockWarning((PasswordBox)sender);

    private static void OnCapsLockKeyUp(object sender, KeyEventArgs e) => UpdateCapsLockWarning((PasswordBox)sender);

    private static void OnCapsLockUnloaded(object sender, RoutedEventArgs e) => CloseCapsLockWarning((PasswordBox)sender);

    private static void UpdateCapsLockWarning(PasswordBox passwordBox)
    {
        var show = passwordBox.IsKeyboardFocused && Keyboard.IsKeyToggled(Key.CapsLock);
        var toolTip = passwordBox.GetValue(CapsLockToolTipProperty) as ToolTip;
        if (show && (toolTip is null))
        {
            toolTip = new ToolTip
            {
                Content = CapsLockMessage,
                PlacementTarget = passwordBox,
                Placement = PlacementMode.Bottom
            };
            passwordBox.SetValue(CapsLockToolTipProperty, toolTip);
        }

        toolTip?.IsOpen = show;
    }

    private static void CloseCapsLockWarning(PasswordBox passwordBox)
    {
        if (passwordBox.GetValue(CapsLockToolTipProperty) is ToolTip toolTip)
        {
            toolTip.IsOpen = false;
        }
    }

    //--------------------------------------------------------------------------------
    // Adorner
    //--------------------------------------------------------------------------------

    private sealed class RevealAdorner : Adorner
    {
        private const string RevealGlyph = "\uF78D";

        private const double ButtonWidth = 30;

        private const double ButtonMargin = 4;

        private const double GlyphFontSize = 12;

        private const double FocusedBorderBottom = 2;

        private readonly PasswordBox passwordBox;

        private readonly Grid root;

        private readonly Border cover;

        private readonly TextBlock text;

        private readonly Border button;

        private readonly TextBlock glyph;

        protected override int VisualChildrenCount => 1;

        public RevealAdorner(PasswordBox passwordBox)
            : base(passwordBox)
        {
            this.passwordBox = passwordBox;
            IsClipEnabled = true;

            var border = passwordBox.BorderThickness;
            var padding = passwordBox.Padding;

            text = new TextBlock
            {
                Margin = new Thickness(padding.Left + TextBoxOption.TextOffset, padding.Top, 0, 0)
            };
            text.SetResourceReference(TextBlock.ForegroundProperty, "TextControlForegroundFocused");
            Bind(TextBlock.FontFamilyProperty, Control.FontFamilyProperty);
            Bind(TextBlock.FontSizeProperty, Control.FontSizeProperty);
            Bind(TextBlock.FontStyleProperty, Control.FontStyleProperty);
            Bind(TextBlock.FontWeightProperty, Control.FontWeightProperty);
            Bind(TextBlock.FontStretchProperty, Control.FontStretchProperty);

            cover = new Border
            {
                Margin = new Thickness(border.Left, border.Top, ButtonWidth + (ButtonMargin * 2), FocusedBorderBottom),
                Visibility = Visibility.Collapsed,
                Child = text
            };
            cover.SetResourceReference(Border.BackgroundProperty, "TextControlBackgroundFocused");

            glyph = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = GlyphFontSize,
                Text = RevealGlyph
            };
            glyph.SetResourceReference(TextBlock.FontFamilyProperty, "SymbolThemeFontFamily");

            button = new Border
            {
                Width = ButtonWidth,
                Margin = new Thickness(0, ButtonMargin, ButtonMargin, ButtonMargin),
                HorizontalAlignment = HorizontalAlignment.Right,
                Visibility = Visibility.Collapsed,
                Cursor = Cursors.Arrow,
                Child = glyph
            };

            root = new Grid();
            root.Children.Add(cover);
            root.Children.Add(button);
            AddVisualChild(root);

            button.MouseEnter += OnButtonMouseChanged;
            button.MouseLeave += OnButtonMouseChanged;
            button.MouseLeftButtonDown += OnButtonMouseLeftButtonDown;
            button.MouseLeftButtonUp += OnButtonMouseLeftButtonUp;
            button.LostMouseCapture += OnButtonLostMouseCapture;
            passwordBox.PasswordChanged += OnPasswordChanged;
            passwordBox.GotKeyboardFocus += OnFocusChanged;
            passwordBox.LostKeyboardFocus += OnFocusChanged;

            UpdateButtonBrushes();
            UpdateButton();
        }

        public void Release()
        {
            button.MouseEnter -= OnButtonMouseChanged;
            button.MouseLeave -= OnButtonMouseChanged;
            button.MouseLeftButtonDown -= OnButtonMouseLeftButtonDown;
            button.MouseLeftButtonUp -= OnButtonMouseLeftButtonUp;
            button.LostMouseCapture -= OnButtonLostMouseCapture;
            passwordBox.PasswordChanged -= OnPasswordChanged;
            passwordBox.GotKeyboardFocus -= OnFocusChanged;
            passwordBox.LostKeyboardFocus -= OnFocusChanged;
            button.ReleaseMouseCapture();
            BindingOperations.ClearAllBindings(text);
            text.Text = string.Empty;
        }

        protected override Visual GetVisualChild(int index) => root;

        protected override Size MeasureOverride(Size constraint)
        {
            var size = AdornedElement.RenderSize;
            root.Measure(size);
            return size;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            root.Arrange(new Rect(finalSize));
            return finalSize;
        }

        private void Bind(DependencyProperty target, DependencyProperty source) =>
            text.SetBinding(target, new Binding { Source = passwordBox, Path = new PropertyPath(source) });

        private void OnButtonMouseChanged(object sender, MouseEventArgs e) => UpdateButtonBrushes();

        private void OnButtonMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            button.CaptureMouse();
            Reveal(true);
            e.Handled = true;
        }

        private void OnButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            button.ReleaseMouseCapture();
            e.Handled = true;
        }

        private void OnButtonLostMouseCapture(object sender, MouseEventArgs e) => Reveal(false);

        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (cover.IsVisible)
            {
                text.Text = passwordBox.Password;
            }
            UpdateButton();
        }

        private void OnFocusChanged(object sender, KeyboardFocusChangedEventArgs e) => UpdateButton();

        private void Reveal(bool reveal)
        {
            text.Text = reveal ? passwordBox.Password : string.Empty;
            cover.Visibility = reveal ? Visibility.Visible : Visibility.Collapsed;
            UpdateButtonBrushes();
        }

        private void UpdateButton()
        {
            var visible = passwordBox.IsKeyboardFocused && (passwordBox.Password.Length > 0);
            button.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            if (!visible)
            {
                button.ReleaseMouseCapture();
            }
        }

        private void UpdateButtonBrushes()
        {
            var (background, foreground) = button.IsMouseCaptured
                ? ("TextControlButtonBackgroundPressed", "TextControlButtonForegroundPressed")
                : button.IsMouseOver
                    ? ("TextControlButtonBackgroundPointerOver", "TextControlButtonForegroundPointerOver")
                    : ("TextControlButtonBackground", "TextControlButtonForeground");
            button.SetResourceReference(Border.BackgroundProperty, background);
            glyph.SetResourceReference(TextBlock.ForegroundProperty, foreground);
        }
    }
}
