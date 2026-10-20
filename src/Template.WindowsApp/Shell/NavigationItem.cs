namespace Template.WindowsApp.Shell;

using Template.WindowsApp.Views;

public sealed partial class NavigationItem : NavigationNode
{
    public ViewId Id { get; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    public NavigationItem(ViewId id, string title, string icon)
        : base(title, icon)
    {
        Id = id;
    }
}
