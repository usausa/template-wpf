namespace Template.WindowsApp.Behaviors;

public sealed partial class DataGridColumnSetting : ObservableObject
{
    public string Header { get; }

    public bool CanHide { get; }

    [ObservableProperty]
    public partial bool IsVisible { get; set; }

    [ObservableProperty]
    public partial double? Width { get; set; }

    public DataGridColumnSetting(string header, bool canHide)
    {
        Header = header;
        CanHide = canHide;
        IsVisible = true;
    }

    public override string ToString() => Header;
}
