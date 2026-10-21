namespace Template.WindowsApp.Services;

using System.Windows.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class TaskbarService : NotificationObject
{
    public TaskbarItemProgressState State
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public double Value
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public void SetProgress(double value)
    {
        Value = value;
        State = TaskbarItemProgressState.Normal;
    }

    public void Clear()
    {
        State = TaskbarItemProgressState.None;
        Value = 0;
    }
}
