namespace Template.WindowsApp.Views.Layout;

using Template.WindowsApp.Controls;
using Template.WindowsApp.Models;
using Template.WindowsApp.Shell;

public sealed partial class DocumentTab : ObservableObject
{
    public string Title { get; }

    [ObservableProperty]
    public partial string Text { get; set; }

    public DocumentTab(string title, string text)
    {
        Title = title;
        Text = text;
    }

    public override string ToString() => Title;
}

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class TabsViewModel : AppViewModelBase, IShellCommandProvider
{
    private const int FirstTabCount = 3;

    private const int LastTabNumber = 9;

    private int nextNumber = 1;

    public ObservableCollection<DocumentTab> Documents { get; } = [];

    [ObservableProperty]
    public partial DocumentTab? SelectedDocument { get; set; }

    [ObservableProperty]
    public partial TabViewWidthMode WidthMode { get; set; }

    [ObservableProperty]
    public partial TabViewCloseButtonOverlayMode CloseButtonMode { get; set; }

    public IObserveCommand AddCommand { get; }

    public IObserveCommand CloseCommand { get; }

    public IObserveCommand CloseSelectedCommand { get; }

    public IObserveCommand SelectNumberCommand { get; }

    public IObserveCommand MoveCommand { get; }

    public TabsViewModel()
    {
        for (var i = 0; i < FirstTabCount; i++)
        {
            Add();
        }
        SelectedDocument = Documents[0];

        AddCommand = MakeDelegateCommand(Add);
        CloseCommand = MakeDelegateCommand<DocumentTab>(Close);
        CloseSelectedCommand = MakeDelegateCommand(CloseSelected, () => SelectedDocument is not null);
        SelectNumberCommand = MakeDelegateCommand<int>(SelectNumber);
        MoveCommand = MakeDelegateCommand<ItemMove>(x => Documents.Move(x.OldIndex, x.NewIndex));
    }

    private void Add()
    {
        var title = String.Create(CultureInfo.InvariantCulture, $"Document {nextNumber}");
        nextNumber++;
        var document = new DocumentTab(title, $"{title}{Environment.NewLine}{Environment.NewLine}Type notes here. Ctrl+T opens a new tab and Ctrl+W closes this one.");
        Documents.Add(document);
        SelectedDocument = document;
    }

    private void Close(DocumentTab document)
    {
        var index = Documents.IndexOf(document);
        if (index < 0)
        {
            return;
        }

        var selected = SelectedDocument == document;
        Documents.RemoveAt(index);
        if (selected)
        {
            SelectedDocument = Documents.Count > 0 ? Documents[Math.Min(index, Documents.Count - 1)] : null;
        }
    }

    private void CloseSelected()
    {
        if (SelectedDocument is { } document)
        {
            Close(document);
        }
    }

    private void SelectNumber(int number)
    {
        if (Documents.Count == 0)
        {
            return;
        }

        if (number >= LastTabNumber)
        {
            SelectedDocument = Documents[^1];
        }
        else if (number <= Documents.Count)
        {
            SelectedDocument = Documents[number - 1];
        }
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("Tabs:New", "New tab", "Tabs", "\uE710", AddCommand),
        new("Tabs:Close", "Close tab", "Tabs", "\uE711", CloseSelectedCommand)
    ];
}
