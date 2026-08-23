namespace Template.WindowsApp.Services;

public interface IDialogService
{
    bool Confirm(string message);

    string? Input(string title, string? initial = null);

    void Notify(string message);
}
