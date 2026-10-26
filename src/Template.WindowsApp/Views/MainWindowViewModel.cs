namespace Template.WindowsApp.Views;

using Template.WindowsApp.Extender;
using Template.WindowsApp.Services;
using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
[ObservableGeneratorOption(Reactive = true, ViewModel = true)]
public sealed partial class MainWindowViewModel : ExtendViewModelBase
{
    private const int RecentViewCount = 5;

    private readonly NavigationHistory history = new();

    private IDisposable? navigatingBusy;

    private bool recentViewsApplied;

    public IWindowManager WindowManager { get; }

    public INavigator Navigator { get; }

    public NavigationMenu Menu { get; }

    public DialogHost Dialogs { get; }

    public CommandPalette Palette { get; }

    public NotificationService Notifications { get; }

    public TaskbarService Taskbar { get; }

    [ObservableProperty]
    public partial bool IsNavigating { get; set; }

    [ObservableProperty]
    public partial bool IsNotificationsOpen { get; set; }

    public IObserveCommand ForwardCommand { get; }

    public IObserveCommand GoBackCommand { get; }

    public IObserveCommand GoForwardCommand { get; }

    public IObserveCommand ToggleGroupCommand { get; }

    public IObserveCommand ToggleNavigationCommand { get; }

    public IObserveCommand OpenPaletteCommand { get; }

    public IObserveCommand ClosePaletteCommand { get; }

    public IObserveCommand MovePaletteCommand { get; }

    public IObserveCommand ExecutePaletteCommand { get; }

    public IObserveCommand CloseNotificationCommand { get; }

    public IObserveCommand ShowNotificationsCommand { get; }

    public IObserveCommand CloseNotificationsCommand { get; }

    public IObserveCommand ClearNotificationsCommand { get; }

    public MainWindowViewModel(
        IWindowManager windowManager,
        INavigator navigator,
        NavigationMenu menu,
        DialogHost dialogs,
        NotificationService notifications,
        TaskbarService taskbar,
        ConversationService conversations)
    {
        WindowManager = windowManager;
        Navigator = navigator;
        Menu = menu;
        Dialogs = dialogs;
        Notifications = notifications;
        Taskbar = taskbar;
        Palette = new CommandPalette(windowManager.RecentCommands, windowManager.CommandCounts);

        ForwardCommand = MakeAsyncCommand<ViewId>(CommandMode.Simple, x => Equals(Navigator.CurrentViewId, x) ? Task.CompletedTask : Navigator.ForwardAsync(x, new NavigationParameter().WithEffect(EntranceEffect.Name)), _ => !BusyState.IsBusy || IsNavigating);
        GoBackCommand = MakeAsyncCommand(CommandMode.Simple, () => MoveAsync(-1), () => history.CanGoBack && CanMove());
        GoForwardCommand = MakeAsyncCommand(CommandMode.Simple, () => MoveAsync(1), () => history.CanGoForward && CanMove());
        ToggleGroupCommand = MakeDelegateCommand<NavigationGroup>(ToggleGroup);
        ToggleNavigationCommand = MakeDelegateCommand(() => WindowManager.IsPaneOpen = !WindowManager.IsPaneOpen);
        OpenPaletteCommand = MakeDelegateCommand(CommandMode.Simple, OpenPalette);
        ClosePaletteCommand = MakeDelegateCommand(CommandMode.Simple, Palette.Close);
        MovePaletteCommand = MakeDelegateCommand<CommandPaletteMove>(CommandMode.Simple, Palette.Move);
        ExecutePaletteCommand = MakeDelegateCommand<CommandCandidate?>(CommandMode.Simple, ExecutePalette);
        CloseNotificationCommand = MakeDelegateCommand<Notification>(CommandMode.Simple, Notifications.Close);
        ShowNotificationsCommand = MakeDelegateCommand(CommandMode.Simple, () => IsNotificationsOpen = true);
        CloseNotificationsCommand = MakeDelegateCommand(CommandMode.Simple, () => IsNotificationsOpen = false, () => IsNotificationsOpen);
        ClearNotificationsCommand = MakeDelegateCommand(CommandMode.Simple, Notifications.ClearHistory, () => Notifications.History.Count > 0);

        SubscribeIsNotificationsOpen(x =>
        {
            if (x)
            {
                Notifications.MarkAllAsRead();
            }
        });

        // Busy while navigating
        Disposables.Add(Observable.FromEventPattern<EventArgs>(h => Navigator.ExecutingChanged += h, h => Navigator.ExecutingChanged -= h)
            .Subscribe(_ => UpdateNavigatingBusy()));

        Disposables.Add(Observable
            .FromEventPattern<NavigationEventArgs>(h => Navigator.Navigated += h, h => Navigator.Navigated -= h)
            .Subscribe(x => OnNavigated(x.EventArgs.Context.ToId as ViewId?)));

        // Unread messages
        Disposables.Add(conversations.UnreadCount.Subscribe(x => Menu.SetBadge(ViewId.Messages, x)));

        // Jump list
        Disposables.Add(Taskbar.RecentViewRemoved.Subscribe(x => WindowManager.RecentViews.Remove(x.ToString())));
    }

    private void UpdateNavigatingBusy()
    {
        if (Navigator.Executing)
        {
            IsNavigating = true;
            navigatingBusy ??= BusyState.Begin();
        }
        else
        {
            navigatingBusy?.Dispose();
            navigatingBusy = null;
            IsNavigating = false;
        }
    }

    private void OnNavigated(ViewId? id)
    {
        Menu.Select(id);

        if (id is { } value)
        {
            history.Visit(value);
        }

        if (AddRecentView(id) || !recentViewsApplied)
        {
            var items = Menu.EnumerateItems().ToDictionary(static x => x.Id.ToString());
            Taskbar.SetRecentViews(WindowManager.RecentViews.Select(items.GetValueOrDefault).OfType<NavigationItem>().ToList());
            recentViewsApplied = true;
        }
    }

    private bool AddRecentView(ViewId? id)
    {
        if ((id is not { } value) || (value == ViewId.Home))
        {
            return false;
        }

        var name = value.ToString();
        var views = WindowManager.RecentViews;
        if ((views.Count > 0) && (views[0] == name))
        {
            return false;
        }

        views.Remove(name);
        views.Insert(0, name);
        while (views.Count > RecentViewCount)
        {
            views.RemoveAt(views.Count - 1);
        }
        return true;
    }

    private bool CanMove() => !BusyState.IsBusy && (Dialogs.Current is null);

    private async Task MoveAsync(int offset)
    {
        if (history.Begin(offset) is not { } id)
        {
            return;
        }

        try
        {
            await Navigator.ForwardAsync(id, new NavigationParameter().WithEffect(EntranceEffect.Name)).ConfigureAwait(true);
        }
        finally
        {
            history.End();
        }
    }

    private void ToggleGroup(NavigationGroup group)
    {
        if (WindowManager.IsPaneOpen)
        {
            group.IsExpanded = !group.IsExpanded;
        }
        else
        {
            WindowManager.IsPaneOpen = true;
            group.IsExpanded = true;
        }
    }

    private List<ShellCommand> CollectCommands()
    {
        var commands = Menu.EnumerateItems().Select(x => new ShellCommand($"View:{x.Id}", x.Title, "View", x.Icon, ForwardCommand, x.Id)).ToList();
        commands.Add(new ShellCommand("Window:GoBack", "Go back", "Window", "\uE72B", GoBackCommand));
        commands.Add(new ShellCommand("Window:GoForward", "Go forward", "Window", "\uE72A", GoForwardCommand));
        commands.Add(new ShellCommand("Window:ShowNotifications", "Show notifications", "Window", "\uEA8F", ShowNotificationsCommand));
        commands.Add(new ShellCommand("Window:ToggleNavigation", "Toggle navigation", "Window", "\uE700", ToggleNavigationCommand));
        if (Navigator.CurrentTarget is IShellCommandProvider provider)
        {
            commands.AddRange(provider.GetCommands());
        }
        return commands;
    }

    private void OpenPalette()
    {
        if (Dialogs.Current is null)
        {
            Palette.Open(CollectCommands());
        }
    }

    private void ExecutePalette(CommandCandidate? candidate)
    {
        var target = Palette.Pick(candidate);
        if ((target is not null) && target.Command.CanExecute(target.Parameter))
        {
            target.Command.Execute(target.Parameter);
        }
    }
}
