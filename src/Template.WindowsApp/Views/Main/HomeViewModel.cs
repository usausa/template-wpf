namespace Template.WindowsApp.Views.Main;

using Template.WindowsApp.Services;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class HomeViewModel : AppViewModelBase
{
    [ObservableProperty]
    public partial string Message { get; set; }

    public IObserveCommand ForwardCommand { get; }

    public HomeViewModel(GreetService greetService)
    {
        Message = greetService.MakeMessage();
        ForwardCommand = MakeAsyncCommand<ViewId>(x => Navigator.ForwardAsync(x));
    }
}
