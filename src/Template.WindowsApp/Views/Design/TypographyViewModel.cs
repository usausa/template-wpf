namespace Template.WindowsApp.Views.Design;

using Template.WindowsApp.Components;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class TypographyViewModel : AppViewModelBase
{
    [ObservableProperty]
    public partial string? Copied { get; set; }

    public IObserveCommand CopyCommand { get; }

    public TypographyViewModel(IClipboard clipboard)
    {
        CopyCommand = MakeDelegateCommand<string>(x =>
        {
            clipboard.SetText(x);
            Copied = x;
        });
    }
}
