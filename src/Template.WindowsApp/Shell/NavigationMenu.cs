namespace Template.WindowsApp.Shell;

using Template.WindowsApp.Views;

public sealed class NavigationMenu
{
    private const int RecentPageCount = 5;

    private readonly ObservableCollection<NavigationItem> favoriteItems = [];

    private readonly ObservableCollection<NavigationItem> recentItems = [];

    public IReadOnlyList<NavigationItem> HeaderItems { get; } =
    [
        new(ViewId.Home, "Home", "\uE80F")
    ];

    public NavigationGroup Favorites { get; }

    public IReadOnlyList<NavigationNode> Items { get; } =
    [
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
                new(ViewId.Hierarchy, "Hierarchy", "\uEABB"),
                new(ViewId.Messages, "Messages", "\uE8F2")
            ]),
        new NavigationGroup(
            "Forms",
            "\uF0E3",
            [
                new(ViewId.Validation, "Validation", "\uE930"),
                new(ViewId.Wizard, "Wizard", "\uEF90"),
                new(ViewId.EditableGrid, "Editable grid", "\uEF60"),
                new(ViewId.GridVisuals, "Grid visuals", "\uF404")
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
                new(ViewId.Containers, "Containers", "\uE90C"),
                new(ViewId.MenusToolbars, "Menus & toolbars", "\uEDE3")
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
                new(ViewId.Charts, "Charts", "\uE9D2"),
                new(ViewId.LiveData, "Live data", "\uE93E")
            ]),
        new NavigationGroup(
            "System",
            "\uE770",
            [
                new(ViewId.Windows, "Windows", "\uE78B"),
                new(ViewId.ClipboardDrag, "Clipboard & drag", "\uE77F"),
                new(ViewId.Environment, "Environment", "\uE9D9"),
                new(ViewId.Logs, "Logs", "\uE9A4")
            ])
    ];

    public IReadOnlyList<NavigationItem> FooterItems { get; } =
    [
        new(ViewId.Settings, "Settings", "\uE713")
    ];

    public IReadOnlyList<NavigationItem> RecentPages => recentItems;

    public IEnumerable<NavigationGroup> Groups => Items.OfType<NavigationGroup>();

    public event EventHandler? Changed;

    public NavigationMenu()
    {
        Favorites = new NavigationGroup("Favorites", "\uE734", favoriteItems)
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
                favoriteItems.Add(new NavigationItem(item.Id, item.Title, item.Icon));
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
                favoriteItems.Insert(index, new NavigationItem(item.Id, item.Title, item.Icon));
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
