namespace Template.WindowsApp.Views.Main;

using Template.WindowsApp.Services;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class SubViewModel : AppViewModelBase
{
    [ObservableProperty]
    public partial string Result { get; set; }

    public ICommand ConfirmCommand { get; }

    public ICommand InputCommand { get; }

    public ICommand BackCommand { get; }

    public SubViewModel(IDialogService dialogService)
    {
        Result = string.Empty;
        ConfirmCommand = MakeDelegateCommand(() =>
        {
            Result = dialogService.Confirm("Are you sure?") ? "Confirmed" : "Canceled";
        });
        InputCommand = MakeDelegateCommand(() =>
        {
            var value = dialogService.Input("Input value", Result);
            if (value is not null)
            {
                Result = value;
            }
        });
        BackCommand = MakeDelegateCommand(() => Navigator.Forward(ViewId.Menu));
    }
}
