namespace Template.WindowsApp.Views.Fundamentals;

public sealed record PersonEntry(string Name, string Role)
{
    public override string ToString() => Name;
}
