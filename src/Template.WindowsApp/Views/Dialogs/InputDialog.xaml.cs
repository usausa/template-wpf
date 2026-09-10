namespace Template.WindowsApp.Views.Dialogs;

public sealed partial class InputDialog
{
    public string Value
    {
        get => ValueText.Text;
        set => ValueText.Text = value;
    }

    public InputDialog()
    {
        InitializeComponent();

        Loaded += (_, _) =>
        {
            ValueText.Focus();
            ValueText.SelectAll();
        };
    }

    private void OnOkClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}
