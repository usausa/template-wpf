namespace Template.WindowsApp.Views.BasicInput;

using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class ButtonsViewModel : AppViewModelBase, IShellCommandProvider
{
    private const string DefaultSortBy = "Name";

    [ObservableProperty]
    public partial int ClickCount { get; set; }

    [ObservableProperty]
    public partial int RepeatCount { get; set; }

    [ObservableProperty]
    public partial bool IsToggled { get; set; }

    [ObservableProperty]
    public partial string? LastPressed { get; set; }

    [ObservableProperty]
    public partial string SortBy { get; set; }

    [ObservableProperty]
    public partial string? LastAction { get; set; }

    public IObserveCommand ClickCommand { get; }

    public IObserveCommand RepeatCommand { get; }

    public IObserveCommand PressCommand { get; }

    public IObserveCommand SortByCommand { get; }

    public IObserveCommand SaveCommand { get; }

    public IObserveCommand ResetCommand { get; }

    public ButtonsViewModel()
    {
        ClickCommand = MakeDelegateCommand(() => ClickCount++);
        RepeatCommand = MakeDelegateCommand(() => RepeatCount++);
        SortBy = DefaultSortBy;

        PressCommand = MakeDelegateCommand<string>(x => LastPressed = x);
        SortByCommand = MakeDelegateCommand<string>(SelectSortBy);
        SaveCommand = MakeDelegateCommand<string>(x => LastAction = x);
        ResetCommand = MakeDelegateCommand(Reset);
    }

    private void Reset()
    {
        ClickCount = 0;
        RepeatCount = 0;
        IsToggled = false;
        LastPressed = null;
        SortBy = DefaultSortBy;
        LastAction = null;
    }

    private void SelectSortBy(string sortBy)
    {
        SortBy = sortBy;
        RaisePropertyChanged(nameof(SortBy));
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("Buttons:Reset", "Reset", "Buttons", "\uE72C", ResetCommand)
    ];
}
