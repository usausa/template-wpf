namespace Template.WindowsApp.Services;

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
