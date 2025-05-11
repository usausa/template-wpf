namespace Template.WindowsApp.Views;

// ReSharper disable once ClassNeverInstantiated.Global
[ObservableGeneratorOption(Reactive = true, ViewModel = true)]
public sealed class MainWindowViewModel : ExtendViewModelBase
{
    private readonly ILogger<MainWindowViewModel> logger;

    public IWindowManager WindowManager { get; }

    public ICommand ExecuteCommand { get; }

    public MainWindowViewModel(
        ILogger<MainWindowViewModel> logger,
        IWindowManager windowManager)
    {
        this.logger = logger;
        WindowManager = windowManager;

        ExecuteCommand = MakeAsyncCommand(Execute, () => !BusyState.IsBusy);
    }

    private async Task Execute()
    {
        logger.InfoExecuteStart();

        await Task.Delay(3000).ConfigureAwait(true);

        logger.InfoExecuteEnd();
    }
}
