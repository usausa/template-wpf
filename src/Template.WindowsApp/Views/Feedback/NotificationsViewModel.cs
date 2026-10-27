namespace Template.WindowsApp.Views.Feedback;

using Template.WindowsApp.Components;
using Template.WindowsApp.Models;
using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class NotificationsViewModel : AppViewModelBase, IShellCommandProvider
{
    private readonly NotificationCenter notificationCenter;

    [ObservableProperty]
    public partial bool IsInformationalOpen { get; set; }

    [ObservableProperty]
    public partial bool IsSuccessOpen { get; set; }

    [ObservableProperty]
    public partial bool IsWarningOpen { get; set; }

    [ObservableProperty]
    public partial bool IsErrorOpen { get; set; }

    [ObservableProperty]
    public partial int OpenCount { get; set; }

    [ObservableProperty]
    public partial bool IsActionOpen { get; set; }

    [ObservableProperty]
    public partial string? LastAction { get; set; }

    public IReadOnlyList<InfoBarSeverity> Severities { get; } =
    [
        InfoBarSeverity.Informational,
        InfoBarSeverity.Success,
        InfoBarSeverity.Warning,
        InfoBarSeverity.Error
    ];

    [ObservableProperty]
    public partial InfoBarSeverity Severity { get; set; }

    [ObservableProperty]
    public partial int ShownCount { get; set; }

    public IObserveCommand ReopenCommand { get; }

    public IObserveCommand RestartCommand { get; }

    public IObserveCommand ShowCommand { get; }

    public NotificationsViewModel(NotificationCenter notificationCenter)
    {
        this.notificationCenter = notificationCenter;

        Reopen();
        Severity = InfoBarSeverity.Success;

        SubscribeIsInformationalOpen(_ => UpdateOpenCount());
        SubscribeIsSuccessOpen(_ => UpdateOpenCount());
        SubscribeIsWarningOpen(_ => UpdateOpenCount());
        SubscribeIsErrorOpen(_ => UpdateOpenCount());

        ReopenCommand = MakeDelegateCommand(Reopen);
        RestartCommand = MakeDelegateCommand(() => LastAction = "Restart");
        ShowCommand = MakeDelegateCommand(Show);
    }

    private void Reopen()
    {
        IsInformationalOpen = true;
        IsSuccessOpen = true;
        IsWarningOpen = true;
        IsErrorOpen = true;
        IsActionOpen = true;
        UpdateOpenCount();
    }

    private void UpdateOpenCount() =>
        OpenCount = (IsInformationalOpen ? 1 : 0) + (IsSuccessOpen ? 1 : 0) + (IsWarningOpen ? 1 : 0) + (IsErrorOpen ? 1 : 0);

    private void Show()
    {
        notificationCenter.Show(Severity, "Notification", "This notification closes after 5 seconds.");
        ShownCount++;
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("Notifications:Show", "Show notification", "Notifications", "\uEA8F", ShowCommand),
        new("Notifications:Reopen", "Reopen bars", "Notifications", "\uE72C", ReopenCommand)
    ];
}
