namespace Template.WindowsApp.Views.Collections;

public sealed partial class Message : ObservableObject
{
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

    public override string ToString() => Text;
}
