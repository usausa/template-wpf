namespace Template.WindowsApp.Views.Fundamentals;

using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class TemplatesViewModel : AppViewModelBase, IShellCommandProvider
{
    public IReadOnlyList<SwatchEntry> Swatches { get; } =
    [
        new("Blue", Color.FromRgb(0x57, 0x94, 0xF2)),
        new("Green", Color.FromRgb(0x73, 0xBF, 0x69)),
        new("Yellow", Color.FromRgb(0xFA, 0xDE, 0x2A)),
        new("Orange", Color.FromRgb(0xFF, 0x98, 0x30)),
        new("Red", Color.FromRgb(0xF2, 0x49, 0x5C)),
        new("Purple", Color.FromRgb(0xB8, 0x77, 0xD9))
    ];

    [ObservableProperty]
    public partial SwatchEntry? SelectedSwatch { get; set; }

    public IReadOnlyList<string> Tags { get; } = ["XAML", "Binding", "Styles", "Templates", "Resources", "Commands", "MVVM"];

    [ObservableProperty]
    public partial string? SelectedTag { get; set; }

    [ObservableProperty]
    public partial int PressCount { get; set; }

    public IReadOnlyList<object> Items { get; } =
    [
        new NoteItem("Bring the slides"),
        new TaskItem("Book the room", true),
        new TaskItem("Send the agenda", false),
        new LinkItem("Team site", "https://example.com/team")
    ];

    public IReadOnlyList<int> Temperatures { get; } = [12, 18, 24, 31, 35, 28];

    public IObserveCommand PressCommand { get; }

    public IObserveCommand ResetCommand { get; }

    public TemplatesViewModel()
    {
        SelectedSwatch = Swatches[0];
        SelectedTag = Tags[1];

        PressCommand = MakeDelegateCommand(() => PressCount++);
        ResetCommand = MakeDelegateCommand(Reset);
    }

    private void Reset()
    {
        SelectedSwatch = Swatches[0];
        SelectedTag = Tags[1];
        PressCount = 0;
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("Templates:Reset", "Reset", "Templates", "\uE72C", ResetCommand)
    ];
}
