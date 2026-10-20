namespace Template.WindowsApp.Views.Design;

using System.Text.Json;

using Template.WindowsApp.Services;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class IconsViewModel : AppViewModelBase
{
    private const int PageSize = 200;

    private const string ResourceName = "Template.WindowsApp.Views.Design.Icons.json";

    private static readonly Lazy<IconEntry[]> AllIcons = new(LoadIcons);

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

    public IObserveCommand PreviousCommand { get; }

    public IObserveCommand NextCommand { get; }

    public IObserveCommand CopyCommand { get; }

    public IconsViewModel(IClipboardService clipboardService)
    {
        SearchText = string.Empty;
        Items = [];

        SubscribeSearchText(_ => Filter());

        PreviousCommand = MakeDelegateCommand(() => ShowPage(pageIndex - 1), () => pageIndex > 0);
        NextCommand = MakeDelegateCommand(() => ShowPage(pageIndex + 1), () => RangeEnd < TotalCount);
        CopyCommand = MakeDelegateCommand<string>(x =>
        {
            clipboardService.SetText(x);
            Copied = x;
        });

        Filter();
    }

    private static IconEntry[] LoadIcons()
    {
        using var stream = typeof(IconsViewModel).Assembly.GetManifestResourceStream(ResourceName)!;
        return JsonSerializer.Deserialize<IconEntry[]>(stream) ?? [];
    }

    private void Filter()
    {
        var text = SearchText.Trim();
        matches = text.Length == 0 ? AllIcons.Value : AllIcons.Value.Where(x => x.Name.Contains(text, StringComparison.OrdinalIgnoreCase)).ToArray();
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
}
