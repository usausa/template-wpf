namespace Template.WindowsApp.Models.Sample;

//--------------------------------------------------------------------------------
// Enum
//--------------------------------------------------------------------------------

public enum ConversationKind
{
    Channel,
    Direct
}

public enum Presence
{
    Active,
    Away
}

public enum AvatarColor
{
    Purple,
    Green,
    Orange,
    Blue
}

public enum ReactionKind
{
    Like,
    Heart,
    Accept,
    Laugh
}

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed class Person
{
    public string Name { get; }

    public string Initials { get; }

    public AvatarColor Color { get; }

    public Presence Presence { get; }

    public Person(string name, AvatarColor color, Presence presence)
    {
        Name = name;
        Initials = String.Concat(name.Split(' ').Select(static x => x[0]));
        Color = color;
        Presence = presence;
    }

    public override string ToString() => Name;
}

public sealed record Attachment(string Name, int Kilobytes);

public sealed partial class Reaction : ObservableObject
{
    public ReactionKind Kind { get; }

    [ObservableProperty]
    public partial int Count { get; set; }

    [ObservableProperty]
    public partial bool IsMine { get; set; }

    public Reaction(ReactionKind kind, int count, bool isMine)
    {
        Kind = kind;
        Count = count;
        IsMine = isMine;
    }

    public override string ToString() => Kind.ToString();
}

public sealed partial class Message : ObservableObject
{
    private static readonly TimeSpan GroupSpan = TimeSpan.FromMinutes(5);

    public Person Author { get; }

    public DateTime Time { get; }

    public string Text { get; }

    public Attachment? Attachment { get; init; }

    public ObservableCollection<Reaction> Reactions { get; } = [];

    public ObservableCollection<Message> Replies { get; } = [];

    public ObservableCollection<Person> Repliers { get; } = [];

    public bool StartsDay { get; set; }

    public bool IsToday { get; set; }

    public bool IsContinuation { get; set; }

    [ObservableProperty]
    public partial bool IsFirstUnread { get; set; }

    [ObservableProperty]
    public partial DateTime? LastReplyTime { get; set; }

    public Message(Person author, DateTime time, string text)
    {
        Author = author;
        Time = time;
        Text = text;
    }

    public void AddReply(Message reply, DateTime today)
    {
        Append(Replies, reply, today);
        if (!Repliers.Contains(reply.Author))
        {
            Repliers.Add(reply.Author);
        }
        LastReplyTime = reply.Time;
    }

    public override string ToString() => Text;

    internal static void Append(ObservableCollection<Message> messages, Message message, DateTime today)
    {
        var previous = messages.Count > 0 ? messages[^1] : null;
        message.StartsDay = previous?.Time.Date != message.Time.Date;
        message.IsToday = message.Time.Date == today;
        message.IsContinuation = (previous is not null) && !message.StartsDay && (previous.Author == message.Author) && ((message.Time - previous.Time) <= GroupSpan);
        messages.Add(message);
    }
}

public sealed partial class Conversation : ObservableObject
{
    public ConversationKind Kind { get; }

    public string Name { get; }

    public string Description { get; }

    public int Members { get; }

    public Person? Person { get; }

    public ObservableCollection<Message> Messages { get; } = [];

    [ObservableProperty]
    public partial int UnreadCount { get; set; }

    [ObservableProperty]
    public partial string Draft { get; set; }

    private Conversation(ConversationKind kind, string name, string description, int members, Person? person)
    {
        Kind = kind;
        Name = name;
        Description = description;
        Members = members;
        Person = person;
        Draft = string.Empty;
    }

    public static Conversation Channel(string name, string description, int members) =>
        new(ConversationKind.Channel, name, description, members, null);

    public static Conversation Direct(Person person) =>
        new(ConversationKind.Direct, person.Name, string.Empty, 2, person);

    public Message Post(Message message, DateTime today)
    {
        Message.Append(Messages, message, today);
        return message;
    }

    public override string ToString() => Name;
}

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

public static class ChatSample
{
    public static Person LoadMe() => new("Dana Kim", AvatarColor.Blue, Presence.Active);

    public static IReadOnlyList<Conversation> LoadConversations(Person me, DateTime today)
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

        Message Post(Conversation conversation, Message message) => conversation.Post(message, today);

        void AddReply(Message root, Message reply) => root.AddReply(reply, today);
    }

    private static DateTime At(DateTime day, int hour, int minute) => day.AddHours(hour).AddMinutes(minute);
}
