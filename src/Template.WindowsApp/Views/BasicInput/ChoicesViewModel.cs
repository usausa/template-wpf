namespace Template.WindowsApp.Views.BasicInput;

using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class ChoicesViewModel : AppViewModelBase, IShellCommandProvider
{
    private const string DefaultSize = "Small";

    private const string DefaultFruit = "Apple";

    private const double DefaultValue = 50;

    [ObservableProperty]
    public partial bool TwoState { get; set; }

    [ObservableProperty]
    public partial bool? ThreeState { get; set; }

    [ObservableProperty]
    public partial bool IsSwitchOn { get; set; }

    [ObservableProperty]
    public partial string Size { get; set; }

    public IReadOnlyList<string> Fruits { get; } = [DefaultFruit, "Banana", "Cherry", "Grape", "Lemon"];

    [ObservableProperty]
    public partial string Fruit { get; set; }

    [ObservableProperty]
    public partial double Value { get; set; }

    public IObserveCommand ResetCommand { get; }

    public ChoicesViewModel()
    {
        Size = DefaultSize;
        Fruit = DefaultFruit;
        Value = DefaultValue;

        ResetCommand = MakeDelegateCommand(Reset);
    }

    private void Reset()
    {
        TwoState = false;
        ThreeState = null;
        IsSwitchOn = false;
        Size = DefaultSize;
        Fruit = DefaultFruit;
        Value = DefaultValue;
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("Choices:Reset", "Reset", "Choices", "\uE72C", ResetCommand)
    ];
}
