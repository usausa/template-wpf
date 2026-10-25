namespace Template.WindowsApp.Views.Collections;

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

    public override string ToString() => Name;
}
