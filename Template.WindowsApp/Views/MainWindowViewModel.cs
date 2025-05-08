namespace Template.WindowsApp.Views;

// ReSharper disable once ClassNeverInstantiated.Global
[ObservableGeneratorOption(Reactive = true, ViewModel = true)]
public sealed partial class MainWindowViewModel : ExtendViewModelBase
{
    private readonly ILogger<MainWindowViewModel> logger;

    public IWindowManager WindowManager { get; }

    [ObservableProperty]
    public partial bool Executing { get; set; }

    public ICommand ExecuteCommand { get; }

    public MainWindowViewModel(
        ILogger<MainWindowViewModel> logger,
        IWindowManager windowManager)
    {
        this.logger = logger;
        WindowManager = windowManager;

        ExecuteCommand = MakeAsyncCommand(Execute, () => !Executing).Observe(this, nameof(Executing));
    }

    private async Task Execute()
    {
        logger.InfoExecuteStart();

        Executing = true;
        try
        {
            await Task.Delay(3000).ConfigureAwait(true);
        }
        finally
        {
            Executing = false;
        }

        logger.InfoExecuteEnd();
    }
}
