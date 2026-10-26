namespace Template.WindowsApp.Services;

using Template.WindowsApp.Controls;

public sealed class Notification
{
    public InfoBarSeverity Severity { get; }

    public string Title { get; }

    public string Message { get; }

    public DateTime Time { get; }

    public Notification(InfoBarSeverity severity, string title, string message, DateTime time)
    {
        Severity = severity;
        Title = title;
        Message = message;
        Time = time;
    }
}
