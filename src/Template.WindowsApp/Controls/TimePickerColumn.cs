namespace Template.WindowsApp.Controls;

using System.Windows.Controls;

using BunnyTail.XamlProperty;

public sealed partial class TimePickerColumn : Control
{
    public const int RowCount = 7;

    private const int CenterRow = RowCount / 2;

    [DependencyProperty(PropertyChanged = nameof(OnValuesChanged))]
    public partial IReadOnlyList<string>? Values { get; set; }

    [DependencyProperty(Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PropertyChanged = nameof(OnSelectedIndexChanged))]
    public partial int SelectedIndex { get; set; }

    [DependencyProperty]
    public partial IReadOnlyList<TimePickerRow>? Rows { get; set; }

    static TimePickerColumn()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TimePickerColumn), new FrameworkPropertyMetadata(typeof(TimePickerColumn)));
        FocusableProperty.OverrideMetadata(typeof(TimePickerColumn), new FrameworkPropertyMetadata(true));
    }

    private void OnValuesChanged() => UpdateRows();

    private void OnSelectedIndexChanged() => UpdateRows();

    private void UpdateRows()
    {
        if (Values is not { Count: > 0 } values)
        {
            Rows = [];
            return;
        }

        var rows = new TimePickerRow[RowCount];
        for (var i = 0; i < RowCount; i++)
        {
            rows[i] = new TimePickerRow(values[Wrap(SelectedIndex + i - CenterRow, values.Count)], i == CenterRow);
        }
        Rows = rows;
    }

    private void Move(int offset)
    {
        if (Values is { Count: > 0 } values)
        {
            SelectedIndex = Wrap(SelectedIndex + offset, values.Count);
        }
    }

    private static int Wrap(int index, int count) => ((index % count) + count) % count;

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        Move(e.Delta > 0 ? -1 : 1);
        e.Handled = true;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        Focus();
        e.Handled = true;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        if (ActualHeight > 0)
        {
            var row = (int)(e.GetPosition(this).Y * RowCount / ActualHeight);
            Move(Math.Clamp(row, 0, RowCount - 1) - CenterRow);
        }
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Up)
        {
            Move(-1);
            e.Handled = true;
        }
        else if (e.Key == Key.Down)
        {
            Move(1);
            e.Handled = true;
        }
    }
}
