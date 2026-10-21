namespace Template.WindowsApp.Shell;

using Template.WindowsApp.Controls;

public sealed class CommandCandidate
{
    public ShellCommand Command { get; }

    public IReadOnlyList<HighlightRun> TitleRuns { get; }

    public CommandCandidate(ShellCommand command, IReadOnlyList<HighlightRun> titleRuns)
    {
        Command = command;
        TitleRuns = titleRuns;
    }

    public override string ToString() => Command.Title;
}
