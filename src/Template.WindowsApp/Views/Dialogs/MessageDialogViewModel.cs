namespace Template.WindowsApp.Views.Dialogs;

public sealed class MessageDialogViewModel : DialogViewModelBase
{
    public string Title { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public string? PrimaryButtonText { get; init; }

    public string CloseButtonText { get; init; } = string.Empty;

    public IObserveCommand PrimaryCommand { get; }

    public IObserveCommand CloseCommand { get; }

    public MessageDialogViewModel()
    {
        PrimaryCommand = MakeDelegateCommand(() => Complete(true));
        CloseCommand = MakeDelegateCommand(() => Complete(false));
    }
}
