namespace Template.WindowsApp.Views.Collections;

public sealed class Department : OrganizationNode
{
    public IReadOnlyList<Team> Teams { get; }

    public Department(string name, IReadOnlyList<Team> teams)
        : base(name)
    {
        Teams = teams;
    }
}
