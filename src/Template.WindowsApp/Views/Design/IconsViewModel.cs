namespace Template.WindowsApp.Views.Design;

using System.Text.RegularExpressions;

using Template.WindowsApp.Components;
using Template.WindowsApp.Models.Sample;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class IconsViewModel : AppViewModelBase
{
    private const int PageSize = 200;

    private static readonly Lazy<IconEntry[]> AllIcons = new(IconSample.LoadIcons);

    private static readonly char[] TermSeparators = [' ', '+'];

    private readonly IClipboard clipboard;

    private IconEntry[] matches = [];

    private int pageIndex;

    [ObservableProperty]
    public partial string SearchText { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<IconEntry> Items { get; set; }

    [ObservableProperty]
    public partial IconEntry? SelectedIcon { get; set; }

    [ObservableProperty]
    public partial int RangeStart { get; set; }

    [ObservableProperty]
    public partial int RangeEnd { get; set; }

    [ObservableProperty]
    public partial int TotalCount { get; set; }

    [ObservableProperty]
    public partial string? Copied { get; set; }

    public EventRequest FindRequest { get; } = new();

    public IObserveCommand PreviousCommand { get; }

    public IObserveCommand NextCommand { get; }

    public IObserveCommand CopyCommand { get; }

    public IObserveCommand CopyNameCommand { get; }

    public IObserveCommand CopyCodeCommand { get; }

    public IObserveCommand CopyXamlCommand { get; }

    public IObserveCommand CopyCSharpCommand { get; }

    public IObserveCommand FindCommand { get; }

    public IconsViewModel(IClipboard clipboard)
    {
        this.clipboard = clipboard;

        SearchText = string.Empty;
        Items = [];

        SubscribeSearchText(_ => Filter());

        PreviousCommand = MakeDelegateCommand(() => ShowPage(pageIndex - 1), () => pageIndex > 0);
        NextCommand = MakeDelegateCommand(() => ShowPage(pageIndex + 1), () => RangeEnd < TotalCount);
        CopyCommand = MakeDelegateCommand<string>(Copy);
        CopyNameCommand = MakeDelegateCommand(() => CopySelected(static x => x.Name), () => SelectedIcon is not null);
        CopyCodeCommand = MakeDelegateCommand(() => CopySelected(static x => x.Code), () => SelectedIcon is not null);
        CopyXamlCommand = MakeDelegateCommand(() => CopySelected(static x => x.Xaml), () => SelectedIcon is not null);
        CopyCSharpCommand = MakeDelegateCommand(() => CopySelected(static x => x.CSharp), () => SelectedIcon is not null);
        FindCommand = MakeDelegateCommand(FindRequest.Request);

        Filter();
    }

    [GeneratedRegex(@"\s*\|\s*")]
    private static partial Regex AlternativeRegex();

    private static bool IsMatch(IconEntry icon, string word) =>
        icon.Name.Contains(word, StringComparison.OrdinalIgnoreCase) || icon.Code.StartsWith(word, StringComparison.OrdinalIgnoreCase);

    private void Filter()
    {
        var terms = AlternativeRegex().Replace(SearchText, "|")
            .Split(TermSeparators, StringSplitOptions.RemoveEmptyEntries)
            .Select(static x => x.Split('|', StringSplitOptions.RemoveEmptyEntries))
            .Where(static x => x.Length > 0)
            .ToArray();
        matches = terms.Length == 0 ? AllIcons.Value : AllIcons.Value.Where(x => terms.All(y => y.Any(z => IsMatch(x, z)))).ToArray();
        TotalCount = matches.Length;
        ShowPage(0);
    }

    private void ShowPage(int index)
    {
        pageIndex = index;
        Items = matches.Skip(index * PageSize).Take(PageSize).ToArray();
        RangeStart = Items.Count > 0 ? (index * PageSize) + 1 : 0;
        RangeEnd = (index * PageSize) + Items.Count;
        SelectedIcon = Items.Count > 0 ? Items[0] : null;
    }

    private void Copy(string text)
    {
        clipboard.SetText(text);
        Copied = text;
    }

    private void CopySelected(Func<IconEntry, string> selector)
    {
        if (SelectedIcon is not null)
        {
            Copy(selector(SelectedIcon));
        }
    }
}
