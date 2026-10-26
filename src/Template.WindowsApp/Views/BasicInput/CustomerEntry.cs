namespace Template.WindowsApp.Views.BasicInput;

public sealed record CustomerEntry(string Name, string Company, string City)
{
    public override string ToString() => Name;
}
