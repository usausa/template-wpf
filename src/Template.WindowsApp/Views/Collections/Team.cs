namespace Template.WindowsApp.Views.Collections;

public sealed class Team : OrganizationNode
{
    public IReadOnlyList<Member> Members { get; }

    public Team(string name, IReadOnlyList<Member> members)
        : base(name)
    {
        Members = members;
    }
}
