namespace Template.WindowsApp.Views.BasicInput;

using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class DateTimeViewModel : AppViewModelBase, IShellCommandProvider
{
    private const int LimitDays = 30;

    [ObservableProperty]
    public partial DateTime? Date { get; set; }

    [ObservableProperty]
    public partial DateTime? CalendarDate { get; set; }

    [ObservableProperty]
    public partial DateTime? RangeStart { get; set; }

    [ObservableProperty]
    public partial DateTime? RangeEnd { get; set; }

    [ObservableProperty]
    public partial int RangeDays { get; set; }

    [ObservableProperty]
    public partial DateTime? LimitedDate { get; set; }

    [ObservableProperty]
    public partial TimeSpan? Time { get; set; }

    [ObservableProperty]
    public partial TimeSpan? StepTime { get; set; }

    public DateTime FirstDate { get; }

    public DateTime LastDate { get; }

    public EventRequest ClearRangeRequest { get; } = new();

    public IObserveCommand RangeCommand { get; }

    public IObserveCommand TodayCommand { get; }

    public IObserveCommand ResetCommand { get; }

    public DateTimeViewModel()
    {
        FirstDate = DateTime.Today;
        LastDate = FirstDate.AddDays(LimitDays);

        RangeCommand = MakeDelegateCommand<IList<DateTime>>(CommandMode.Simple, UpdateRange);
        TodayCommand = MakeDelegateCommand(SetToday);
        ResetCommand = MakeDelegateCommand(Reset);
    }

    private void UpdateRange(IList<DateTime> dates)
    {
        RangeStart = dates.Count > 0 ? dates.Min() : null;
        RangeEnd = dates.Count > 0 ? dates.Max() : null;
        RangeDays = dates.Count;
    }

    private void SetToday()
    {
        Date = DateTime.Today;
        CalendarDate = DateTime.Today;
        LimitedDate = DateTime.Today;
    }

    private void Reset()
    {
        Date = null;
        CalendarDate = null;
        LimitedDate = null;
        Time = null;
        StepTime = null;
        ClearRangeRequest.Request();
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("DateTime:Today", "Today", "Date & time", "\uE787", TodayCommand),
        new("DateTime:Reset", "Reset", "Date & time", "\uE72C", ResetCommand)
    ];
}
