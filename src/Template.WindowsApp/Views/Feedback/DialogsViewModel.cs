namespace Template.WindowsApp.Views.Feedback;

using Template.WindowsApp.Components;
using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class DialogsViewModel : AppViewModelBase, IShellCommandProvider
{
    private const string Canceled = "Canceled";

    private const int TaskSteps = 50;

    private static readonly TimeSpan TaskInterval = TimeSpan.FromMilliseconds(100);

    [ObservableProperty]
    public partial string ConfirmResult { get; set; }

    [ObservableProperty]
    public partial string InputResult { get; set; }

    [ObservableProperty]
    public partial string ProgressResult { get; set; }

    [ObservableProperty]
    public partial string OpenFileResult { get; set; }

    [ObservableProperty]
    public partial string SaveFileResult { get; set; }

    [ObservableProperty]
    public partial string SelectFolderResult { get; set; }

    public IObserveCommand ConfirmCommand { get; }

    public IObserveCommand NotifyCommand { get; }

    public IObserveCommand InputCommand { get; }

    public IObserveCommand ProgressCommand { get; }

    public IObserveCommand OpenFileCommand { get; }

    public IObserveCommand SaveFileCommand { get; }

    public IObserveCommand SelectFolderCommand { get; }

    public IObserveCommand UnhandledExceptionCommand { get; }

    public DialogsViewModel(IDialog dialog)
    {
        ConfirmResult = string.Empty;
        InputResult = string.Empty;
        ProgressResult = string.Empty;
        OpenFileResult = string.Empty;
        SaveFileResult = string.Empty;
        SelectFolderResult = string.Empty;

        ConfirmCommand = MakeAsyncCommand(async () => ConfirmResult = await dialog.ConfirmAsync("Are you sure?").ConfigureAwait(true) ? "Yes" : "No");
        NotifyCommand = MakeAsyncCommand(() => dialog.NotifyAsync("This is a notification."));
        InputCommand = MakeAsyncCommand(async () =>
        {
            var value = await dialog.InputAsync("Input value", InputResult).ConfigureAwait(true);
            if (value is not null)
            {
                InputResult = value;
            }
        });
        ProgressCommand = MakeAsyncCommand(async () => ProgressResult = await dialog.ProgressAsync("Working", "Please wait while the task runs.", RunTaskAsync).ConfigureAwait(true) ? "Completed" : Canceled);
        OpenFileCommand = MakeDelegateCommand(() => OpenFileResult = dialog.OpenFile() ?? Canceled);
        SaveFileCommand = MakeDelegateCommand(() => SaveFileResult = dialog.SaveFile("sample.txt") ?? Canceled);
        SelectFolderCommand = MakeDelegateCommand(() => SelectFolderResult = dialog.SelectFolder() ?? Canceled);
        UnhandledExceptionCommand = MakeDelegateCommand(static () => throw new InvalidOperationException("This is a sample exception."));
    }

    private static async Task RunTaskAsync(IProgress<double> progress, CancellationToken token)
    {
        for (var i = 1; i <= TaskSteps; i++)
        {
            await Task.Delay(TaskInterval, token).ConfigureAwait(true);
            progress.Report(i * 100d / TaskSteps);
        }
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("Dialogs:Confirm", "Confirm", "Dialogs", "\uE8FB", ConfirmCommand),
        new("Dialogs:Notify", "Notify", "Dialogs", "\uE8BD", NotifyCommand),
        new("Dialogs:Input", "Input", "Dialogs", "\uE70F", InputCommand),
        new("Dialogs:Progress", "Progress", "Dialogs", "\uF16A", ProgressCommand),
        new("Dialogs:OpenFile", "Open file", "Dialogs", "\uE8E5", OpenFileCommand),
        new("Dialogs:SaveFile", "Save file", "Dialogs", "\uE74E", SaveFileCommand),
        new("Dialogs:SelectFolder", "Select folder", "Dialogs", "\uE8B7", SelectFolderCommand),
        new("Dialogs:UnhandledException", "Unhandled exception", "Dialogs", "\uE783", UnhandledExceptionCommand)
    ];
}
