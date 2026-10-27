namespace Template.WindowsApp.Components;

using System.Windows.Threading;

using Template.WindowsApp.Models;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class NotificationCenter : NotificationObject
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

    public bool ShowsPopups
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool ShowsInformational
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool ShowsSuccess
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool ShowsWarning
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool ShowsError
    {
        get;
        set => SetProperty(ref field, value);
    }

    public NotificationCenter(TimeProvider timeProvider)
    {
        this.timeProvider = timeProvider;

        ShowsPopups = true;
        ShowsInformational = true;
        ShowsSuccess = true;
        ShowsWarning = true;
        ShowsError = true;
    }

    public void Show(InfoBarSeverity severity, string title, string message)
    {
        var notification = new Notification(severity, title, message, timeProvider.GetLocalNow().DateTime);
        History.Insert(0, notification);
        while (History.Count > HistoryCount)
        {
            History.RemoveAt(History.Count - 1);
        }
        UnreadCount = Math.Min(UnreadCount + 1, History.Count);

        if (!ShowsPopups || !Shows(severity))
        {
            return;
        }

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

    private bool Shows(InfoBarSeverity severity) =>
        severity switch
        {
            InfoBarSeverity.Success => ShowsSuccess,
            InfoBarSeverity.Warning => ShowsWarning,
            InfoBarSeverity.Error => ShowsError,
            _ => ShowsInformational
        };

    public void MarkAllAsRead() => UnreadCount = 0;

    public void ClearHistory()
    {
        History.Clear();
        UnreadCount = 0;
    }
}
