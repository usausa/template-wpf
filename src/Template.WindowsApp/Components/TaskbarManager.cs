namespace Template.WindowsApp.Components;

using System.Windows.Shell;

using Template.WindowsApp.Shell;
using Template.WindowsApp.Views;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class TaskbarManager : NotificationObject, IDisposable
{
    private const string FavoriteCategory = "Favorites";

    private const string RecentCategory = "Recent";

    private const string ViewArgument = "--view ";

    private readonly Subject<ViewId> viewRemoved = new();

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

    public IObservable<ViewId> ViewRemoved => viewRemoved;

    public void Dispose()
    {
        viewRemoved.Dispose();
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

    public void SetViews(IEnumerable<NavigationItem> favorites, IEnumerable<NavigationItem> recentPages)
    {
        var jumpList = new JumpList();
        AddViews(jumpList, favorites, FavoriteCategory);
        AddViews(jumpList, recentPages, RecentCategory);

        var removed = new List<ViewId>();
        void OnJumpItemsRemovedByUser(object? sender, JumpItemsRemovedEventArgs e)
        {
            foreach (var task in e.RemovedItems.OfType<JumpTask>())
            {
                if ((task.Arguments is { } arguments) &&
                    arguments.StartsWith(ViewArgument, StringComparison.Ordinal) &&
                    Enum.TryParse<ViewId>(arguments[ViewArgument.Length..], out var id))
                {
                    removed.Add(id);
                }
            }
        }

        jumpList.JumpItemsRemovedByUser += OnJumpItemsRemovedByUser;
        JumpList.SetJumpList(Application.Current, jumpList);
        jumpList.Apply();
        jumpList.JumpItemsRemovedByUser -= OnJumpItemsRemovedByUser;

        foreach (var id in removed)
        {
            viewRemoved.OnNext(id);
        }
    }

    private static void AddViews(JumpList jumpList, IEnumerable<NavigationItem> items, string category)
    {
        var path = Environment.ProcessPath;
        foreach (var item in items)
        {
            jumpList.JumpItems.Add(new JumpTask
            {
                Title = item.Title,
                Description = $"Open {item.Title}",
                ApplicationPath = path,
                IconResourcePath = path,
                Arguments = ViewArgument + item.Id,
                CustomCategory = category
            });
        }
    }
}
