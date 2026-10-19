namespace Template.WindowsApp.Shell;

public sealed class ShellCommand
{
    public string Id { get; }

    public string Title { get; }

    public string Category { get; }

    public string Icon { get; }

    public ICommand Command { get; }

    public object? Parameter { get; }

    public ShellCommand(string id, string title, string category, string icon, ICommand command, object? parameter = null)
    {
        Id = id;
        Title = title;
        Category = category;
        Icon = icon;
        Command = command;
        Parameter = parameter;
    }

    public override string ToString() => Title;
}
