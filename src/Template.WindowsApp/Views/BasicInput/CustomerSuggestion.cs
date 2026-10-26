namespace Template.WindowsApp.Views.BasicInput;

using Template.WindowsApp.Controls;

public sealed record CustomerSuggestion(CustomerEntry Customer, IReadOnlyList<HighlightRun> NameRuns, IReadOnlyList<HighlightRun> CompanyRuns)
{
    public override string ToString() => Customer.Name;
}
