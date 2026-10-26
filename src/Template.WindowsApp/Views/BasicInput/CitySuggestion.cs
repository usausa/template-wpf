namespace Template.WindowsApp.Views.BasicInput;

using Template.WindowsApp.Controls;

public sealed record CitySuggestion(string Name, IReadOnlyList<HighlightRun> NameRuns)
{
    public override string ToString() => Name;
}
