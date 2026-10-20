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
                new(ViewId.Text, "Text", "\uE8D2"),
                new(ViewId.DateTime, "Date & time", "\uEC92")
            ]),
        new NavigationGroup(
            "Collections",
            "\uE8A9",
            [
                new(ViewId.DataGrid, "Data grid", "\uE80A"),
                new(ViewId.ListDetail, "List & detail", "\uE779"),
                new(ViewId.Hierarchy, "Hierarchy", "\uEABB")
            ]),
        new NavigationGroup(
            "Forms",
            "\uF0E3",
            [
                new(ViewId.Validation, "Validation", "\uE930"),
                new(ViewId.Wizard, "Wizard", "\uEF90")
            ]),
        new NavigationGroup(
            "Feedback",
            "\uE946",
            [
                new(ViewId.Dialogs, "Dialogs", "\uE737"),
                new(ViewId.Progress, "Progress", "\uF16A"),
                new(ViewId.Notifications, "Notifications", "\uEA8F")
            ]),
        new NavigationGroup(
            "Layout",
            "\uECA5",
            [
                new(ViewId.Panels, "Panels", "\uE8A1"),
                new(ViewId.Containers, "Containers", "\uE90C")
            ]),
        new NavigationGroup(
            "Design",
            "\uEB3C",
            [
                new(ViewId.Colors, "Colors", "\uE790"),
                new(ViewId.Typography, "Typography", "\uE8E9"),
                new(ViewId.Icons, "Icons", "\uED58")
            ]),
        new NavigationGroup(
            "Graphics",
            "\uEC87",
            [
                new(ViewId.ShapesAnimation, "Shapes & animation", "\uF158"),
                new(ViewId.Charts, "Charts", "\uE9D2")
            ]),
        new NavigationGroup(
            "System",
            "\uE770",
            [
                new(ViewId.ClipboardDrag, "Clipboard & drag", "\uE77F"),
                new(ViewId.Environment, "Environment", "\uE9D9")
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
