namespace Template.WindowsApp.Services;

using Template.WindowsApp.Controls;

public sealed class Notification
{
    public InfoBarSeverity Severity { get; }

    public string Title { get; }

    public string Message { get; }

    public Notification(InfoBarSeverity severity, string title, string message)
    {
        Severity = severity;
        Title = title;
        Message = message;
    }
}
