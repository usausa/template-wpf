namespace Template.WindowsApp.Services;

using Template.WindowsApp.Controls;
using Template.WindowsApp.Mocks;

public sealed class NotificationServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ShowAddsNewestFirstToHistoryAndPopups()
    {
        // Arrange
        var service = new NotificationService(new FixedTimeProvider(Now));

        // Act
        service.Show(InfoBarSeverity.Informational, "First", "Message");
        service.Show(InfoBarSeverity.Success, "Second", "Message");

        // Assert
        Assert.Equal(["Second", "First"], service.History.Select(static x => x.Title));
        Assert.Equal(["First", "Second"], service.Items.Select(static x => x.Title));
        Assert.Equal(2, service.UnreadCount);
        Assert.Equal(Now.DateTime, service.History[0].Time);
    }

    [Fact]
    public void ShowKeepsThreePopups()
    {
        // Arrange
        var service = new NotificationService(new FixedTimeProvider(Now));

        // Act
        for (var i = 1; i <= 4; i++)
        {
            service.Show(InfoBarSeverity.Informational, $"N{i}", "Message");
        }

        // Assert
        Assert.Equal(["N2", "N3", "N4"], service.Items.Select(static x => x.Title));
        Assert.Equal(4, service.History.Count);
    }

    [Fact]
    public void ShowWithoutPopupsAddsOnlyToHistory()
    {
        // Arrange
        var service = new NotificationService(new FixedTimeProvider(Now)) { ShowsPopups = false };

        // Act
        service.Show(InfoBarSeverity.Error, "Failed", "Message");

        // Assert
        Assert.Empty(service.Items);
        Assert.Single(service.History);
        Assert.Equal(1, service.UnreadCount);
    }

    [Theory]
    [InlineData(InfoBarSeverity.Informational)]
    [InlineData(InfoBarSeverity.Success)]
    [InlineData(InfoBarSeverity.Warning)]
    [InlineData(InfoBarSeverity.Error)]
    public void ShowSkipsPopupOfHiddenType(InfoBarSeverity severity)
    {
        // Arrange
        var service = new NotificationService(new FixedTimeProvider(Now))
        {
            ShowsInformational = severity != InfoBarSeverity.Informational,
            ShowsSuccess = severity != InfoBarSeverity.Success,
            ShowsWarning = severity != InfoBarSeverity.Warning,
            ShowsError = severity != InfoBarSeverity.Error
        };

        // Act
        service.Show(severity, "Hidden", "Message");

        // Assert
        Assert.Empty(service.Items);
        Assert.Single(service.History);
    }

    [Fact]
    public void HistoryKeepsFiftyNotifications()
    {
        // Arrange
        var service = new NotificationService(new FixedTimeProvider(Now)) { ShowsPopups = false };

        // Act
        for (var i = 1; i <= 55; i++)
        {
            service.Show(InfoBarSeverity.Informational, $"N{i}", "Message");
        }

        // Assert
        Assert.Equal(50, service.History.Count);
        Assert.Equal("N55", service.History[0].Title);
        Assert.Equal(50, service.UnreadCount);
    }

    [Fact]
    public void MarkAllAsReadKeepsHistory()
    {
        // Arrange
        var service = new NotificationService(new FixedTimeProvider(Now)) { ShowsPopups = false };
        service.Show(InfoBarSeverity.Informational, "First", "Message");

        // Act
        service.MarkAllAsRead();

        // Assert
        Assert.Equal(0, service.UnreadCount);
        Assert.Single(service.History);
    }

    [Fact]
    public void ClearHistoryRemovesNotifications()
    {
        // Arrange
        var service = new NotificationService(new FixedTimeProvider(Now)) { ShowsPopups = false };
        service.Show(InfoBarSeverity.Informational, "First", "Message");

        // Act
        service.ClearHistory();

        // Assert
        Assert.Empty(service.History);
        Assert.Equal(0, service.UnreadCount);
    }

    [Fact]
    public void CloseRemovesPopup()
    {
        // Arrange
        var service = new NotificationService(new FixedTimeProvider(Now));
        service.Show(InfoBarSeverity.Informational, "First", "Message");

        // Act
        service.Close(service.Items[0]);

        // Assert
        Assert.Empty(service.Items);
        Assert.Single(service.History);
    }
}
