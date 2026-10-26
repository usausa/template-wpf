namespace Template.WindowsApp.Views.Fundamentals;

using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class CustomControlsViewModel : AppViewModelBase, IShellCommandProvider
{
    private static readonly string[] DefaultTags = ["XAML", "WPF", "MVVM", "Binding", "Styles"];

    public ObservableCollection<string> Tags { get; } = [.. DefaultTags];

    [ObservableProperty]
    public partial string PostalCode { get; set; }

    [ObservableProperty]
    public partial int EnterCount { get; set; }

    public IObserveCommand RemoveTagCommand { get; }

    public IObserveCommand ResetTagsCommand { get; }

    public IObserveCommand EnterCommand { get; }

    public CustomControlsViewModel()
    {
        PostalCode = string.Empty;

        RemoveTagCommand = MakeDelegateCommand<string>(x => Tags.Remove(x));
        ResetTagsCommand = MakeDelegateCommand(ResetTags);
        EnterCommand = MakeDelegateCommand(() => EnterCount++);
    }

    private void ResetTags()
    {
        Tags.Clear();
        foreach (var tag in DefaultTags)
        {
            Tags.Add(tag);
        }
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("CustomControls:ResetTags", "Reset tags", "Custom controls", "\uE72C", ResetTagsCommand)
    ];
}
