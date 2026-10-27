namespace Template.WindowsApp.Views.Platform;

using Template.WindowsApp.Components;
using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class WindowsViewModel : AppViewModelBase, IShellCommandProvider
{
    private readonly IWindowManager windowManager;

    [ObservableProperty]
    public partial string WindowResult { get; set; }

    public IObserveCommand NewWindowCommand { get; }

    public IObserveCommand ToolWindowCommand { get; }

    public IObserveCommand CompactOverlayCommand { get; }

    public WindowsViewModel(IWindowManager windowManager, ProcessMonitor processMonitor)
    {
        this.windowManager = windowManager;

        WindowResult = string.Empty;

        NewWindowCommand = MakeDelegateCommand(() => windowManager.Show(WindowId.Sample, new SampleWindowViewModel()));
        ToolWindowCommand = MakeDelegateCommand(() => windowManager.Show(WindowId.Tool, new SampleWindowViewModel { ShowInTaskbar = false }, true));
        CompactOverlayCommand = MakeDelegateCommand(() =>
        {
            if (!windowManager.Activate(WindowId.Memory))
            {
                windowManager.Show(WindowId.Memory, new MemoryWindowViewModel(processMonitor));
            }
        });

        UpdateWindowResult();
        Disposables.Add(Observable
            .FromEventPattern(h => windowManager.WindowsChanged += h, h => windowManager.WindowsChanged -= h)
            .Subscribe(_ => UpdateWindowResult()));
    }

    private void UpdateWindowResult()
    {
        var count = windowManager.Count(WindowId.Sample);
        WindowResult = count > 0 ? String.Create(CultureInfo.CurrentCulture, $"{count} open") : string.Empty;
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("Windows:NewWindow", "New window", "Windows", "\uE78B", NewWindowCommand),
        new("Windows:ToolWindow", "Tool window", "Windows", "\uE8A7", ToolWindowCommand),
        new("Windows:CompactOverlay", "Compact overlay", "Windows", "\uEE49", CompactOverlayCommand)
    ];
}
