namespace Template.WindowsApp.Shell;

using Template.WindowsApp.Views;

public sealed class NavigationMenu
{
    public IReadOnlyList<NavigationNode> Items { get; } =
    [
        new NavigationItem(ViewId.Home, "Home", "\uE80F"),
        new NavigationGroup(
            "Basic input",
            "\uE961",
            [
                new(ViewId.Buttons, "Buttons", "\uEDA4"),
                new(ViewId.Choices, "Choices", "\uE73A"),
                new(ViewId.Text, "Text", "\uE8D2")
            ]),
        new NavigationGroup(
            "Feedback",
            "\uE946",
            [
                new(ViewId.Dialogs, "Dialogs", "\uE737")
            ])
    ];

    public IReadOnlyList<NavigationItem> FooterItems { get; } =
    [
        new(ViewId.Settings, "Settings", "\uE713")
    ];

    public IEnumerable<NavigationGroup> Groups => Items.OfType<NavigationGroup>();

    public IEnumerable<NavigationItem> EnumerateItems()
    {
        foreach (var node in Items)
        {
            if (node is NavigationGroup group)
            {
                foreach (var child in group.Items)
                {
                    yield return child;
                }
            }
            else if (node is NavigationItem item)
            {
                yield return item;
            }
        }

        foreach (var item in FooterItems)
        {
            yield return item;
        }
    }

    public void Select(ViewId? id)
    {
        foreach (var item in EnumerateItems())
        {
            item.IsSelected = item.Id == id;
        }

        foreach (var group in Groups)
        {
            group.IsChildSelected = group.Items.Any(static x => x.IsSelected);
            if (group.IsChildSelected)
            {
                group.IsExpanded = true;
            }
        }
    }
}
