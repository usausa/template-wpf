namespace Template.WindowsApp.Views.Layout;

public sealed partial class DocumentTab : ObservableObject
{
    public string Title { get; }

    [ObservableProperty]
    public partial string Text { get; set; }

    public DocumentTab(string title, string text)
    {
        Title = title;
        Text = text;
    }

    public override string ToString() => Title;
}
