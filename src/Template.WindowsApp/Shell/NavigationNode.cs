namespace Template.WindowsApp.Shell;

public abstract class NavigationNode : ObservableObject
{
    public string Title { get; }

    public string Icon { get; }

    protected NavigationNode(string title, string icon)
    {
        Title = title;
        Icon = icon;
    }
}
