namespace Template.WindowsApp.Shell;

public enum CommandPaletteMove
{
    Previous,
    Next,
    PreviousPage,
    NextPage
}

public sealed class CommandPalette : NotificationObject
{
    private const int RecentLimit = 10;

    private const int PageSize = 10;

    private const CompareOptions MatchOptions = CompareOptions.IgnoreCase | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth;

    private readonly IList<string> recentIds;

    private List<ShellCommand> sources = [];

    public bool IsOpen
    {
        get;
        set
        {
            if (SetProperty(ref field, value) && !value)
            {
                sources = [];
                Candidates = [];
            }
        }
    }

    public string Text
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                Update();
            }
        }
    }

    public IReadOnlyList<ShellCommand> Candidates
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public int SelectedIndex
    {
        get;
        set => SetProperty(ref field, value);
    }

    public CommandPalette(IList<string> recentIds)
    {
        this.recentIds = recentIds;
        Text = string.Empty;
        Candidates = [];
        SelectedIndex = -1;
    }

    public void Open(IEnumerable<ShellCommand> commands)
    {
        sources = commands.Where(static x => x.Command.CanExecute(x.Parameter)).ToList();
        Text = string.Empty;
        Update();
        IsOpen = true;
    }

    public void Close() => IsOpen = false;

    public void Move(CommandPaletteMove move)
    {
        var count = Candidates.Count;
        if (count == 0)
        {
            return;
        }

        SelectedIndex = move switch
        {
            CommandPaletteMove.Previous => SelectedIndex <= 0 ? count - 1 : SelectedIndex - 1,
            CommandPaletteMove.Next => SelectedIndex >= count - 1 ? 0 : SelectedIndex + 1,
            CommandPaletteMove.PreviousPage => Math.Max(SelectedIndex - PageSize, 0),
            _ => Math.Min(SelectedIndex + PageSize, count - 1)
        };
    }

    public ShellCommand? Pick(ShellCommand? command)
    {
        var target = command ?? ((SelectedIndex >= 0) && (SelectedIndex < Candidates.Count) ? Candidates[SelectedIndex] : null);
        if (target is null)
        {
            return null;
        }

        IsOpen = false;

        recentIds.Remove(target.Id);
        recentIds.Insert(0, target.Id);
        while (recentIds.Count > RecentLimit)
        {
            recentIds.RemoveAt(recentIds.Count - 1);
        }

        return target;
    }

    private void Update()
    {
        var text = Text.Trim();
        Candidates = sources
            .Where(x => (text.Length == 0) || IsMatch(x.Title, text) || IsMatch(x.Category, text))
            .OrderBy(x => RecentRank(x.Id))
            .ToList();
        SelectedIndex = Candidates.Count > 0 ? 0 : -1;
    }

    private int RecentRank(string id)
    {
        var index = recentIds.IndexOf(id);
        return index >= 0 ? index : RecentLimit;
    }

    private static bool IsMatch(string source, string text) =>
        CultureInfo.InvariantCulture.CompareInfo.IndexOf(source, text, MatchOptions) >= 0;
}
