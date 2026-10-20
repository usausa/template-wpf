namespace Template.WindowsApp.Views.Graphics;

using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class ShapesAnimationViewModel : AppViewModelBase, IShellCommandProvider
{
    [ObservableProperty]
    public partial double Angle { get; set; }

    [ObservableProperty]
    public partial double Scale { get; set; }

    public EventRequest PlayRequest { get; } = new();

    public EventRequest StopRequest { get; } = new();

    public IObserveCommand PlayCommand { get; }

    public IObserveCommand ResetCommand { get; }

    public ShapesAnimationViewModel()
    {
        Reset();

        PlayCommand = MakeDelegateCommand(PlayRequest.Request);
        ResetCommand = MakeDelegateCommand(Reset);
    }

    private void Reset()
    {
        Angle = 0;
        Scale = 1;
        StopRequest.Request();
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("ShapesAnimation:Play", "Play", "Shapes & animation", "\uE768", PlayCommand),
        new("ShapesAnimation:Reset", "Reset", "Shapes & animation", "\uE72C", ResetCommand)
    ];
}
