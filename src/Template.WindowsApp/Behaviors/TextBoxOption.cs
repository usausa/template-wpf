namespace Template.WindowsApp.Behaviors;

using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Documents;

using BunnyTail.XamlProperty;

public static partial class TextBoxOption
{
    internal const double TextOffset = 2;

    private static readonly DependencyProperty AdornerProperty = DependencyProperty.RegisterAttached(
        "Adorner",
        typeof(PlaceholderAdorner),
        typeof(TextBoxOption),
        new PropertyMetadata(null));

    [AttachedProperty(PropertyChanged = nameof(OnPlaceholderTextChanged))]
    public static partial string? GetPlaceholderText(TextBox textBox);

    public static partial void SetPlaceholderText(TextBox textBox, string? value);

    [AttachedProperty(PropertyChanged = nameof(OnAllowedCharactersChanged))]
    public static partial string? GetAllowedCharacters(TextBox textBox);

    public static partial void SetAllowedCharacters(TextBox textBox, string? value);

    private static void OnPlaceholderTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox textBox)
        {
            return;
        }

        textBox.Loaded -= OnLoaded;
        textBox.Unloaded -= OnUnloaded;
        if (e.NewValue is null)
        {
            Detach(textBox);
            return;
        }

        textBox.Loaded += OnLoaded;
        textBox.Unloaded += OnUnloaded;
        if (textBox.IsLoaded)
        {
            Attach(textBox);
        }
    }

    private static void OnAllowedCharactersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox textBox)
        {
            return;
        }

        textBox.PreviewTextInput -= OnPreviewTextInput;
        DataObject.RemovePastingHandler(textBox, OnPasting);
        if (e.NewValue is not null)
        {
            textBox.PreviewTextInput += OnPreviewTextInput;
            DataObject.AddPastingHandler(textBox, OnPasting);
        }
    }

    private static void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if ((GetAllowedCharacters((TextBox)sender) is { } pattern) && (Regex.Count(e.Text, pattern) != e.Text.Length))
        {
            e.Handled = true;
        }
    }

    private static void OnPasting(object sender, DataObjectPastingEventArgs e)
    {
        if ((GetAllowedCharacters((TextBox)sender) is not { } pattern) || (e.SourceDataObject.GetData(DataFormats.UnicodeText) is not string text))
        {
            return;
        }

        var allowed = String.Concat(Regex.Matches(text, pattern).Select(static x => x.Value));
        if (allowed.Length == 0)
        {
            e.CancelCommand();
        }
        else if (allowed.Length != text.Length)
        {
            e.DataObject = new DataObject(DataFormats.UnicodeText, allowed);
        }
    }

    private static void OnLoaded(object sender, RoutedEventArgs e) => Attach((TextBox)sender);

    private static void OnUnloaded(object sender, RoutedEventArgs e) => Detach((TextBox)sender);

    private static void Attach(TextBox textBox)
    {
        if ((textBox.GetValue(AdornerProperty) is null) && (AdornerLayer.GetAdornerLayer(textBox) is { } layer))
        {
            var adorner = new PlaceholderAdorner(textBox);
            layer.Add(adorner);
            textBox.SetValue(AdornerProperty, adorner);
        }
    }

    private static void Detach(TextBox textBox)
    {
        if (textBox.GetValue(AdornerProperty) is PlaceholderAdorner adorner)
        {
            adorner.Release();
            (VisualTreeHelper.GetParent(adorner) as AdornerLayer)?.Remove(adorner);
            textBox.ClearValue(AdornerProperty);
        }
    }

    private sealed class PlaceholderAdorner : Adorner
    {
        private readonly TextBox textBox;

        private readonly TextBlock text;

        protected override int VisualChildrenCount => 1;

        public PlaceholderAdorner(TextBox textBox)
            : base(textBox)
        {
            this.textBox = textBox;
            IsHitTestVisible = false;
            IsClipEnabled = true;

            text = new TextBlock
            {
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            text.SetResourceReference(TextBlock.ForegroundProperty, "TextControlPlaceholderForeground");
            Bind(TextBlock.TextProperty, PlaceholderTextProperty);
            Bind(TextBlock.FontFamilyProperty, Control.FontFamilyProperty);
            Bind(TextBlock.FontSizeProperty, Control.FontSizeProperty);
            Bind(TextBlock.FontStyleProperty, Control.FontStyleProperty);
            Bind(TextBlock.FontWeightProperty, Control.FontWeightProperty);
            Bind(TextBlock.FontStretchProperty, Control.FontStretchProperty);
            AddVisualChild(text);

            textBox.TextChanged += OnTextChanged;
            textBox.IsVisibleChanged += OnIsVisibleChanged;
            UpdateVisibility();
        }

        public void Release()
        {
            textBox.TextChanged -= OnTextChanged;
            textBox.IsVisibleChanged -= OnIsVisibleChanged;
            BindingOperations.ClearAllBindings(text);
        }

        protected override Visual GetVisualChild(int index) => text;

        protected override Size MeasureOverride(Size constraint)
        {
            var size = AdornedElement.RenderSize;
            text.Measure(new Size(TextWidth(size.Width), size.Height));
            return size;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            text.Arrange(new Rect(TextLeft(), textBox.BorderThickness.Top + textBox.Padding.Top, TextWidth(finalSize.Width), text.DesiredSize.Height));
            return finalSize;
        }

        private double TextLeft() => textBox.BorderThickness.Left + textBox.Padding.Left + TextOffset;

        private double TextWidth(double width) => Math.Max(0, width - TextLeft() - textBox.BorderThickness.Right - textBox.Padding.Right);

        private void Bind(DependencyProperty target, DependencyProperty source) =>
            text.SetBinding(target, new Binding { Source = textBox, Path = new PropertyPath(source) });

        private void OnTextChanged(object sender, TextChangedEventArgs e) => UpdateVisibility();

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) => UpdateVisibility();

        private void UpdateVisibility() =>
            text.Visibility = textBox.IsVisible && String.IsNullOrEmpty(textBox.Text) ? Visibility.Visible : Visibility.Collapsed;
    }
}
