namespace Template.WindowsApp.Views;

using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
[ObservableGeneratorOption(Reactive = true, ViewModel = true)]
public sealed class MainWindowViewModel : ExtendViewModelBase
{
    private readonly ILogger<MainWindowViewModel> logger;

    private IDisposable? navigatingBusy;

    public IWindowManager WindowManager { get; }

    public INavigator Navigator { get; }

    public IReadOnlyList<NavigationItem> Items { get; } =
    [
        new(ViewId.Home, "Home", ""),
        new(ViewId.Sub, "Sub", "")
    ];

    public IObserveCommand ForwardCommand { get; }

    public IObserveCommand ExecuteCommand { get; }

    public MainWindowViewModel(
        ILogger<MainWindowViewModel> logger,
        IWindowManager windowManager,
        INavigator navigator)
    {
        this.logger = logger;
        WindowManager = windowManager;
        Navigator = navigator;

        ForwardCommand = MakeAsyncCommand<ViewId>(x => Equals(Navigator.CurrentViewId, x) ? Task.CompletedTask : Navigator.ForwardAsync(x));
        ExecuteCommand = MakeAsyncCommand(Execute, () => !BusyState.IsBusy);

        // Busy while navigating
        Disposables.Add(Observable.FromEventPattern<EventArgs>(h => Navigator.ExecutingChanged += h, h => Navigator.ExecutingChanged -= h)
            .Subscribe(_ => UpdateNavigatingBusy()));

        Disposables.Add(Observable
            .FromEventPattern<NavigationEventArgs>(h => Navigator.Navigated += h, h => Navigator.Navigated -= h)
            .Subscribe(x => UpdateSelection(x.EventArgs.Context.ToId as ViewId?)));
    }

    private void UpdateNavigatingBusy()
    {
        if (Navigator.Executing)
        {
            navigatingBusy ??= BusyState.Begin();
        }
        else
        {
            navigatingBusy?.Dispose();
            navigatingBusy = null;
        }
    }

    private void UpdateSelection(ViewId? id)
    {
        foreach (var item in Items)
        {
            item.IsSelected = item.Id == id;
        }
    }

    private async Task Execute()
    {
        logger.InfoExecuteStart();

        await Task.Delay(3000).ConfigureAwait(true);

        logger.InfoExecuteEnd();
    }
}
