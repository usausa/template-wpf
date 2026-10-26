namespace Template.WindowsApp.Services;

using Template.WindowsApp.Mocks;

public sealed class ConversationServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void UnreadCountStartsWithSampleMessages()
    {
        // Arrange
        using var service = new ConversationService(new FixedTimeProvider(Now));

        // Act
        var count = LatestUnreadCount(service);

        // Assert
        Assert.Equal(4, count);
    }

    [Fact]
    public void MarkReadLowersUnreadCount()
    {
        // Arrange
        using var service = new ConversationService(new FixedTimeProvider(Now));
        var general = service.Conversations.First(static x => x.Name == "general");

        // Act
        service.MarkRead(general);

        // Assert
        Assert.Equal(0, general.UnreadCount);
        Assert.Equal(3, LatestUnreadCount(service));
    }

    [Fact]
    public void MarkAllAsReadClearsUnreadCount()
    {
        // Arrange
        using var service = new ConversationService(new FixedTimeProvider(Now));

        // Act
        service.MarkAllAsRead();

        // Assert
        Assert.All(service.Conversations, static x => Assert.Equal(0, x.UnreadCount));
        Assert.Equal(0, LatestUnreadCount(service));
    }

    [Fact]
    public void SendAddsMessageFromMe()
    {
        // Arrange
        using var service = new ConversationService(new FixedTimeProvider(Now));
        var general = service.Conversations.First(static x => x.Name == "general");

        // Act
        service.Send(general, "Hello");

        // Assert
        var message = general.Messages[^1];
        Assert.Equal(service.Me, message.Author);
        Assert.Equal("Hello", message.Text);
        Assert.True(message.IsToday);
        Assert.False(message.StartsDay);
        Assert.False(message.IsContinuation);
    }

    [Fact]
    public void SendAgainSoonContinuesMessage()
    {
        // Arrange
        var time = new FixedTimeProvider(Now);
        using var service = new ConversationService(time);
        var general = service.Conversations.First(static x => x.Name == "general");
        service.Send(general, "Hello");

        // Act
        time.Current = Now.AddMinutes(5);
        service.Send(general, "Are you there?");

        // Assert
        Assert.True(general.Messages[^1].IsContinuation);
    }

    [Fact]
    public void SendAgainLaterStartsNewMessage()
    {
        // Arrange
        var time = new FixedTimeProvider(Now);
        using var service = new ConversationService(time);
        var general = service.Conversations.First(static x => x.Name == "general");
        service.Send(general, "Hello");

        // Act
        time.Current = Now.AddMinutes(6);
        service.Send(general, "Are you there?");

        // Assert
        Assert.False(general.Messages[^1].IsContinuation);
    }

    [Fact]
    public void ReplyAddsReplyAndReplier()
    {
        // Arrange
        using var service = new ConversationService(new FixedTimeProvider(Now));
        var root = service.Conversations.First(static x => x.Name == "design").Messages[0];

        // Act
        service.Reply(root, "Thanks!");

        // Assert
        Assert.Equal("Thanks!", root.Replies[^1].Text);
        Assert.Contains(service.Me, root.Repliers);
        Assert.Equal(Now.DateTime, root.LastReplyTime);
    }

    [Fact]
    public void ReactAddsMyReaction()
    {
        // Arrange
        var message = new Message(new Person("Aiko Tanaka", AvatarColor.Purple, Presence.Active), Now.DateTime, "Hi");

        // Act
        ConversationService.React(message, ReactionKind.Like);

        // Assert
        var reaction = Assert.Single(message.Reactions);
        Assert.Equal(ReactionKind.Like, reaction.Kind);
        Assert.Equal(1, reaction.Count);
        Assert.True(reaction.IsMine);
    }

    [Fact]
    public void ReactAgainRemovesMyReaction()
    {
        // Arrange
        var message = new Message(new Person("Aiko Tanaka", AvatarColor.Purple, Presence.Active), Now.DateTime, "Hi");
        ConversationService.React(message, ReactionKind.Like);

        // Act
        ConversationService.React(message, ReactionKind.Like);

        // Assert
        Assert.Empty(message.Reactions);
    }

    [Fact]
    public void ToggleOthersReactionAddsMine()
    {
        // Arrange
        using var service = new ConversationService(new FixedTimeProvider(Now));
        var reaction = service.Conversations.SelectMany(static x => x.Messages).SelectMany(static x => x.Reactions).First(static x => !x.IsMine);
        var count = reaction.Count;

        // Act
        service.Toggle(reaction);

        // Assert
        Assert.True(reaction.IsMine);
        Assert.Equal(count + 1, reaction.Count);
    }

    private static int? LatestUnreadCount(ConversationService service)
    {
        int? count = null;
        using var subscription = service.UnreadCount.Subscribe(x => count = x);
        return count;
    }
}
