namespace Template.WindowsApp.Views.Home;

public sealed partial class Summary : ObservableObject
{
    public string Emoji { get; }

    public string Title { get; }

    [ObservableProperty]
    public partial string Value { get; set; }

    [ObservableProperty]
    public partial string Detail { get; set; }

    public Summary(string emoji, string title, string value, string detail)
    {
        Emoji = emoji;
        Title = title;
        Value = value;
        Detail = detail;
    }
}
