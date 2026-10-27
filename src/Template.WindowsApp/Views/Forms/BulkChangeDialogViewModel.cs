namespace Template.WindowsApp.Views.Forms;

using Template.WindowsApp.Models.Sample;

[ObservableGeneratorOption(Reactive = true, ViewModel = true)]
public sealed partial class BulkChangeDialogViewModel : AppDialogViewModelBase
{
    public string Title { get; init; } = string.Empty;

    [ObservableProperty]
    public partial OrderStatus? Status { get; set; }

    [ObservableProperty]
    public partial DateTime? Delivery { get; set; }

    [ObservableProperty]
    public partial double? Discount { get; set; }

    public IObserveCommand PrimaryCommand { get; }

    public IObserveCommand CloseCommand { get; }

    public BulkChangeDialogViewModel()
    {
        PrimaryCommand = MakeDelegateCommand(() => Complete(true), () => (Status is not null) || (Delivery is not null) || (Discount is not null));
        CloseCommand = MakeDelegateCommand(() => Complete(false));
    }
}
