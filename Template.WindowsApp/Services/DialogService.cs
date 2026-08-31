namespace Template.WindowsApp.Services;

using Template.WindowsApp.Views.Dialogs;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class DialogService : IDialogService
{
    public bool Confirm(string message) =>
        MessageBox.Show(Application.Current.MainWindow, message, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

    public string? Input(string title, string? initial = null)
    {
        var dialog = new InputDialog
        {
            Owner = Application.Current.MainWindow,
            Title = title,
            Value = initial ?? string.Empty
        };
        return dialog.ShowDialog() == true ? dialog.Value : null;
    }

    public void Notify(string message) =>
        MessageBox.Show(Application.Current.MainWindow, message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
}
