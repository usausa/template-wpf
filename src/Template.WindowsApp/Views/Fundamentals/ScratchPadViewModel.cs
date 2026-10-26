namespace Template.WindowsApp.Views.Fundamentals;

using Template.WindowsApp.Services;
using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class ScratchPadViewModel : AppViewModelBase, IShellCommandProvider
{
    private const string DefaultXaml = """
        <StackPanel>
            <TextBlock Text="Hello, XAML" FontSize="20" FontWeight="SemiBold" />
            <TextBlock Margin="0,4,0,12" Text="Edit the markup below and press Load." />
            <StackPanel Orientation="Horizontal">
                <Button Content="Button" />
                <CheckBox Margin="12,0,0,0" Content="Check box" />
            </StackPanel>
        </StackPanel>
        """;

    private readonly IDialogService dialogService;

    [ObservableProperty]
    public partial string Text { get; set; }

    [ObservableProperty]
    public partial string LoadedXaml { get; set; }

    public IObserveCommand LoadCommand { get; }

    public IObserveCommand ResetCommand { get; }

    public ScratchPadViewModel(IDialogService dialogService)
    {
        this.dialogService = dialogService;

        Text = DefaultXaml;
        LoadedXaml = DefaultXaml;

        LoadCommand = MakeDelegateCommand(() => LoadedXaml = Text);
        ResetCommand = MakeAsyncCommand(ResetAsync);
    }

    private async Task ResetAsync()
    {
        if (await dialogService.ConfirmAsync("Reset the markup?", "The markup in the editor is replaced with the first example.", "Reset", "Cancel").ConfigureAwait(true))
        {
            Text = DefaultXaml;
            LoadedXaml = DefaultXaml;
        }
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("ScratchPad:Load", "Load", "Scratch pad", "\uE768", LoadCommand),
        new("ScratchPad:Reset", "Reset", "Scratch pad", "\uE72C", ResetCommand)
    ];
}
