namespace Template.WindowsApp.Controls;

using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

using BunnyTail.XamlProperty;

[TemplatePart(Name = ToggleButtonPartName, Type = typeof(ToggleButton))]
[TemplatePart(Name = HourTextPartName, Type = typeof(TextBlock))]
[TemplatePart(Name = MinuteTextPartName, Type = typeof(TextBlock))]
[TemplatePart(Name = HourColumnPartName, Type = typeof(TimePickerColumn))]
[TemplatePart(Name = MinuteColumnPartName, Type = typeof(TimePickerColumn))]
[TemplatePart(Name = AcceptButtonPartName, Type = typeof(ButtonBase))]
[TemplatePart(Name = CancelButtonPartName, Type = typeof(ButtonBase))]
public sealed partial class TimePicker : Control
{
    private const string ToggleButtonPartName = "PART_ToggleButton";

    private const string HourTextPartName = "PART_HourText";

    private const string MinuteTextPartName = "PART_MinuteText";

    private const string HourColumnPartName = "PART_HourColumn";

    private const string MinuteColumnPartName = "PART_MinuteColumn";

    private const string AcceptButtonPartName = "PART_AcceptButton";

    private const string CancelButtonPartName = "PART_CancelButton";

    private const string HourPlaceholder = "hour";

    private const string MinutePlaceholder = "minute";

    private const int MinutesPerHour = 60;

    private static readonly string[] HourValues = Enumerable.Range(0, 24).Select(static x => x.ToString("00", CultureInfo.InvariantCulture)).ToArray();

    private ToggleButton? toggleButton;

    private TextBlock? hourText;

    private TextBlock? minuteText;

    private TimePickerColumn? hourColumn;

    private TimePickerColumn? minuteColumn;

    private ButtonBase? acceptButton;

    private ButtonBase? cancelButton;

    private int[] minuteValues = MakeMinuteValues(1);

    [DependencyProperty(Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PropertyChanged = nameof(OnSelectedTimeChanged))]
    public partial TimeSpan? SelectedTime { get; set; }

    [DependencyProperty(DefaultValue = 1, PropertyChanged = nameof(OnMinuteIncrementChanged))]
    public partial int MinuteIncrement { get; set; }

    [DependencyProperty(Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PropertyChanged = nameof(OnIsDropDownOpenChanged))]
    public partial bool IsDropDownOpen { get; set; }

    static TimePicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TimePicker), new FrameworkPropertyMetadata(typeof(TimePicker)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (acceptButton is not null)
        {
            acceptButton.Click -= OnAcceptClick;
        }
        if (cancelButton is not null)
        {
            cancelButton.Click -= OnCancelClick;
        }

        toggleButton = GetTemplateChild(ToggleButtonPartName) as ToggleButton;
        hourText = GetTemplateChild(HourTextPartName) as TextBlock;
        minuteText = GetTemplateChild(MinuteTextPartName) as TextBlock;
        hourColumn = GetTemplateChild(HourColumnPartName) as TimePickerColumn;
        minuteColumn = GetTemplateChild(MinuteColumnPartName) as TimePickerColumn;
        acceptButton = GetTemplateChild(AcceptButtonPartName) as ButtonBase;
        cancelButton = GetTemplateChild(CancelButtonPartName) as ButtonBase;

        if (acceptButton is not null)
        {
            acceptButton.Click += OnAcceptClick;
        }
        if (cancelButton is not null)
        {
            cancelButton.Click += OnCancelClick;
        }

        hourColumn?.Values = HourValues;
        UpdateMinuteValues();
        UpdateText();
    }

    private void OnSelectedTimeChanged() => UpdateText();

    private void OnMinuteIncrementChanged()
    {
        minuteValues = MakeMinuteValues(MinuteIncrement);
        UpdateMinuteValues();
    }

    private void OnIsDropDownOpenChanged()
    {
        if (!IsDropDownOpen || (hourColumn is not { } hours) || (minuteColumn is not { } minutes))
        {
            return;
        }

        var time = SelectedTime ?? DateTime.Now.TimeOfDay;
        hours.SelectedIndex = time.Hours;
        minutes.SelectedIndex = Math.Max(Array.FindLastIndex(minuteValues, x => x <= time.Minutes), 0);
        Dispatcher.BeginInvoke(() => hours.Focus(), DispatcherPriority.Input);
    }

    private void OnAcceptClick(object sender, RoutedEventArgs e) => Accept();

    private void OnCancelClick(object sender, RoutedEventArgs e) => Cancel();

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (!IsDropDownOpen || e.Handled)
        {
            return;
        }

        if (e.Key == Key.Enter)
        {
            Accept();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            Cancel();
            e.Handled = true;
        }
    }

    private void Accept()
    {
        if ((hourColumn is not null) && (minuteColumn is not null))
        {
            SelectedTime = new TimeSpan(hourColumn.SelectedIndex, minuteValues[minuteColumn.SelectedIndex], 0);
        }
        Close();
    }

    private void Cancel() => Close();

    private void Close()
    {
        IsDropDownOpen = false;
        toggleButton?.Focus();
    }

    private void UpdateMinuteValues()
    {
        minuteColumn?.Values = minuteValues.Select(static x => x.ToString("00", CultureInfo.InvariantCulture)).ToArray();
    }

    private void UpdateText()
    {
        hourText?.Text = SelectedTime is { } time ? time.Hours.ToString("00", CultureInfo.InvariantCulture) : HourPlaceholder;
        minuteText?.Text = SelectedTime is { } value ? value.Minutes.ToString("00", CultureInfo.InvariantCulture) : MinutePlaceholder;
    }

    private static int[] MakeMinuteValues(int increment)
    {
        var step = Math.Clamp(increment, 1, MinutesPerHour);
        return Enumerable.Range(0, (MinutesPerHour + step - 1) / step).Select(x => x * step).ToArray();
    }
}
