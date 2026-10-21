namespace Template.WindowsApp.Controls;

using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using BunnyTail.XamlProperty;

[TemplatePart(Name = TextBoxPartName, Type = typeof(TextBox))]
[TemplatePart(Name = UpButtonPartName, Type = typeof(RepeatButton))]
[TemplatePart(Name = DownButtonPartName, Type = typeof(RepeatButton))]
public sealed partial class NumberBox : Control
{
    private const string TextBoxPartName = "PART_TextBox";

    private const string UpButtonPartName = "PART_UpButton";

    private const string DownButtonPartName = "PART_DownButton";

    private const int InitialInterval = 100;

    private const int MinimumInterval = 20;

    private const int IntervalStep = 10;

    private const int MaxDecimalPlaces = 15;

    private const NumberStyles ParseStyles = NumberStyles.Float | NumberStyles.AllowThousands;

    private TextBox? textBox;

    private RepeatButton? upButton;

    private RepeatButton? downButton;

    [DependencyProperty(Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PropertyChanged = nameof(OnValueChanged))]
    public partial double? Value { get; set; }

    [DependencyProperty(DefaultValue = double.MinValue, PropertyChanged = nameof(OnRangeChanged))]
    public partial double Minimum { get; set; }

    [DependencyProperty(DefaultValue = double.MaxValue, PropertyChanged = nameof(OnRangeChanged))]
    public partial double Maximum { get; set; }

    [DependencyProperty(DefaultValue = 1d)]
    public partial double SmallChange { get; set; }

    [DependencyProperty(DefaultValue = 10d)]
    public partial double LargeChange { get; set; }

    [DependencyProperty(PropertyChanged = nameof(OnValueChanged))]
    public partial int DecimalPlaces { get; set; }

    private int Digits => Math.Clamp(DecimalPlaces, 0, MaxDecimalPlaces);

    static NumberBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(NumberBox), new FrameworkPropertyMetadata(typeof(NumberBox)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (textBox is not null)
        {
            textBox.PreviewKeyDown -= OnTextBoxPreviewKeyDown;
            textBox.LostKeyboardFocus -= OnTextBoxLostKeyboardFocus;
        }
        if (upButton is not null)
        {
            upButton.PreviewMouseLeftButtonDown -= OnSpinButtonPress;
            upButton.Click -= OnUpClick;
        }
        if (downButton is not null)
        {
            downButton.PreviewMouseLeftButtonDown -= OnSpinButtonPress;
            downButton.Click -= OnDownClick;
        }

        textBox = GetTemplateChild(TextBoxPartName) as TextBox;
        upButton = GetTemplateChild(UpButtonPartName) as RepeatButton;
        downButton = GetTemplateChild(DownButtonPartName) as RepeatButton;

        if (textBox is not null)
        {
            textBox.PreviewKeyDown += OnTextBoxPreviewKeyDown;
            textBox.LostKeyboardFocus += OnTextBoxLostKeyboardFocus;
        }
        if (upButton is not null)
        {
            upButton.PreviewMouseLeftButtonDown += OnSpinButtonPress;
            upButton.Click += OnUpClick;
        }
        if (downButton is not null)
        {
            downButton.PreviewMouseLeftButtonDown += OnSpinButtonPress;
            downButton.Click += OnDownClick;
        }

        UpdateText();
        UpdateButtons();
    }

    protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
    {
        base.OnPreviewMouseWheel(e);

        if ((textBox is not null) && textBox.IsKeyboardFocused && (e.Delta != 0))
        {
            Step(e.Delta > 0 ? SmallChange : -SmallChange);
            e.Handled = true;
        }
    }

    private void OnValueChanged()
    {
        UpdateText();
        UpdateButtons();
    }

    private void OnRangeChanged() => UpdateButtons();

    private void OnTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                Commit();
                break;
            case Key.Up:
                Step(SmallChange);
                break;
            case Key.Down:
                Step(-SmallChange);
                break;
            case Key.PageUp:
                Step(LargeChange);
                break;
            case Key.PageDown:
                Step(-LargeChange);
                break;
            default:
                return;
        }

        e.Handled = true;
    }

    private void OnTextBoxLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => Commit();

    private static void OnSpinButtonPress(object sender, MouseButtonEventArgs e) => ((RepeatButton)sender).Interval = InitialInterval;

    private void OnUpClick(object sender, RoutedEventArgs e) => Spin((RepeatButton)sender, SmallChange);

    private void OnDownClick(object sender, RoutedEventArgs e) => Spin((RepeatButton)sender, -SmallChange);

    private void Spin(RepeatButton button, double change)
    {
        button.Interval = Math.Max(MinimumInterval, button.Interval - IntervalStep);
        Step(change);
    }

    private void Step(double change)
    {
        Commit();
        SetCurrentValue(ValueProperty, Normalize((Value ?? Normalize(0)) + change));
    }

    private void Commit()
    {
        if (textBox is null)
        {
            return;
        }

        var text = textBox.Text.Trim();
        if (text.Length == 0)
        {
            SetCurrentValue(ValueProperty, null);
        }
        else if (Double.TryParse(text, ParseStyles, CultureInfo.CurrentCulture, out var value))
        {
            SetCurrentValue(ValueProperty, Normalize(value));
        }

        UpdateText();
    }

    private double Normalize(double value) => Math.Round(Math.Min(Math.Max(value, Minimum), Maximum), Digits);

    private void UpdateText()
    {
        if (textBox is null)
        {
            return;
        }

        var text = Value?.ToString($"F{Digits}", CultureInfo.CurrentCulture) ?? string.Empty;
        if (textBox.Text != text)
        {
            textBox.Text = text;
        }
    }

    private void UpdateButtons()
    {
        var value = Value;
        upButton?.IsEnabled = !value.HasValue || (value.Value < Maximum);
        downButton?.IsEnabled = !value.HasValue || (value.Value > Minimum);
    }
}
