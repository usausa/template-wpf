namespace Template.WindowsApp.Views.Dialogs;

public abstract class DialogViewModelBase : ExtendViewModelBase
{
    private readonly TaskCompletionSource<bool> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task<bool> Completion => completion.Task;

    protected DialogViewModelBase()
        : base(new ExtendViewModelOptions { BusyState = new BusyState() })
    {
    }

    protected void Complete(bool result) => completion.TrySetResult(result);
}
