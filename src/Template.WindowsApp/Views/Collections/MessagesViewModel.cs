namespace Template.WindowsApp.Views.Collections;

using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class MessagesViewModel : AppViewModelBase, IShellCommandProvider
{
    private static readonly TimeSpan GroupSpan = TimeSpan.FromMinutes(5);

    private readonly TimeProvider timeProvider;

    private readonly Person me = new("Dana Kim", AvatarColor.Blue, Presence.Active);

    private Message? firstUnread;

    public IReadOnlyList<Conversation> Conversations { get; }

    [ObservableProperty]
    public partial Conversation? Selected { get; set; }

    [ObservableProperty(NotifyAlso = [nameof(IsThreadOpen)])]
    public partial Message? Thread { get; set; }

    public bool IsThreadOpen => Thread is not null;

    [ObservableProperty]
    public partial string ReplyDraft { get; set; }

    public EventRequest ScrollRequest { get; } = new();

    public EventRequest ThreadScrollRequest { get; } = new();

    public IObserveCommand SendCommand { get; }

    public IObserveCommand ReplyCommand { get; }

    public IObserveCommand OpenThreadCommand { get; }

    public IObserveCommand CloseThreadCommand { get; }

    public IObserveCommand ToggleReactionCommand { get; }

    public IObserveCommand LikeCommand { get; }

    public IObserveCommand HeartCommand { get; }

    public IObserveCommand AcceptCommand { get; }

    public IObserveCommand LaughCommand { get; }

    public IObserveCommand MarkAllAsReadCommand { get; }

    public MessagesViewModel(TimeProvider timeProvider)
    {
        this.timeProvider = timeProvider;

        ReplyDraft = string.Empty;
        Conversations = MakeConversations(timeProvider.GetLocalNow().Date);

        SubscribeSelected(Open);
        SubscribeThread(_ =>
        {
            ReplyDraft = string.Empty;
            ThreadScrollRequest.Request();
        });

        SendCommand = MakeDelegateCommand(Send, () => !String.IsNullOrWhiteSpace(Selected?.Draft));
        ReplyCommand = MakeDelegateCommand(Reply, () => IsThreadOpen && !String.IsNullOrWhiteSpace(ReplyDraft));
        OpenThreadCommand = MakeDelegateCommand<Message>(x => Thread = x);
        CloseThreadCommand = MakeDelegateCommand(() => Thread = null, () => IsThreadOpen);
        ToggleReactionCommand = MakeDelegateCommand<Reaction>(ToggleReaction);
        LikeCommand = MakeDelegateCommand<Message>(static x => React(x, ReactionKind.Like));
        HeartCommand = MakeDelegateCommand<Message>(static x => React(x, ReactionKind.Heart));
        AcceptCommand = MakeDelegateCommand<Message>(static x => React(x, ReactionKind.Accept));
        LaughCommand = MakeDelegateCommand<Message>(static x => React(x, ReactionKind.Laugh));
        MarkAllAsReadCommand = MakeDelegateCommand(MarkAllAsRead, () => Conversations.Any(static x => x.UnreadCount > 0));

        Selected = Conversations[0];
        Thread = Conversations[0].Messages.First(static x => x.Replies.Count > 0);
    }

    private List<Conversation> MakeConversations(DateTime today)
    {
        var yesterday = today.AddDays(-1);
        var aiko = new Person("Aiko Tanaka", AvatarColor.Purple, Presence.Active);
        var ben = new Person("Ben Carter", AvatarColor.Green, Presence.Away);
        var chen = new Person("Chen Wei", AvatarColor.Orange, Presence.Active);

        var general = Conversation.Channel("general", "Team-wide announcements", 12);
        Post(general, new Message(aiko, At(yesterday, 16, 20), "The new build is ready for review."));
        Post(general, new Message(aiko, At(yesterday, 16, 21), "I put it on the test machine."));
        Post(general, new Message(ben, At(yesterday, 16, 45), "Thanks! I'll check it tonight.")
        {
            Reactions = { new(ReactionKind.Like, 2, true), new(ReactionKind.Accept, 1, false) }
        });
        Post(general, new Message(chen, At(today, 9, 12), "Morning! The test report is attached.")
        {
            Attachment = new("test-report.pdf", 120)
        });
        Post(general, new Message(me, At(today, 9, 30), "I'll join the review at 11:00."));
        var warnings = Post(general, new Message(aiko, At(today, 10, 5), "Can someone look at the warnings in the log?"));
        AddReply(warnings, new Message(ben, At(today, 10, 20), "They come from the leak check. Nothing is wrong."));
        AddReply(warnings, new Message(chen, At(today, 10, 31), "I'll add a note to the guide."));
        AddReply(warnings, new Message(aiko, At(today, 10, 42), "Thanks, both!")
        {
            Reactions = { new(ReactionKind.Heart, 1, false) }
        });
        general.UnreadCount = 1;

        var design = Conversation.Channel("design", "Screens, icons and colors", 8);
        Post(design, new Message(chen, At(yesterday, 15, 10), "I updated the icons on the settings page."));
        Post(design, new Message(me, At(yesterday, 15, 24), "Looks good. Can we try a larger size?"));
        Post(design, new Message(chen, At(today, 9, 40), "I tried 20 and 24 pixels."));
        Post(design, new Message(aiko, At(today, 9, 55), "24 is easier to read.")
        {
            Reactions = { new(ReactionKind.Like, 1, false) }
        });
        design.UnreadCount = 2;

        var releases = Conversation.Channel("releases", "Release notes and schedules", 12);
        Post(releases, new Message(ben, At(yesterday, 18, 0), "Version 2.4.0 is out.")
        {
            Reactions = { new(ReactionKind.Accept, 3, false), new(ReactionKind.Heart, 1, true) }
        });
        Post(releases, new Message(aiko, At(today, 8, 45), "The next release is planned for Friday."));

        var random = Conversation.Channel("random", "Anything else", 12);
        Post(random, new Message(chen, At(yesterday, 12, 5), "Lunch at the new noodle place?")
        {
            Reactions = { new(ReactionKind.Like, 2, true) }
        });
        Post(random, new Message(ben, At(today, 8, 30), "The coffee machine works again.")
        {
            Reactions = { new(ReactionKind.Laugh, 2, false), new(ReactionKind.Heart, 1, false) }
        });

        var withAiko = Conversation.Direct(aiko);
        Post(withAiko, new Message(aiko, At(yesterday, 17, 5), "Do you have time to look at the wizard page tomorrow?"));
        Post(withAiko, new Message(me, At(yesterday, 17, 12), "Sure, after the review."));
        Post(withAiko, new Message(aiko, At(today, 10, 15), "The wizard page looks good now. Thanks!")
        {
            Reactions = { new(ReactionKind.Heart, 1, true) }
        });

        var withBen = Conversation.Direct(ben);
        Post(withBen, new Message(ben, At(yesterday, 17, 40), "I'll be out this afternoon."));
        Post(withBen, new Message(ben, At(today, 8, 10), "Can you send me the log from the test machine?"));
        withBen.UnreadCount = 1;

        var withChen = Conversation.Direct(chen);
        Post(withChen, new Message(me, At(yesterday, 14, 0), "Thanks for the report template."));
        Post(withChen, new Message(chen, At(yesterday, 14, 3), "You're welcome!")
        {
            Reactions = { new(ReactionKind.Heart, 1, true) }
        });

        return [general, design, releases, random, withAiko, withBen, withChen];
    }

    private static DateTime At(DateTime day, int hour, int minute) => day.AddHours(hour).AddMinutes(minute);

    private static void React(Message message, ReactionKind kind)
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

    private DateTime Now() => timeProvider.GetLocalNow().DateTime;

    private Message Post(Conversation conversation, Message message)
    {
        Append(conversation.Messages, message);
        return message;
    }

    private void AddReply(Message root, Message reply)
    {
        Append(root.Replies, reply);
        if (!root.Repliers.Contains(reply.Author))
        {
            root.Repliers.Add(reply.Author);
        }
        root.LastReplyTime = reply.Time;
    }

    private void Append(ObservableCollection<Message> messages, Message message)
    {
        var previous = messages.Count > 0 ? messages[^1] : null;
        message.StartsDay = previous?.Time.Date != message.Time.Date;
        message.IsToday = message.Time.Date == Now().Date;
        message.IsContinuation = (previous is not null) && !message.StartsDay && (previous.Author == message.Author) && ((message.Time - previous.Time) <= GroupSpan);
        messages.Add(message);
    }

    private void Open(Conversation? conversation)
    {
        if (firstUnread is not null)
        {
            firstUnread.IsFirstUnread = false;
            firstUnread = null;
        }

        Thread = null;

        if (conversation is { UnreadCount: > 0 })
        {
            firstUnread = conversation.Messages[^conversation.UnreadCount];
            firstUnread.IsFirstUnread = true;
            conversation.UnreadCount = 0;
        }

        ScrollRequest.Request();
    }

    private void Send()
    {
        if ((Selected is not { } conversation) || String.IsNullOrWhiteSpace(conversation.Draft))
        {
            return;
        }

        Append(conversation.Messages, new Message(me, Now(), conversation.Draft.Trim()));
        conversation.Draft = string.Empty;
        ScrollRequest.Request();
    }

    private void Reply()
    {
        if ((Thread is not { } root) || String.IsNullOrWhiteSpace(ReplyDraft))
        {
            return;
        }

        AddReply(root, new Message(me, Now(), ReplyDraft.Trim()));
        ReplyDraft = string.Empty;
        ThreadScrollRequest.Request();
    }

    private void ToggleReaction(Reaction reaction)
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

    private void MarkAllAsRead()
    {
        foreach (var conversation in Conversations)
        {
            conversation.UnreadCount = 0;
        }
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("Messages:CloseThread", "Close thread", "Messages", "\uE711", CloseThreadCommand),
        new("Messages:MarkAllAsRead", "Mark all as read", "Messages", "\uE8C3", MarkAllAsReadCommand)
    ];
}
