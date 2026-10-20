namespace Template.WindowsApp.Views;

[ObservableGeneratorOption(Reactive = true, ViewModel = true)]
public abstract class AppViewModelBase :
    ExtendViewModelBase,
    INotifyDataErrorInfo,
    INavigatorAware,
    INavigationEventSupportAsync,
    INavigationLifecycleSupport
{
    private readonly HashSet<string> errorKeys = [];

    public INavigator Navigator { get; set; } = default!;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public bool HasErrors => Errors.HasError;

    protected AppViewModelBase()
    {
        AcceptsCommand = false;

        Disposables.Add(Observable
            .FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(h => Errors.PropertyChanged += h, h => Errors.PropertyChanged -= h)
            .Where(static x => x.EventArgs.PropertyName == Binding.IndexerName)
            .Subscribe(_ => NotifyErrorsChanged()));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        System.Diagnostics.Debug.WriteLine($"{GetType()} is Disposed");
    }

    public void Validate(string name)
    {
        ValidationHelper.Validate(this, name, Errors);
    }

    public IEnumerable GetErrors(string? propertyName) =>
        String.IsNullOrEmpty(propertyName) ? Array.Empty<string>() : Errors.GetErrors(propertyName);

    private void NotifyErrorsChanged()
    {
        var keys = Errors.GetKeys().ToList();
        var changed = errorKeys.Union(keys).ToList();
        errorKeys.Clear();
        errorKeys.UnionWith(keys);
        foreach (var key in changed)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(key));
        }
    }

    public virtual Task OnNavigatingFromAsync(INavigationContext context) => Task.CompletedTask;

    public virtual Task OnNavigatingToAsync(INavigationContext context) => Task.CompletedTask;

    public virtual Task OnNavigatedToAsync(INavigationContext context) => Task.CompletedTask;

    public void OnActivated() => AcceptsCommand = true;

    public void OnDeactivated() => AcceptsCommand = false;
}
