namespace Template.WindowsApp.Views;

[ObservableGeneratorOption(Reactive = true, ViewModel = true)]
public abstract class AppViewModelBase :
    ExtendViewModelBase,
    INavigatorAware,
    INavigationEventSupport,
    INavigationLifecycleSupport
{
    public INavigator Navigator { get; set; } = default!;

    protected AppViewModelBase()
    {
        AcceptsCommand = false;
    }

    public void OnNavigatingFrom(INavigationContext context)
    {
    }

    public void OnNavigatingTo(INavigationContext context)
    {
    }

    public void OnNavigatedTo(INavigationContext context)
    {
    }

    public void OnActivated() => AcceptsCommand = true;

    public void OnDeactivated() => AcceptsCommand = false;
}
