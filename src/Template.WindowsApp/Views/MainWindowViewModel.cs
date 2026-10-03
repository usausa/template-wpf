namespace Template.WindowsApp.Views;

using Smart.Mvvm.ViewModels;

// ReSharper disable once ClassNeverInstantiated.Global
[ObservableGeneratorOption(Reactive = true, ViewModel = true)]
public sealed class MainWindowViewModel : ExtendViewModelBase
{
    private readonly ILogger<MainWindowViewModel> logger;

    private IDisposable? navigatingBusy;

    public IWindowManager WindowManager { get; }

    public INavigator Navigator { get; }

    public ICommand ExecuteCommand { get; }

    public MainWindowViewModel(
        ILogger<MainWindowViewModel> logger,
        IWindowManager windowManager,
        INavigator navigator)
    {
        this.logger = logger;
        WindowManager = windowManager;
        Navigator = navigator;

        ExecuteCommand = MakeAsyncCommand(Execute, () => !BusyState.IsBusy);

        // Busy while navigating
        Disposables.Add(Observable.FromEventPattern<EventArgs>(h => Navigator.ExecutingChanged += h, h => Navigator.ExecutingChanged -= h)
            .Subscribe(_ => UpdateNavigatingBusy()));
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

    private async Task Execute()
    {
        logger.InfoExecuteStart();

        await Task.Delay(3000).ConfigureAwait(true);

        logger.InfoExecuteEnd();
    }
}
