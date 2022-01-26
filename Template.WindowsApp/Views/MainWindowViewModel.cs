namespace Template.WindowsApp.Views;

using System.Windows.Input;

using Microsoft.Extensions.Logging;

using Smart.ComponentModel;
using Smart.Windows.ViewModels;

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
        logger.LogInformation("Execute start.");

        Executing.Value = true;

        try
        {
            await Task.Delay(3000);
        }
        finally
        {
            Executing.Value = false;

            logger.LogInformation("Execute end.");
        }
    }
}
