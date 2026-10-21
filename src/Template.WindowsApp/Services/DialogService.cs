namespace Template.WindowsApp.Services;

using Microsoft.Win32;

using Template.WindowsApp.Views.Dialogs;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class DialogService : IDialogService
{
    private const string AllFilesFilter = "All files (*.*)|*.*";

    private static Window? Owner => Application.Current.MainWindow;

    public bool Confirm(string message) =>
        Show(message, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

    public string? Input(string title, string? initial = null)
    {
        var viewModel = new InputDialogViewModel
        {
            Title = title,
            Value = initial ?? string.Empty
        };
        var dialog = new InputDialog
        {
            DataContext = viewModel
        };
        if (Owner is { } owner)
        {
            dialog.Owner = owner;
        }

        return dialog.ShowDialog() == true ? viewModel.Value : null;
    }

    public void Notify(string message) =>
        Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);

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

    private static MessageBoxResult Show(string message, string caption, MessageBoxButton button, MessageBoxImage icon) =>
        Owner is { } owner ? MessageBox.Show(owner, message, caption, button, icon) : MessageBox.Show(message, caption, button, icon);

    private static bool ShowDialog(CommonDialog dialog) =>
        (Owner is { } owner ? dialog.ShowDialog(owner) : dialog.ShowDialog()) == true;
}
