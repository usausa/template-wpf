namespace Template.WindowsApp.Shell;

using Template.WindowsApp.Controls;

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

    private const int TitleStartScore = 100;

    private const int WordStartScore = 80;

    private const int ContainsScore = 60;

    private const int SequenceScore = 30;

    private const int CategoryScore = 20;

    private const int CountScore = 5;

    private const CompareOptions MatchOptions = CompareOptions.IgnoreCase | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth;

    private static readonly CompareInfo Compare = CultureInfo.InvariantCulture.CompareInfo;

    private readonly IList<string> recentIds;

    private readonly IDictionary<string, int> counts;

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

    public IReadOnlyList<CommandCandidate> Candidates
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public int SelectedIndex
    {
        get;
        set => SetProperty(ref field, value);
    }

    public CommandPalette(IList<string> recentIds, IDictionary<string, int> counts)
    {
        this.recentIds = recentIds;
        this.counts = counts;
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

    public ShellCommand? Pick(CommandCandidate? candidate)
    {
        var target = (candidate ?? ((SelectedIndex >= 0) && (SelectedIndex < Candidates.Count) ? Candidates[SelectedIndex] : null))?.Command;
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
        counts[target.Id] = UseCount(target.Id) + 1;

        return target;
    }

    private void Update()
    {
        var words = Text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        Candidates = words.Length == 0
            ? sources
                .OrderBy(x => RecentRank(x.Id))
                .Select(static x => new CommandCandidate(x, [new HighlightRun(x.Title, false)]))
                .ToList()
            : sources
                .Select(x => (Command: x, Match: Match(x, words)))
                .Where(static x => x.Match.Score > 0)
                .OrderByDescending(x => x.Match.Score + (UseCount(x.Command.Id) * CountScore))
                .ThenBy(x => RecentRank(x.Command.Id))
                .Select(static x => new CommandCandidate(x.Command, x.Match.Runs))
                .ToList();
        SelectedIndex = Candidates.Count > 0 ? 0 : -1;
    }

    private int RecentRank(string id)
    {
        var index = recentIds.IndexOf(id);
        return index >= 0 ? index : RecentLimit;
    }

    private int UseCount(string id) => counts.TryGetValue(id, out var count) ? count : 0;

    private static (int Score, IReadOnlyList<HighlightRun> Runs) Match(ShellCommand command, IEnumerable<string> words)
    {
        var title = command.Title;
        var marks = new bool[title.Length];
        var score = 0;
        foreach (var word in words)
        {
            var wordScore = MatchTitle(title, word, marks);
            if (wordScore == 0)
            {
                if (Compare.IndexOf(command.Category, word, MatchOptions) < 0)
                {
                    return (0, []);
                }

                wordScore = CategoryScore;
            }

            score += wordScore;
        }

        return (score, HighlightRun.Split(title, marks));
    }

    private static int MatchTitle(string title, string word, bool[] marks)
    {
        var first = Compare.IndexOf(title.AsSpan(), word.AsSpan(), MatchOptions, out var firstLength);
        if (first >= 0)
        {
            var index = first;
            var length = firstLength;
            while ((index >= 0) && !IsWordStart(title, index))
            {
                var next = Compare.IndexOf(title.AsSpan(index + 1), word.AsSpan(), MatchOptions, out length);
                index = next >= 0 ? index + 1 + next : -1;
            }

            if (index >= 0)
            {
                marks.AsSpan(index, length).Fill(true);
                return index == 0 ? TitleStartScore : WordStartScore;
            }

            marks.AsSpan(first, firstLength).Fill(true);
            return ContainsScore;
        }

        var positions = new List<(int Index, int Length)>(word.Length);
        var start = 0;
        for (var i = 0; i < word.Length; i++)
        {
            var found = Compare.IndexOf(title.AsSpan(start), word.AsSpan(i, 1), MatchOptions, out var charLength);
            if (found < 0)
            {
                return 0;
            }

            positions.Add((start + found, charLength));
            start += found + charLength;
        }

        foreach (var (index, length) in positions)
        {
            marks.AsSpan(index, length).Fill(true);
        }

        return SequenceScore;
    }

    private static bool IsWordStart(string title, int index) =>
        (index == 0) || !Char.IsLetterOrDigit(title[index - 1]);
}
