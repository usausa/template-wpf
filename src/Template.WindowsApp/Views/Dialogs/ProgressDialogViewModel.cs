namespace Template.WindowsApp.Views.Dialogs;

[ObservableGeneratorOption(Reactive = true, ViewModel = true)]
public sealed partial class ProgressDialogViewModel : DialogViewModelBase
{
    private readonly CancellationTokenSource cancellation = new();

    public string Title { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    [ObservableProperty]
    public partial double Value { get; set; }

    [ObservableProperty]
    public partial bool IsCanceling { get; set; }

    public IProgress<double> Progress { get; }

    public CancellationToken Token => cancellation.Token;

    public IObserveCommand CloseCommand { get; }

    public ProgressDialogViewModel()
    {
        Progress = new Progress<double>(x => Value = x);
        CloseCommand = MakeDelegateCommand(() =>
        {
            IsCanceling = true;
            cancellation.Cancel();
        }, () => !IsCanceling);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            cancellation.Dispose();
        }

        base.Dispose(disposing);
    }
}
