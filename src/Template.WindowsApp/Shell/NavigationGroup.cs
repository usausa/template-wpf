namespace Template.WindowsApp.Shell;

public sealed partial class NavigationGroup : NavigationNode
{
    public IReadOnlyList<NavigationItem> Items { get; }

    [ObservableProperty]
    public partial bool IsExpanded { get; set; }

    [ObservableProperty]
    public partial bool IsChildSelected { get; set; }

    public NavigationGroup(string title, string icon, string emoji, IReadOnlyList<NavigationItem> items)
        : base(title, icon, emoji)
    {
        Items = items;
    }
}
