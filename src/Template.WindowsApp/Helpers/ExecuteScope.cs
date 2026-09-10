namespace Template.WindowsApp.Helpers;

#pragma warning disable CA1815
public readonly struct ExecuteScope : IDisposable
{
    private readonly NotificationValue<bool> executing;

    public ExecuteScope(NotificationValue<bool> executing)
    {
        this.executing = executing;
        executing.Value = true;
    }

    public void Dispose() => executing.Value = false;
}
#pragma warning restore CA1815
