namespace Template.WindowsApp.Views.Dialogs;

using Template.WindowsApp.Services;

[ObservableGeneratorOption(Reactive = true, ViewModel = true)]
public sealed partial class ErrorDialogViewModel : ExtendViewModelBase
{
    [ObservableProperty]
    public partial string Details { get; set; }

    [ObservableProperty]
    public partial string? LogFolder { get; set; }

    public IObserveCommand CopyCommand { get; }

    public IObserveCommand OpenLogFolderCommand { get; }

    public ErrorDialogViewModel(IClipboardService clipboardService, IExplorerService explorerService)
    {
        Details = string.Empty;

        CopyCommand = MakeDelegateCommand(() => clipboardService.SetText(Details));
        OpenLogFolderCommand = MakeDelegateCommand(() =>
        {
            if (LogFolder is { } folder)
            {
                explorerService.Open(folder);
            }
        }, () => LogFolder is not null);
    }
}
