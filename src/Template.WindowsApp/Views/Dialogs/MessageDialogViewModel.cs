namespace Template.WindowsApp.Views.Dialogs;

public sealed class MessageDialogViewModel : AppDialogViewModelBase
{
    public string Title { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public string? PrimaryButtonText { get; init; }

    public string? SecondaryButtonText { get; init; }

    public string CloseButtonText { get; init; } = string.Empty;

    public bool IsPrimaryEnabled { get; init; } = true;

    public bool IsSecondary { get; private set; }

    public IObserveCommand PrimaryCommand { get; }

    public IObserveCommand SecondaryCommand { get; }

    public IObserveCommand CloseCommand { get; }

    public MessageDialogViewModel()
    {
        PrimaryCommand = MakeDelegateCommand(() => Complete(true), () => IsPrimaryEnabled);
        SecondaryCommand = MakeDelegateCommand(() =>
        {
            IsSecondary = true;
            Complete(true);
        });
        CloseCommand = MakeDelegateCommand(() => Complete(false));
    }
}
