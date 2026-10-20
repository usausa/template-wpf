namespace Template.WindowsApp.Views.BasicInput;

using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class TextViewModel : AppViewModelBase, IShellCommandProvider
{
    [ObservableProperty]
    public partial string SingleLineText { get; set; }

    [ObservableProperty]
    public partial string MultiLineText { get; set; }

    [ObservableProperty]
    public partial int LineCount { get; set; }

    public EventRequest ClearPasswordRequest { get; } = new();

    public IObserveCommand ResetCommand { get; }

    public TextViewModel()
    {
        SingleLineText = string.Empty;
        MultiLineText = string.Empty;
        LineCount = CountLines(MultiLineText);

        SubscribeMultiLineText(x => LineCount = CountLines(x));

        ResetCommand = MakeDelegateCommand(Reset);
    }

    private static int CountLines(string text) => text.AsSpan().Count('\n') + 1;

    private void Reset()
    {
        SingleLineText = string.Empty;
        MultiLineText = string.Empty;
        ClearPasswordRequest.Request();
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("Text:Reset", "Reset", "Text", "\uE72C", ResetCommand)
    ];
}
