namespace Template.WindowsApp.Services;

using System.Windows.Threading;

using Template.WindowsApp.Controls;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class NotificationService : NotificationObject
{
    private const int MaxCount = 3;

    private const int HistoryCount = 50;

    private static readonly TimeSpan Duration = TimeSpan.FromSeconds(5);

    private readonly TimeProvider timeProvider;

    public ObservableCollection<Notification> Items { get; } = [];

    public ObservableCollection<Notification> History { get; } = [];

    public int UnreadCount
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public NotificationService(TimeProvider timeProvider)
    {
        this.timeProvider = timeProvider;
    }

    public void Show(InfoBarSeverity severity, string title, string message)
    {
        var notification = new Notification(severity, title, message, timeProvider.GetLocalNow().DateTime);
        Items.Add(notification);
        while (Items.Count > MaxCount)
        {
            Items.RemoveAt(0);
        }

        History.Insert(0, notification);
        while (History.Count > HistoryCount)
        {
            History.RemoveAt(History.Count - 1);
        }
        UnreadCount = Math.Min(UnreadCount + 1, History.Count);

        var timer = new DispatcherTimer { Interval = Duration };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            Items.Remove(notification);
        };
        timer.Start();
    }

    public void Close(Notification notification) => Items.Remove(notification);

    public void MarkAllAsRead() => UnreadCount = 0;

    public void ClearHistory()
    {
        History.Clear();
        UnreadCount = 0;
    }
}
