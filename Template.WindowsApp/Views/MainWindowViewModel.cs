namespace Template.WindowsApp.Views;

public sealed class MainWindowViewModel : ViewModelBase
{
    private readonly ILogger<MainWindowViewModel> logger;

    public IWindowManager WindowManager { get; }

    public NotificationValue<bool> Executing { get; } = new();

    public ICommand ExecuteCommand { get; }

    public MainWindowViewModel(
        ILogger<MainWindowViewModel> logger,
        IWindowManager windowManager)
    {
        this.logger = logger;
        WindowManager = windowManager;

        ExecuteCommand = MakeAsyncCommand(Execute, () => !Executing.Value).Observe(Executing);
    }

    private async Task Execute()
    {
        logger.InfoExecuteStart();

        using var executing = new ExecuteScope(Executing);

        await Task.Delay(3000).ConfigureAwait(true);

        logger.InfoExecuteEnd();
    }
}
