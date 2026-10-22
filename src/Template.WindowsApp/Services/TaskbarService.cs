namespace Template.WindowsApp.Services;

using System.Windows.Shell;

using Template.WindowsApp.Shell;
using Template.WindowsApp.Views;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class TaskbarService : NotificationObject, IDisposable
{
    private const string RecentCategory = "Recent";

    private const string ViewArgument = "--view ";

    private readonly Subject<ViewId> recentViewRemoved = new();

    public TaskbarItemProgressState State
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public double Value
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public IObservable<ViewId> RecentViewRemoved => recentViewRemoved;

    public void Dispose()
    {
        recentViewRemoved.Dispose();
    }

    public void SetProgress(double value)
    {
        Value = value;
        State = TaskbarItemProgressState.Normal;
    }

    public void Clear()
    {
        State = TaskbarItemProgressState.None;
        Value = 0;
    }

    public void SetRecentViews(IEnumerable<NavigationItem> items)
    {
        var path = Environment.ProcessPath;
        var jumpList = new JumpList();
        foreach (var item in items)
        {
            jumpList.JumpItems.Add(new JumpTask
            {
                Title = item.Title,
                Description = $"Open {item.Title}",
                ApplicationPath = path,
                IconResourcePath = path,
                Arguments = ViewArgument + item.Id,
                CustomCategory = RecentCategory
            });
        }

        jumpList.JumpItemsRemovedByUser += OnJumpItemsRemovedByUser;
        JumpList.SetJumpList(Application.Current, jumpList);
        jumpList.Apply();
        jumpList.JumpItemsRemovedByUser -= OnJumpItemsRemovedByUser;
    }

    private void OnJumpItemsRemovedByUser(object? sender, JumpItemsRemovedEventArgs e)
    {
        foreach (var task in e.RemovedItems.OfType<JumpTask>())
        {
            if ((task.Arguments is { } arguments) &&
                arguments.StartsWith(ViewArgument, StringComparison.Ordinal) &&
                Enum.TryParse<ViewId>(arguments[ViewArgument.Length..], out var id))
            {
                recentViewRemoved.OnNext(id);
            }
        }
    }
}
