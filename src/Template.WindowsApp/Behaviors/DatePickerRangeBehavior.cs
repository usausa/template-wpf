namespace Template.WindowsApp.Behaviors;

using System.Windows.Controls;

using BunnyTail.XamlProperty;

using Microsoft.Xaml.Behaviors;

public sealed partial class DatePickerRangeBehavior : Behavior<DatePicker>
{
    [DependencyProperty(PropertyChanged = nameof(OnRangeChanged))]
    public partial DateTime? Minimum { get; set; }

    [DependencyProperty(PropertyChanged = nameof(OnRangeChanged))]
    public partial DateTime? Maximum { get; set; }

    protected override void OnAttached()
    {
        base.OnAttached();
        UpdateBlackoutDates();
    }

    private void OnRangeChanged() => UpdateBlackoutDates();

    private void UpdateBlackoutDates()
    {
        if (AssociatedObject is null)
        {
            return;
        }

        var dates = AssociatedObject.BlackoutDates;
        dates.Clear();
        if ((Minimum is { } minimum) && (minimum > DateTime.MinValue))
        {
            dates.Add(new CalendarDateRange(DateTime.MinValue, minimum.AddDays(-1)));
        }
        if ((Maximum is { } maximum) && (maximum < DateTime.MaxValue.Date))
        {
            dates.Add(new CalendarDateRange(maximum.AddDays(1), DateTime.MaxValue));
        }
    }
}
