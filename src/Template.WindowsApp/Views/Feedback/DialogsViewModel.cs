namespace Template.WindowsApp.Views.Feedback;

using Template.WindowsApp.Services;
using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class DialogsViewModel : AppViewModelBase, IShellCommandProvider
{
    private const string Canceled = "Canceled";

    [ObservableProperty]
    public partial string ConfirmResult { get; set; }

    [ObservableProperty]
    public partial string InputResult { get; set; }

    [ObservableProperty]
    public partial string OpenFileResult { get; set; }

    [ObservableProperty]
    public partial string SaveFileResult { get; set; }

    [ObservableProperty]
    public partial string SelectFolderResult { get; set; }

    public IObserveCommand ConfirmCommand { get; }

    public IObserveCommand NotifyCommand { get; }

    public IObserveCommand InputCommand { get; }

    public IObserveCommand OpenFileCommand { get; }

    public IObserveCommand SaveFileCommand { get; }

    public IObserveCommand SelectFolderCommand { get; }

    public DialogsViewModel(IDialogService dialogService)
    {
        ConfirmResult = string.Empty;
        InputResult = string.Empty;
        OpenFileResult = string.Empty;
        SaveFileResult = string.Empty;
        SelectFolderResult = string.Empty;

        ConfirmCommand = MakeDelegateCommand(() => ConfirmResult = dialogService.Confirm("Are you sure?") ? "Yes" : "No");
        NotifyCommand = MakeDelegateCommand(() => dialogService.Notify("This is a notification."));
        InputCommand = MakeDelegateCommand(() =>
        {
            var value = dialogService.Input("Input value", InputResult);
            if (value is not null)
            {
                InputResult = value;
            }
        });
        OpenFileCommand = MakeDelegateCommand(() => OpenFileResult = dialogService.OpenFile() ?? Canceled);
        SaveFileCommand = MakeDelegateCommand(() => SaveFileResult = dialogService.SaveFile("sample.txt") ?? Canceled);
        SelectFolderCommand = MakeDelegateCommand(() => SelectFolderResult = dialogService.SelectFolder() ?? Canceled);
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("Dialogs:Confirm", "Confirm", "Dialogs", "\uE8FB", ConfirmCommand),
        new("Dialogs:Notify", "Notify", "Dialogs", "\uE8BD", NotifyCommand),
        new("Dialogs:Input", "Input", "Dialogs", "\uE70F", InputCommand),
        new("Dialogs:OpenFile", "Open file", "Dialogs", "\uE8E5", OpenFileCommand),
        new("Dialogs:SaveFile", "Save file", "Dialogs", "\uE74E", SaveFileCommand),
        new("Dialogs:SelectFolder", "Select folder", "Dialogs", "\uE8B7", SelectFolderCommand)
    ];
}
