namespace Template.WindowsApp.Views.Layout;

using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class ContainersViewModel : AppViewModelBase, IShellCommandProvider
{
    private static readonly GridLength HalfWidth = new(1, GridUnitType.Star);

    [ObservableProperty]
    public partial bool IsFirstExpanded { get; set; }

    [ObservableProperty]
    public partial bool IsSecondExpanded { get; set; }

    [ObservableProperty]
    public partial int ExpandedCount { get; set; }

    [ObservableProperty]
    public partial int TabIndex { get; set; }

    [ObservableProperty]
    public partial GridLength LeftWidth { get; set; }

    [ObservableProperty]
    public partial GridLength RightWidth { get; set; }

    public IObserveCommand ResetCommand { get; }

    public ContainersViewModel()
    {
        Reset();

        SubscribeIsFirstExpanded(_ => UpdateExpandedCount());
        SubscribeIsSecondExpanded(_ => UpdateExpandedCount());

        ResetCommand = MakeDelegateCommand(Reset);
    }

    private void Reset()
    {
        IsFirstExpanded = true;
        IsSecondExpanded = false;
        TabIndex = 0;
        LeftWidth = HalfWidth;
        RightWidth = HalfWidth;
        UpdateExpandedCount();
    }

    private void UpdateExpandedCount() =>
        ExpandedCount = (IsFirstExpanded ? 1 : 0) + (IsSecondExpanded ? 1 : 0);

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("Containers:Reset", "Reset", "Containers", "\uE72C", ResetCommand)
    ];
}
