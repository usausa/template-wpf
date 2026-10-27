namespace Template.WindowsApp.Views;

public abstract class AppDialogViewModelBase : ExtendViewModelBase
{
    private readonly TaskCompletionSource<bool> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task<bool> Completion => completion.Task;

    protected AppDialogViewModelBase()
        : base(new ExtendViewModelOptions { BusyState = new BusyState() })
    {
    }

    protected void Complete(bool result) => completion.TrySetResult(result);
}
