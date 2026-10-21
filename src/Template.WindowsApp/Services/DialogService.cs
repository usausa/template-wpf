namespace Template.WindowsApp.Services;

using Microsoft.Win32;

using Template.WindowsApp.Shell;
using Template.WindowsApp.Views.Dialogs;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class DialogService : IDialogService
{
    private const string AllFilesFilter = "All files (*.*)|*.*";

    private readonly DialogHost host;

    private static Window? Owner => Application.Current.MainWindow;

    public DialogService(DialogHost host)
    {
        this.host = host;
    }

    public async Task<bool> ConfirmAsync(string message)
    {
        using var dialog = new MessageDialogViewModel
        {
            Title = "Confirm",
            Message = message,
            PrimaryButtonText = "Yes",
            CloseButtonText = "No"
        };
        return await host.ShowAsync(dialog, static x => x.Completion).ConfigureAwait(true);
    }

    public async Task NotifyAsync(string message)
    {
        using var dialog = new MessageDialogViewModel
        {
            Title = "Information",
            Message = message,
            CloseButtonText = "OK"
        };
        await host.ShowAsync(dialog, static x => x.Completion).ConfigureAwait(true);
    }

    public async Task<string?> InputAsync(string title, string? initial = null)
    {
        using var dialog = new InputDialogViewModel
        {
            Title = title
        };
        dialog.Value = initial ?? string.Empty;
        return await host.ShowAsync(dialog, static async x => await x.Completion.ConfigureAwait(true) ? x.Value : null).ConfigureAwait(true);
    }

    public async Task<bool> ProgressAsync(string title, string message, Func<IProgress<double>, CancellationToken, Task> action)
    {
        using var dialog = new ProgressDialogViewModel
        {
            Title = title,
            Message = message
        };
        return await host.ShowAsync(dialog, async x =>
        {
            try
            {
                await action(x.Progress, x.Token).ConfigureAwait(true);
                return true;
            }
            catch (OperationCanceledException) when (x.Token.IsCancellationRequested)
            {
                return false;
            }
        }).ConfigureAwait(true);
    }

    public string? OpenFile(string? filter = null)
    {
        var dialog = new OpenFileDialog
        {
            Filter = filter ?? AllFilesFilter
        };
        return ShowDialog(dialog) ? dialog.FileName : null;
    }

    public string? SaveFile(string? fileName = null, string? filter = null)
    {
        var dialog = new SaveFileDialog
        {
            FileName = fileName ?? string.Empty,
            Filter = filter ?? AllFilesFilter
        };
        return ShowDialog(dialog) ? dialog.FileName : null;
    }

    public string? SelectFolder()
    {
        var dialog = new OpenFolderDialog();
        return ShowDialog(dialog) ? dialog.FolderName : null;
    }

    private static bool ShowDialog(CommonDialog dialog) =>
        (Owner is { } owner ? dialog.ShowDialog(owner) : dialog.ShowDialog()) == true;
}
