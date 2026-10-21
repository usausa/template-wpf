namespace Template.WindowsApp.Shell;

public sealed class DialogHost : NotificationObject
{
    private Task pending = Task.CompletedTask;

    public object? Current
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public async Task<T> ShowAsync<TDialog, T>(TDialog dialog, Func<TDialog, Task<T>> wait)
    {
        var previous = pending;
        var done = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        pending = done.Task;
        await previous.ConfigureAwait(true);

        var focused = Keyboard.FocusedElement;
        Current = dialog;
        try
        {
            return await wait(dialog).ConfigureAwait(true);
        }
        finally
        {
            Current = null;
            focused?.Focus();
            done.SetResult();
        }
    }
}
