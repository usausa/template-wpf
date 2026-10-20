namespace Template.WindowsApp.Views.Design;

using Template.WindowsApp.Services;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class TypographyViewModel : AppViewModelBase
{
    [ObservableProperty]
    public partial string? Copied { get; set; }

    public IObserveCommand CopyCommand { get; }

    public TypographyViewModel(IClipboardService clipboardService)
    {
        CopyCommand = MakeDelegateCommand<string>(x =>
        {
            clipboardService.SetText(x);
            Copied = x;
        });
    }
}
