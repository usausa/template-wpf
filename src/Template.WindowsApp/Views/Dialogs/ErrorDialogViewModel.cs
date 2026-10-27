namespace Template.WindowsApp.Views.Dialogs;

using Template.WindowsApp.Components;

[ObservableGeneratorOption(Reactive = true, ViewModel = true)]
public sealed partial class ErrorDialogViewModel : ExtendViewModelBase
{
    [ObservableProperty]
    public partial string Details { get; set; }

    [ObservableProperty]
    public partial string? LogFolder { get; set; }

    public IObserveCommand CopyCommand { get; }

    public IObserveCommand OpenLogFolderCommand { get; }

    public ErrorDialogViewModel(IClipboard clipboard, IExplorerLauncher explorerLauncher)
    {
        Details = string.Empty;

        CopyCommand = MakeDelegateCommand(() => clipboard.SetText(Details));
        OpenLogFolderCommand = MakeDelegateCommand(() =>
        {
            if (LogFolder is { } folder)
            {
                explorerLauncher.Open(folder);
            }
        }, () => LogFolder is not null);
    }
}
