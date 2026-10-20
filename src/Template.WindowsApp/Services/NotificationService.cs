namespace Template.WindowsApp.Services;

using System.Windows.Threading;

using Template.WindowsApp.Controls;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class NotificationService
{
    private const int MaxCount = 3;

    private static readonly TimeSpan Duration = TimeSpan.FromSeconds(5);

    public ObservableCollection<Notification> Items { get; } = [];

    public void Show(InfoBarSeverity severity, string title, string message)
    {
        var notification = new Notification(severity, title, message);
        Items.Add(notification);
        while (Items.Count > MaxCount)
        {
            Items.RemoveAt(0);
        }

        var timer = new DispatcherTimer { Interval = Duration };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            Items.Remove(notification);
        };
        timer.Start();
    }

    public void Close(Notification notification) => Items.Remove(notification);
}
