namespace Template.WindowsApp.Shell;

using Template.WindowsApp.Views;

public sealed class NavigationMenuTests
{
    [Fact]
    public void EnumerateItemsListsEachPageOnce()
    {
        // Arrange
        var menu = new NavigationMenu();
        menu.AddFavorite(ViewId.DataGrid);

        // Act
        var ids = Ids(menu.EnumerateItems());

        // Assert
        Assert.Equal(Enum.GetValues<ViewId>(), ids);
    }

    [Fact]
    public void GroupsDoNotIncludeFavorites()
    {
        // Arrange
        var menu = new NavigationMenu();
        menu.AddFavorite(ViewId.DataGrid);

        // Act
        var groups = menu.Groups.ToList();

        // Assert
        Assert.DoesNotContain(menu.Favorites, groups);
        Assert.Equal(10, groups.Count);
    }

    [Fact]
    public void FavoritesFollowMenuOrder()
    {
        // Arrange
        var menu = new NavigationMenu();

        // Act
        menu.AddFavorite(ViewId.Logs);
        menu.AddFavorite(ViewId.DataGrid);
        menu.AddFavorite(ViewId.Settings);
        menu.AddFavorite(ViewId.Charts);

        // Assert
        Assert.Equal([ViewId.DataGrid, ViewId.Charts, ViewId.Logs, ViewId.Settings], Ids(menu.Favorites.Items));
        Assert.True(menu.IsFavorite(ViewId.Charts));
        Assert.False(menu.IsFavorite(ViewId.Icons));
    }

    [Fact]
    public void AddingSameFavoriteAgainChangesNothing()
    {
        // Arrange
        var menu = new NavigationMenu();
        menu.AddFavorite(ViewId.Logs);
        var changed = 0;
        menu.Changed += (_, _) => changed++;

        // Act
        menu.AddFavorite(ViewId.Logs);

        // Assert
        Assert.Single(menu.Favorites.Items);
        Assert.Equal(0, changed);
    }

    [Fact]
    public void FavoritesDoNotShareSelectionAndBadge()
    {
        // Arrange
        var menu = new NavigationMenu();
        menu.AddFavorite(ViewId.Messages);

        // Act
        menu.Select(ViewId.Messages);
        menu.SetBadge(ViewId.Messages, 3);

        // Assert
        var favorite = menu.Favorites.Items[0];
        Assert.False(favorite.IsSelected);
        Assert.Equal(0, favorite.Badge);
        Assert.False(menu.Favorites.IsChildSelected);
        Assert.Equal(0, menu.Favorites.Badge);
        var original = menu.EnumerateItems().First(static x => x.Id == ViewId.Messages);
        Assert.True(original.IsSelected);
        Assert.Equal(3, original.Badge);
    }

    [Fact]
    public void RemoveFavoriteRaisesChanged()
    {
        // Arrange
        var menu = new NavigationMenu();
        menu.AddFavorite(ViewId.DataGrid);
        menu.AddFavorite(ViewId.Logs);
        var changed = 0;
        menu.Changed += (_, _) => changed++;

        // Act
        menu.RemoveFavorite(ViewId.DataGrid);
        menu.RemoveFavorite(ViewId.Icons);

        // Assert
        Assert.Equal([ViewId.Logs], Ids(menu.Favorites.Items));
        Assert.Equal(1, changed);
    }

    [Fact]
    public void ClearFavoritesRemovesAll()
    {
        // Arrange
        var menu = new NavigationMenu();
        menu.AddFavorite(ViewId.DataGrid);
        menu.AddFavorite(ViewId.Logs);
        var changed = 0;
        menu.Changed += (_, _) => changed++;

        // Act
        menu.ClearFavorites();
        menu.ClearFavorites();

        // Assert
        Assert.Empty(menu.Favorites.Items);
        Assert.Equal(1, changed);
    }

    [Fact]
    public void RecentPagesStartWithNewest()
    {
        // Arrange
        var menu = new NavigationMenu();

        // Act
        menu.AddRecentPage(ViewId.DataGrid);
        menu.AddRecentPage(ViewId.Logs);
        menu.AddRecentPage(ViewId.Charts);
        menu.AddRecentPage(ViewId.DataGrid);

        // Assert
        Assert.Equal([ViewId.DataGrid, ViewId.Charts, ViewId.Logs], Ids(menu.RecentPages));
    }

    [Fact]
    public void RecentPagesSkipHomeAndSamePageAgain()
    {
        // Arrange
        var menu = new NavigationMenu();
        menu.AddRecentPage(ViewId.Logs);
        var changed = 0;
        menu.Changed += (_, _) => changed++;

        // Act
        menu.AddRecentPage(ViewId.Home);
        menu.AddRecentPage(ViewId.Logs);

        // Assert
        Assert.Equal([ViewId.Logs], Ids(menu.RecentPages));
        Assert.Equal(0, changed);
    }

    [Fact]
    public void RecentPagesKeepFive()
    {
        // Arrange
        var menu = new NavigationMenu();

        // Act
        foreach (var id in new[] { ViewId.Buttons, ViewId.Choices, ViewId.Text, ViewId.DateTime, ViewId.DataGrid, ViewId.Logs, ViewId.Settings })
        {
            menu.AddRecentPage(id);
        }

        // Assert
        Assert.Equal([ViewId.Settings, ViewId.Logs, ViewId.DataGrid, ViewId.DateTime, ViewId.Text], Ids(menu.RecentPages));
    }

    [Fact]
    public void RemoveAndClearRecentPages()
    {
        // Arrange
        var menu = new NavigationMenu();
        menu.AddRecentPage(ViewId.DataGrid);
        menu.AddRecentPage(ViewId.Logs);

        // Act
        menu.RemoveRecentPage(ViewId.DataGrid);
        var afterRemove = Ids(menu.RecentPages);
        menu.ClearRecentPages();

        // Assert
        Assert.Equal([ViewId.Logs], afterRemove);
        Assert.Empty(menu.RecentPages);
    }

    [Fact]
    public void RestoreSkipsUnknownPages()
    {
        // Arrange
        var menu = new NavigationMenu();
        var changed = 0;
        menu.Changed += (_, _) => changed++;

        // Act
        menu.Restore(
            ["Logs", "Unknown", "DataGrid"],
            ["Charts", "Home", "Charts", "Unknown", "Logs", "Buttons", "Text", "Colors", "Icons"]);

        // Assert
        Assert.Equal([ViewId.DataGrid, ViewId.Logs], Ids(menu.Favorites.Items));
        Assert.Equal([ViewId.Charts, ViewId.Logs, ViewId.Buttons, ViewId.Text, ViewId.Colors], Ids(menu.RecentPages));
        Assert.Equal(1, changed);
    }

    private static ViewId[] Ids(IEnumerable<NavigationItem> items) => [.. items.Select(static x => x.Id)];
}
