namespace Template.WindowsApp.Shell;

using Template.WindowsApp.Views;

public sealed partial class NavigationItem : ObservableObject
{
    public ViewId Id { get; }

    public string Title { get; }

    public string Icon { get; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    public NavigationItem(ViewId id, string title, string icon)
    {
        Id = id;
        Title = title;
        Icon = icon;
    }
}
