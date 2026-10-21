namespace Template.WindowsApp.Views.Dialogs;

[ObservableGeneratorOption(Reactive = true, ViewModel = true)]
public sealed partial class InputDialogViewModel : ExtendViewModelBase
{
    [ObservableProperty]
    public partial string Title { get; set; }

    [ObservableProperty]
    public partial string Value { get; set; }

    public InputDialogViewModel()
    {
        Title = string.Empty;
        Value = string.Empty;
    }
}
