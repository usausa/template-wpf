namespace Template.WindowsApp.Views.Dialogs;

[ObservableGeneratorOption(Reactive = true, ViewModel = true)]
public sealed partial class InputDialogViewModel : AppDialogViewModelBase
{
    public string Title { get; init; } = string.Empty;

    [ObservableProperty]
    public partial string Value { get; set; }

    public IObserveCommand PrimaryCommand { get; }

    public IObserveCommand CloseCommand { get; }

    public InputDialogViewModel()
    {
        Value = string.Empty;

        PrimaryCommand = MakeDelegateCommand(() => Complete(true));
        CloseCommand = MakeDelegateCommand(() => Complete(false));
    }
}
