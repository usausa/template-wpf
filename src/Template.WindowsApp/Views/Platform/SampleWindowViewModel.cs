namespace Template.WindowsApp.Views.Platform;

[ObservableGeneratorOption(Reactive = true, ViewModel = true)]
public sealed partial class SampleWindowViewModel : ExtendViewModelBase
{
    private static int sequence;

    public string Title { get; }

    [ObservableProperty]
    public partial bool IsTopmost { get; set; }

    [ObservableProperty]
    public partial bool IsResizable { get; set; }

    [ObservableProperty]
    public partial bool ShowInTaskbar { get; set; }

    [ObservableProperty]
    public partial bool ShowIcon { get; set; }

    public SampleWindowViewModel()
    {
        Title = String.Create(CultureInfo.CurrentCulture, $"Window {++sequence}");
        IsResizable = true;
        ShowInTaskbar = true;
        ShowIcon = true;
    }
}
