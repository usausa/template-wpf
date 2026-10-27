namespace Template.WindowsApp.Views.Collections;

using Template.WindowsApp.Models.Sample;
using Template.WindowsApp.Services;
using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class MessagesViewModel : AppViewModelBase, IShellCommandProvider
{
    private readonly ConversationService conversationService;

    private Message? firstUnread;

    public IReadOnlyList<Conversation> Conversations => conversationService.Conversations;

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

    public MessagesViewModel(ConversationService conversationService)
    {
        this.conversationService = conversationService;

        ReplyDraft = string.Empty;

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
        ToggleReactionCommand = MakeDelegateCommand<Reaction>(conversationService.Toggle);
        LikeCommand = MakeDelegateCommand<Message>(static x => ConversationService.React(x, ReactionKind.Like));
        HeartCommand = MakeDelegateCommand<Message>(static x => ConversationService.React(x, ReactionKind.Heart));
        AcceptCommand = MakeDelegateCommand<Message>(static x => ConversationService.React(x, ReactionKind.Accept));
        LaughCommand = MakeDelegateCommand<Message>(static x => ConversationService.React(x, ReactionKind.Laugh));
        MarkAllAsReadCommand = MakeDelegateCommand(conversationService.MarkAllAsRead, () => Conversations.Any(static x => x.UnreadCount > 0));

        Selected = Conversations[0];
        Thread = Conversations[0].Messages.First(static x => x.Replies.Count > 0);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ClearNewLine();
        }

        base.Dispose(disposing);
    }

    private void Open(Conversation? conversation)
    {
        ClearNewLine();

        Thread = null;

        if (conversation is { UnreadCount: > 0 })
        {
            firstUnread = conversation.Messages[^conversation.UnreadCount];
            firstUnread.IsFirstUnread = true;
            conversationService.MarkRead(conversation);
        }

        ScrollRequest.Request();
    }

    private void ClearNewLine()
    {
        if (firstUnread is not null)
        {
            firstUnread.IsFirstUnread = false;
            firstUnread = null;
        }
    }

    private void Send()
    {
        if ((Selected is not { } conversation) || String.IsNullOrWhiteSpace(conversation.Draft))
        {
            return;
        }

        conversationService.Send(conversation, conversation.Draft.Trim());
        conversation.Draft = string.Empty;
        ScrollRequest.Request();
    }

    private void Reply()
    {
        if ((Thread is not { } root) || String.IsNullOrWhiteSpace(ReplyDraft))
        {
            return;
        }

        conversationService.Reply(root, ReplyDraft.Trim());
        ReplyDraft = string.Empty;
        ThreadScrollRequest.Request();
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("Messages:CloseThread", "Close thread", "Messages", "\uE711", CloseThreadCommand),
        new("Messages:MarkAllAsRead", "Mark all as read", "Messages", "\uE8C3", MarkAllAsReadCommand)
    ];
}
