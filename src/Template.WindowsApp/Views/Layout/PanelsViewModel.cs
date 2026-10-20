namespace Template.WindowsApp.Views.Layout;

using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class PanelsViewModel : AppViewModelBase, IShellCommandProvider
{
    private const string DefaultOrientation = "Vertical";

    private const double DefaultWrapWidth = 320;

    private const int DefaultColumns = 3;

    public IReadOnlyList<int> StackItems { get; } = [1, 2, 3, 4];

    public IReadOnlyList<int> WrapItems { get; } = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

    public IReadOnlyList<int> GridItems { get; } = [1, 2, 3, 4, 5, 6, 7, 8];

    [ObservableProperty]
    public partial string Orientation { get; set; }

    [ObservableProperty]
    public partial double WrapWidth { get; set; }

    [ObservableProperty]
    public partial int Columns { get; set; }

    public IObserveCommand ResetCommand { get; }

    public PanelsViewModel()
    {
        Orientation = DefaultOrientation;
        WrapWidth = DefaultWrapWidth;
        Columns = DefaultColumns;

        ResetCommand = MakeDelegateCommand(Reset);
    }

    private void Reset()
    {
        Orientation = DefaultOrientation;
        WrapWidth = DefaultWrapWidth;
        Columns = DefaultColumns;
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("Panels:Reset", "Reset", "Panels", "\uE72C", ResetCommand)
    ];
}
