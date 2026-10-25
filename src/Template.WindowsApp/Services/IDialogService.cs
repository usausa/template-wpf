namespace Template.WindowsApp.Services;

public interface IDialogService
{
    Task<bool> ConfirmAsync(string message);

    Task<bool> ConfirmAsync(string title, string message, string acceptText, string cancelText);

    Task<SaveChoice> AskSaveChangesAsync(string message, bool canSave);

    Task NotifyAsync(string message);

    Task<string?> InputAsync(string title, string? initial = null);

    Task<bool> ProgressAsync(string title, string message, Func<IProgress<double>, CancellationToken, Task> action);

    string? OpenFile(string? filter = null);

    string? SaveFile(string? fileName = null, string? filter = null);

    string? SelectFolder();
}
