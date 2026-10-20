namespace Template.WindowsApp.Views.Feedback;

using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class ProgressViewModel : AppViewModelBase, IShellCommandProvider
{
    private const double InitialValue = 40;

    private const int TaskSteps = 50;

    private static readonly TimeSpan TaskInterval = TimeSpan.FromMilliseconds(100);

    private static readonly TimeSpan BusyDuration = TimeSpan.FromSeconds(3);

    private Action? cancel;

    [ObservableProperty]
    public partial double Value { get; set; }

    [ObservableProperty]
    public partial bool IsRunning { get; set; }

    [ObservableProperty]
    public partial int TaskProgress { get; set; }

    [ObservableProperty]
    public partial TaskState TaskState { get; set; }

    public IObserveCommand BusyCommand { get; }

    public IObserveCommand StartCommand { get; }

    public IObserveCommand CancelCommand { get; }

    public ProgressViewModel()
    {
        Value = InitialValue;

        BusyCommand = MakeAsyncCommand(static () => Task.Delay(BusyDuration));
        StartCommand = MakeAsyncCommand(CommandMode.Simple, RunTaskAsync, () => TaskState != TaskState.Running);
        CancelCommand = MakeDelegateCommand(CommandMode.Simple, () => cancel?.Invoke(), () => TaskState == TaskState.Running);

        Disposables.Add(new DelegateDisposable(() => cancel?.Invoke()));
    }

    private async Task RunTaskAsync()
    {
        using var source = new CancellationTokenSource();
        cancel = source.Cancel;
        TaskProgress = 0;
        TaskState = TaskState.Running;
        try
        {
            for (var i = 1; i <= TaskSteps; i++)
            {
                await Task.Delay(TaskInterval, source.Token).ConfigureAwait(true);
                TaskProgress = i * 100 / TaskSteps;
            }

            TaskState = TaskState.Completed;
        }
        catch (OperationCanceledException)
        {
            TaskState = TaskState.Canceled;
        }
        finally
        {
            cancel = null;
        }
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("Progress:Start", "Start", "Progress", "\uE768", StartCommand),
        new("Progress:Cancel", "Cancel", "Progress", "\uE711", CancelCommand)
    ];
}
