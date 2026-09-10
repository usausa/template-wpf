namespace Template.WindowsApp.Services;

using Template.WindowsApp.Views.Dialogs;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class DialogService : IDialogService
{
    private static Window? Owner => Application.Current.MainWindow;

    public bool Confirm(string message) =>
        Show(message, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

    public string? Input(string title, string? initial = null)
    {
        var dialog = new InputDialog
        {
            Title = title,
            Value = initial ?? string.Empty
        };
        if (Owner is { } owner)
        {
            dialog.Owner = owner;
        }

        return dialog.ShowDialog() == true ? dialog.Value : null;
    }

    public void Notify(string message) =>
        Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);

    private static MessageBoxResult Show(string message, string caption, MessageBoxButton button, MessageBoxImage icon) =>
        Owner is { } owner ? MessageBox.Show(owner, message, caption, button, icon) : MessageBox.Show(message, caption, button, icon);
}
