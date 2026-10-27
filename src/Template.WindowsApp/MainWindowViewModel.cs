namespace Template.WindowsApp;

using Template.WindowsApp.Components;
using Template.WindowsApp.Extender;
using Template.WindowsApp.Services;
using Template.WindowsApp.Shell;
using Template.WindowsApp.Views;

// ReSharper disable once ClassNeverInstantiated.Global
[ObservableGeneratorOption(Reactive = true, ViewModel = true)]
public sealed partial class MainWindowViewModel : ExtendViewModelBase
{
    private readonly NavigationHistory history = new();

    private IDisposable? navigatingBusy;

    public IWindowManager WindowManager { get; }

    public INavigator Navigator { get; }

    public NavigationMenu Menu { get; }

    public DialogHost Dialogs { get; }

    public CommandPalette Palette { get; }

    public NotificationCenter Notifications { get; }

    public TaskbarManager Taskbar { get; }

    [ObservableProperty]
    public partial bool IsNavigating { get; set; }

    [ObservableProperty]
    public partial bool IsNotificationsOpen { get; set; }

    [ObservableProperty]
    public partial bool IsFavorite { get; set; }

    [ObservableProperty]
    public partial bool CanFavorite { get; set; }

    public IObserveCommand ForwardCommand { get; }

    public IObserveCommand GoBackCommand { get; }

    public IObserveCommand GoForwardCommand { get; }

    public IObserveCommand ToggleFavoriteCommand { get; }

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
        NotificationCenter notificationCenter,
        TaskbarManager taskbarManager,
        ConversationService conversations)
    {
        WindowManager = windowManager;
        Navigator = navigator;
        Menu = menu;
        Dialogs = dialogs;
        Notifications = notificationCenter;
        Taskbar = taskbarManager;
        Palette = new CommandPalette(windowManager.RecentCommands, windowManager.CommandCounts);

        ForwardCommand = MakeAsyncCommand<ViewId>(CommandMode.Simple, x => Equals(Navigator.CurrentViewId, x) ? Task.CompletedTask : Navigator.ForwardAsync(x, new NavigationParameter().WithEffect(EntranceEffect.Name)), _ => !BusyState.IsBusy || IsNavigating);
        GoBackCommand = MakeAsyncCommand(CommandMode.Simple, () => MoveAsync(-1), () => history.CanGoBack && CanMove());
        GoForwardCommand = MakeAsyncCommand(CommandMode.Simple, () => MoveAsync(1), () => history.CanGoForward && CanMove());
        ToggleFavoriteCommand = MakeDelegateCommand(CommandMode.Simple, () => IsFavorite = !IsFavorite, () => CanFavorite);
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
        SubscribeIsFavorite(x =>
        {
            if (Navigator.CurrentViewId is not ViewId id)
            {
                return;
            }

            if (x)
            {
                Menu.AddFavorite(id);
            }
            else
            {
                Menu.RemoveFavorite(id);
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

        // Favorites and recent pages
        Disposables.Add(Observable
            .FromEventPattern(h => Menu.Changed += h, h => Menu.Changed -= h)
            .Subscribe(_ => OnMenuChanged()));

        // Jump list
        Disposables.Add(Taskbar.ViewRemoved.Subscribe(x =>
        {
            Menu.RemoveFavorite(x);
            Menu.RemoveRecentPage(x);
        }));
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
            WindowManager.LastView = value;
            Menu.AddRecentPage(value);
        }

        UpdateFavorite();
    }

    private void OnMenuChanged()
    {
        UpdateFavorite();
        Taskbar.SetViews(Menu.Favorites.Items, Menu.RecentPages);
    }

    private void UpdateFavorite()
    {
        CanFavorite = Navigator.CurrentViewId is ViewId and not ViewId.Home;
        IsFavorite = (Navigator.CurrentViewId is ViewId id) && Menu.IsFavorite(id);
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
        commands.Add(IsFavorite
            ? new ShellCommand("Window:RemoveFavorite", "Remove from favorites", "Window", "\uE8D9", ToggleFavoriteCommand)
            : new ShellCommand("Window:AddFavorite", "Add to favorites", "Window", "\uE734", ToggleFavoriteCommand));
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
