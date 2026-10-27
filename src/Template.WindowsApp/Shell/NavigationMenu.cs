namespace Template.WindowsApp.Shell;

using Template.WindowsApp.Views;

public sealed class NavigationMenu
{
    private const int RecentPageCount = 5;

    private readonly ObservableCollection<NavigationItem> favoriteItems = [];

    private readonly ObservableCollection<NavigationItem> recentItems = [];

    public IReadOnlyList<NavigationItem> HeaderItems { get; } =
    [
        new(ViewId.Home, "Home", "\uE80F", "\U0001F3E0")
    ];

    public NavigationGroup Favorites { get; }

    public IReadOnlyList<NavigationNode> Items { get; } =
    [
        new NavigationGroup(
            "Fundamentals",
            "\uE8F1",
            "\U0001F9F1",
            [
                new(ViewId.ResourcesStyles, "Resources & styles", "\uEE56", "\U0001F9F0"),
                new(ViewId.Binding, "Binding", "\uE71B", "\U0001F517"),
                new(ViewId.Templates, "Templates", "\uE7C3", "\U0001F9E9"),
                new(ViewId.CustomControls, "Custom controls", "\uEA86", "\U0001F39B\uFE0F"),
                new(ViewId.ScratchPad, "Scratch pad", "\uE943", "\U0001F4DD")
            ]),
        new NavigationGroup(
            "Basic input",
            "\uE961",
            "\u2328\uFE0F",
            [
                new(ViewId.Buttons, "Buttons", "\uEDA4", "\U0001F518"),
                new(ViewId.Choices, "Choices", "\uE73A", "\u2611\uFE0F"),
                new(ViewId.Text, "Text", "\uE8D2", "\U0001F524"),
                new(ViewId.DateTime, "Date & time", "\uEC92", "\U0001F4C5")
            ]),
        new NavigationGroup(
            "Collections",
            "\uE8A9",
            "\U0001F5C2\uFE0F",
            [
                new(ViewId.DataGrid, "Data grid", "\uE80A", "\U0001F4CB"),
                new(ViewId.ListDetail, "List & detail", "\uE779", "\U0001F4C7"),
                new(ViewId.Hierarchy, "Hierarchy", "\uEABB", "\U0001F333"),
                new(ViewId.Messages, "Messages", "\uE8F2", "\U0001F4AC")
            ]),
        new NavigationGroup(
            "Forms",
            "\uF0E3",
            "\U0001F9FE",
            [
                new(ViewId.Validation, "Validation", "\uE930", "\u2705"),
                new(ViewId.Wizard, "Wizard", "\uEF90", "\U0001F9D9"),
                new(ViewId.EditableGrid, "Editable grid", "\uEF60", "\u270F\uFE0F"),
                new(ViewId.GridVisuals, "Grid visuals", "\uF404", "\U0001F4CA")
            ]),
        new NavigationGroup(
            "Feedback",
            "\uE946",
            "\U0001F514",
            [
                new(ViewId.Dialogs, "Dialogs", "\uE737", "\U0001F5E8\uFE0F"),
                new(ViewId.Progress, "Progress", "\uF16A", "\u23F3"),
                new(ViewId.Notifications, "Notifications", "\uEA8F", "\U0001F4E3")
            ]),
        new NavigationGroup(
            "Layout",
            "\uECA5",
            "\U0001F4D0",
            [
                new(ViewId.Panels, "Panels", "\uE8A1", "\U0001F532"),
                new(ViewId.Containers, "Containers", "\uE90C", "\U0001F4E6"),
                new(ViewId.MenusToolbars, "Menus & toolbars", "\uEDE3", "\U0001F6E0\uFE0F"),
                new(ViewId.Tabs, "Tabs", "\uE89A", "\U0001F4D1")
            ]),
        new NavigationGroup(
            "Design",
            "\uEB3C",
            "\U0001F3A8",
            [
                new(ViewId.Colors, "Colors", "\uE790", "\U0001F308"),
                new(ViewId.Typography, "Typography", "\uE8E9", "\U0001F520"),
                new(ViewId.Icons, "Icons", "\uED58", "\U0001F523")
            ]),
        new NavigationGroup(
            "Graphics",
            "\uEC87",
            "\U0001F5BC\uFE0F",
            [
                new(ViewId.ShapesAnimation, "Shapes & animation", "\uF158", "\U0001F537"),
                new(ViewId.Charts, "Charts", "\uE9D2", "\U0001F4C8"),
                new(ViewId.LiveData, "Live data", "\uE93E", "\U0001F4E1")
            ]),
        new NavigationGroup(
            "System",
            "\uE770",
            "\U0001F5A5\uFE0F",
            [
                new(ViewId.Windows, "Windows", "\uE78B", "\U0001FA9F"),
                new(ViewId.ClipboardDrag, "Clipboard & drag", "\uE77F", "\U0001F4CE"),
                new(ViewId.Environment, "Environment", "\uE9D9", "\U0001F30D"),
                new(ViewId.Logs, "Logs", "\uE9A4", "\U0001F4DC")
            ])
    ];

    public IReadOnlyList<NavigationItem> FooterItems { get; } =
    [
        new(ViewId.Settings, "Settings", "\uE713", "\u2699\uFE0F")
    ];

    public IReadOnlyList<NavigationItem> RecentPages => recentItems;

    public IEnumerable<NavigationGroup> Groups => Items.OfType<NavigationGroup>();

    public event EventHandler? Changed;

    public NavigationMenu()
    {
        Favorites = new NavigationGroup("Favorites", "\uE734", "\u2B50", favoriteItems)
        {
            IsExpanded = true
        };
    }

    public IEnumerable<NavigationItem> EnumerateItems()
    {
        foreach (var item in HeaderItems)
        {
            yield return item;
        }

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

    public void SetBadge(ViewId id, int count)
    {
        foreach (var item in EnumerateItems())
        {
            if (item.Id == id)
            {
                item.Badge = count;
            }
        }

        foreach (var group in Groups)
        {
            group.Badge = group.Items.Sum(static x => x.Badge);
        }
    }

    public void Restore(IEnumerable<string> favoriteNames, IEnumerable<string> recentNames)
    {
        var favorites = favoriteNames.ToHashSet();
        favoriteItems.Clear();
        foreach (var item in EnumerateItems())
        {
            if (favorites.Contains(item.Id.ToString()))
            {
                favoriteItems.Add(new NavigationItem(item.Id, item.Title, item.Icon, item.Emoji));
            }
        }

        var items = EnumerateItems().ToDictionary(static x => x.Id.ToString());
        recentItems.Clear();
        foreach (var name in recentNames)
        {
            if ((recentItems.Count < RecentPageCount) &&
                items.TryGetValue(name, out var item) &&
                (item.Id != ViewId.Home) &&
                !recentItems.Contains(item))
            {
                recentItems.Add(item);
            }
        }

        RaiseChanged();
    }

    public bool IsFavorite(ViewId id) => favoriteItems.Any(x => x.Id == id);

    public void AddFavorite(ViewId id)
    {
        if (IsFavorite(id))
        {
            return;
        }

        var index = 0;
        foreach (var item in EnumerateItems())
        {
            if (item.Id == id)
            {
                favoriteItems.Insert(index, new NavigationItem(item.Id, item.Title, item.Icon, item.Emoji));
                RaiseChanged();
                return;
            }

            if ((index < favoriteItems.Count) && (favoriteItems[index].Id == item.Id))
            {
                index++;
            }
        }
    }

    public void RemoveFavorite(ViewId id)
    {
        if (Remove(favoriteItems, id))
        {
            RaiseChanged();
        }
    }

    public void ClearFavorites()
    {
        if (favoriteItems.Count > 0)
        {
            favoriteItems.Clear();
            RaiseChanged();
        }
    }

    public void AddRecentPage(ViewId id)
    {
        if ((id == ViewId.Home) ||
            ((recentItems.Count > 0) && (recentItems[0].Id == id)) ||
            (EnumerateItems().FirstOrDefault(x => x.Id == id) is not { } item))
        {
            return;
        }

        Remove(recentItems, id);
        recentItems.Insert(0, item);
        while (recentItems.Count > RecentPageCount)
        {
            recentItems.RemoveAt(recentItems.Count - 1);
        }
        RaiseChanged();
    }

    public void RemoveRecentPage(ViewId id)
    {
        if (Remove(recentItems, id))
        {
            RaiseChanged();
        }
    }

    public void ClearRecentPages()
    {
        if (recentItems.Count > 0)
        {
            recentItems.Clear();
            RaiseChanged();
        }
    }

    private static bool Remove(ObservableCollection<NavigationItem> items, ViewId id)
    {
        for (var i = 0; i < items.Count; i++)
        {
            if (items[i].Id == id)
            {
                items.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    private void RaiseChanged() => Changed?.Invoke(this, EventArgs.Empty);
}
