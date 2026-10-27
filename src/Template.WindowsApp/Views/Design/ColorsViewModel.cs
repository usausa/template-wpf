namespace Template.WindowsApp.Views.Design;

using Template.WindowsApp.Components;
using Template.WindowsApp.Models.Sample;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class ColorsViewModel : AppViewModelBase
{
    public IReadOnlyList<ColorCategory> Categories { get; } = ColorSample.LoadCategories();

    [ObservableProperty]
    public partial ColorCategory SelectedCategory { get; set; }

    [ObservableProperty]
    public partial string? Copied { get; set; }

    public IObserveCommand CopyCommand { get; }

    public ColorsViewModel(IClipboard clipboard)
    {
        SelectedCategory = Categories[0];

        CopyCommand = MakeDelegateCommand<string>(x =>
        {
            clipboard.SetText(x);
            Copied = x;
        });
    }
}
