namespace Template.WindowsApp.Views.Fundamentals;

public sealed record SwatchEntry(string Name, Color Color)
{
    public override string ToString() => Name;
}
