namespace Template.WindowsApp.Helpers;

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
