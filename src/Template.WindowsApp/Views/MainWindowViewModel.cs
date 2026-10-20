namespace Template.WindowsApp.Views;

using Template.WindowsApp.Services;
using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
[ObservableGeneratorOption(Reactive = true, ViewModel = true)]
public sealed partial class MainWindowViewModel : ExtendViewModelBase
{
    private readonly ILogger<MainWindowViewModel> logger;

    private IDisposable? navigatingBusy;

    public IWindowManager WindowManager { get; }

    public INavigator Navigator { get; }

    public NavigationMenu Menu { get; }

    public CommandPalette Palette { get; }

    public NotificationService Notifications { get; }

    [ObservableProperty]
    public partial bool IsNavigating { get; set; }

    public IObserveCommand ForwardCommand { get; }

    public IObserveCommand ToggleGroupCommand { get; }

    public IObserveCommand ExecuteCommand { get; }

    public IObserveCommand ToggleNavigationCommand { get; }

    public IObserveCommand OpenPaletteCommand { get; }

    public IObserveCommand ClosePaletteCommand { get; }

    public IObserveCommand MovePaletteCommand { get; }

    public IObserveCommand ExecutePaletteCommand { get; }

    public IObserveCommand CloseNotificationCommand { get; }

    public MainWindowViewModel(
        ILogger<MainWindowViewModel> logger,
        IWindowManager windowManager,
        INavigator navigator,
        NavigationMenu menu,
        NotificationService notifications)
    {
        this.logger = logger;
        WindowManager = windowManager;
        Navigator = navigator;
        Menu = menu;
        Notifications = notifications;
        Palette = new CommandPalette(windowManager.RecentCommands);

        ForwardCommand = MakeAsyncCommand<ViewId>(x => Equals(Navigator.CurrentViewId, x) ? Task.CompletedTask : Navigator.ForwardAsync(x, new NavigationParameter().WithPushEffect()));
        ToggleGroupCommand = MakeDelegateCommand<NavigationGroup>(ToggleGroup);
        ExecuteCommand = MakeAsyncCommand(Execute, () => !BusyState.IsBusy);
        ToggleNavigationCommand = MakeDelegateCommand(() => WindowManager.IsPaneOpen = !WindowManager.IsPaneOpen);
        OpenPaletteCommand = MakeDelegateCommand(CommandMode.Simple, () => Palette.Open(CollectCommands()));
        ClosePaletteCommand = MakeDelegateCommand(CommandMode.Simple, Palette.Close);
        MovePaletteCommand = MakeDelegateCommand<CommandPaletteMove>(CommandMode.Simple, Palette.Move);
        ExecutePaletteCommand = MakeDelegateCommand<ShellCommand?>(CommandMode.Simple, ExecutePalette);
        CloseNotificationCommand = MakeDelegateCommand<Notification>(CommandMode.Simple, Notifications.Close);

        // Busy while navigating
        Disposables.Add(Observable.FromEventPattern<EventArgs>(h => Navigator.ExecutingChanged += h, h => Navigator.ExecutingChanged -= h)
            .Subscribe(_ => UpdateNavigatingBusy()));

        Disposables.Add(Observable
            .FromEventPattern<NavigationEventArgs>(h => Navigator.Navigated += h, h => Navigator.Navigated -= h)
            .Subscribe(x => Menu.Select(x.EventArgs.Context.ToId as ViewId?)));
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
        commands.Add(new ShellCommand("Window:Execute", "Execute", "Window", "\uE768", ExecuteCommand));
        commands.Add(new ShellCommand("Window:ToggleNavigation", "Toggle navigation", "Window", "\uE700", ToggleNavigationCommand));
        if (Navigator.CurrentTarget is IShellCommandProvider provider)
        {
            commands.AddRange(provider.GetCommands());
        }
        return commands;
    }

    private void ExecutePalette(ShellCommand? command)
    {
        var target = Palette.Pick(command);
        if ((target is not null) && target.Command.CanExecute(target.Parameter))
        {
            target.Command.Execute(target.Parameter);
        }
    }

    private async Task Execute()
    {
        logger.InfoExecuteStart();

        await Task.Delay(3000).ConfigureAwait(true);

        logger.InfoExecuteEnd();
    }
}
