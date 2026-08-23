namespace Template.WindowsApp.Views.Main;

using Template.WindowsApp.Services;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class MenuViewModel : AppViewModelBase
{
    [ObservableProperty]
    public partial string Message { get; set; }

    public ICommand NavigateCommand { get; }

    public MenuViewModel(GreetService greetService)
    {
        Message = greetService.MakeMessage();
        NavigateCommand = MakeDelegateCommand(() => Navigator.Forward(ViewId.Sub));
    }
}
