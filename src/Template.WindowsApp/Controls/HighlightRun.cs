namespace Template.WindowsApp.Controls;

public readonly record struct HighlightRun(string Text, bool IsHighlighted)
{
    public static IReadOnlyList<HighlightRun> Split(string text, bool[] marks)
    {
        var runs = new List<HighlightRun>();
        var start = 0;
        for (var i = 1; i <= text.Length; i++)
        {
            if ((i == text.Length) || (marks[i] != marks[start]))
            {
                runs.Add(new HighlightRun(text[start..i], marks[start]));
                start = i;
            }
        }

        return runs;
    }
}
