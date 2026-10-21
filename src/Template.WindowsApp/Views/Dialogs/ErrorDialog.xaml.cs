namespace Template.WindowsApp.Views.Dialogs;

using Template.WindowsApp.Services;

public sealed partial class ErrorDialog
{
    private readonly IClipboardService clipboardService;

    private readonly IExplorerService explorerService;

    public string Details
    {
        get => DetailsText.Text;
        set => DetailsText.Text = value;
    }

    public string? LogFolder
    {
        get;
        set
        {
            field = value;
            OpenLogFolderButton.IsEnabled = value is not null;
        }
    }

    public ErrorDialog(IClipboardService clipboardService, IExplorerService explorerService)
    {
        this.clipboardService = clipboardService;
        this.explorerService = explorerService;

        InitializeComponent();
    }

    private void OnCopyClick(object sender, RoutedEventArgs e)
    {
        clipboardService.SetText(Details);
    }

    private void OnOpenLogFolderClick(object sender, RoutedEventArgs e)
    {
        if (LogFolder is not null)
        {
            explorerService.Open(LogFolder);
        }
    }
}
