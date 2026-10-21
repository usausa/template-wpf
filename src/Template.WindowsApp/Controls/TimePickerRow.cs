namespace Template.WindowsApp.Controls;

public sealed class TimePickerRow
{
    public string Text { get; }

    public bool IsSelected { get; }

    public TimePickerRow(string text, bool isSelected)
    {
        Text = text;
        IsSelected = isSelected;
    }

    public override string ToString() => Text;
}
