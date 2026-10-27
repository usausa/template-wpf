namespace Template.WindowsApp.Shell;

public abstract partial class NavigationNode : ObservableObject
{
    public string Title { get; }

    public string Icon { get; }

    public string Emoji { get; }

    [ObservableProperty]
    public partial int Badge { get; set; }

    protected NavigationNode(string title, string icon, string emoji)
    {
        Title = title;
        Icon = icon;
        Emoji = emoji;
    }
}
