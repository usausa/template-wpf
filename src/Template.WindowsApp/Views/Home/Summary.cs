namespace Template.WindowsApp.Views.Home;

public sealed partial class Summary : ObservableObject
{
    public string Title { get; }

    [ObservableProperty]
    public partial string Value { get; set; }

    [ObservableProperty]
    public partial string Detail { get; set; }

    public Summary(string title, string value, string detail)
    {
        Title = title;
        Value = value;
        Detail = detail;
    }
}
