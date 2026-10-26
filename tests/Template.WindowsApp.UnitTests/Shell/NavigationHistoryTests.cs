namespace Template.WindowsApp.Shell;

using Template.WindowsApp.Views;

public sealed class NavigationHistoryTests
{
    [Fact]
    public void NewHistoryCannotMove()
    {
        // Arrange
        var history = new NavigationHistory();

        // Act
        var target = history.Begin(-1);

        // Assert
        Assert.Null(target);
        Assert.False(history.CanGoBack);
        Assert.False(history.CanGoForward);
    }

    [Fact]
    public void VisitAddsPagesInOrder()
    {
        // Arrange
        var history = new NavigationHistory();

        // Act
        history.Visit(ViewId.Home);
        history.Visit(ViewId.Logs);

        // Assert
        Assert.True(history.CanGoBack);
        Assert.False(history.CanGoForward);
    }

    [Fact]
    public void GoingBackKeepsForwardPage()
    {
        // Arrange
        var history = new NavigationHistory();
        history.Visit(ViewId.Home);
        history.Visit(ViewId.Logs);

        // Act
        var target = Move(history, -1);

        // Assert
        Assert.Equal(ViewId.Home, target);
        Assert.False(history.CanGoBack);
        Assert.True(history.CanGoForward);
    }

    [Fact]
    public void GoingForwardReturnsNextPage()
    {
        // Arrange
        var history = new NavigationHistory();
        history.Visit(ViewId.Home);
        history.Visit(ViewId.Logs);
        Move(history, -1);

        // Act
        var target = Move(history, 1);

        // Assert
        Assert.Equal(ViewId.Logs, target);
        Assert.False(history.CanGoForward);
    }

    [Fact]
    public void VisitAfterGoingBackDropsForwardPages()
    {
        // Arrange
        var history = new NavigationHistory();
        history.Visit(ViewId.Home);
        history.Visit(ViewId.Logs);
        history.Visit(ViewId.Settings);
        Move(history, -1);

        // Act
        history.Visit(ViewId.Messages);

        // Assert
        Assert.False(history.CanGoForward);
        Assert.Equal(ViewId.Logs, history.Begin(-1));
    }

    [Fact]
    public void VisitSamePageAgainAddsNothing()
    {
        // Arrange
        var history = new NavigationHistory();
        history.Visit(ViewId.Home);

        // Act
        history.Visit(ViewId.Home);

        // Assert
        Assert.False(history.CanGoBack);
    }

    [Fact]
    public void HistoryKeepsFiftyPages()
    {
        // Arrange
        var history = new NavigationHistory();
        for (var i = 0; i < 60; i++)
        {
            history.Visit(i % 2 == 0 ? ViewId.Home : ViewId.Logs);
        }

        // Act
        var moves = 0;
        while (Move(history, -1) is not null)
        {
            moves++;
        }

        // Assert
        Assert.Equal(49, moves);
    }

    private static ViewId? Move(NavigationHistory history, int offset)
    {
        var target = history.Begin(offset);
        if (target is { } id)
        {
            history.Visit(id);
        }
        history.End();
        return target;
    }
}
