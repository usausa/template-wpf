namespace Template.WindowsApp.Views.Fundamentals;

using Template.WindowsApp.Shell;

public sealed record PersonEntry(string Name, string Role)
{
    public override string ToString() => Name;
}

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class BindingViewModel : AppViewModelBase, IShellCommandProvider
{
    private const string DefaultName = "Aiko";

    private const string SampleNickname = "Ai";

    private const string DefaultFirstName = "Aiko";

    private const string DefaultLastName = "Tanaka";

    private readonly DateTime today;

    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial DateTime? Date { get; set; }

    [ObservableProperty]
    public partial bool ShowsWeekday { get; set; }

    [ObservableProperty]
    public partial string? Nickname { get; set; }

    [ObservableProperty]
    public partial string FirstName { get; set; }

    [ObservableProperty]
    public partial string LastName { get; set; }

    public IReadOnlyList<PersonEntry> People { get; } =
    [
        new("Aiko Tanaka", "Designer"),
        new("Ben Carter", "Developer"),
        new("Chen Wei", "Tester"),
        new("Dana Kim", "Manager")
    ];

    [ObservableProperty]
    public partial PersonEntry? SelectedPerson { get; set; }

    public IObserveCommand SetNicknameCommand { get; }

    public IObserveCommand ClearNicknameCommand { get; }

    public IObserveCommand ResetCommand { get; }

    public BindingViewModel(TimeProvider timeProvider)
    {
        today = timeProvider.GetLocalNow().Date;

        Name = DefaultName;
        Date = today;
        FirstName = DefaultFirstName;
        LastName = DefaultLastName;
        SelectedPerson = People[0];

        SetNicknameCommand = MakeDelegateCommand(() => Nickname = SampleNickname);
        ClearNicknameCommand = MakeDelegateCommand(() => Nickname = null);
        ResetCommand = MakeDelegateCommand(Reset);
    }

    private void Reset()
    {
        Name = DefaultName;
        Date = today;
        ShowsWeekday = false;
        Nickname = null;
        FirstName = DefaultFirstName;
        LastName = DefaultLastName;
        SelectedPerson = People[0];
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("Binding:Reset", "Reset", "Binding", "\uE72C", ResetCommand)
    ];
}
