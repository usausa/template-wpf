namespace Template.WindowsApp.Services;

using Template.WindowsApp.Models.Sample;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ConversationService : IDisposable
{
    private readonly TimeProvider timeProvider;

    private readonly BehaviorSubject<int> unreadCount = new(0);

    public Person Me { get; } = ChatSample.LoadMe();

    public IReadOnlyList<Conversation> Conversations { get; }

    public IObservable<int> UnreadCount => unreadCount;

    public ConversationService(TimeProvider timeProvider)
    {
        this.timeProvider = timeProvider;

        Conversations = ChatSample.LoadConversations(Me, timeProvider.GetLocalNow().Date);
        UpdateUnreadCount();
    }

    public void Dispose() => unreadCount.Dispose();

    public void Send(Conversation conversation, string text)
    {
        var now = Now();
        conversation.Post(new Message(Me, now, text), now.Date);
    }

    public void Reply(Message root, string text)
    {
        var now = Now();
        root.AddReply(new Message(Me, now, text), now.Date);
    }

    public static void React(Message message, ReactionKind kind)
    {
        if (message.Reactions.FirstOrDefault(x => x.Kind == kind) is { } reaction)
        {
            Toggle(message, reaction);
        }
        else
        {
            message.Reactions.Add(new Reaction(kind, 1, true));
        }
    }

    public void Toggle(Reaction reaction)
    {
        var message = Conversations
            .SelectMany(static x => x.Messages)
            .SelectMany(static x => x.Replies.Prepend(x))
            .FirstOrDefault(x => x.Reactions.Contains(reaction));
        if (message is not null)
        {
            Toggle(message, reaction);
        }
    }

    public void MarkRead(Conversation conversation)
    {
        conversation.UnreadCount = 0;
        UpdateUnreadCount();
    }

    public void MarkAllAsRead()
    {
        foreach (var conversation in Conversations)
        {
            conversation.UnreadCount = 0;
        }
        UpdateUnreadCount();
    }

    private static void Toggle(Message message, Reaction reaction)
    {
        if (reaction.IsMine)
        {
            reaction.IsMine = false;
            reaction.Count--;
            if (reaction.Count == 0)
            {
                message.Reactions.Remove(reaction);
            }
        }
        else
        {
            reaction.IsMine = true;
            reaction.Count++;
        }
    }

    private void UpdateUnreadCount() => unreadCount.OnNext(Conversations.Sum(static x => x.UnreadCount));

    private DateTime Now() => timeProvider.GetLocalNow().DateTime;
}
