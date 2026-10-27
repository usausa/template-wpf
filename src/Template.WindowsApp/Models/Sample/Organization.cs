namespace Template.WindowsApp.Models.Sample;

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public abstract partial class OrganizationNode : ObservableObject
{
    public string Name { get; }

    [ObservableProperty]
    public partial bool IsExpanded { get; set; }

    protected OrganizationNode(string name)
    {
        Name = name;
    }
}

public sealed class Department : OrganizationNode
{
    public IReadOnlyList<Team> Teams { get; }

    public Department(string name, IReadOnlyList<Team> teams)
        : base(name)
    {
        Teams = teams;
    }
}

public sealed class Team : OrganizationNode
{
    public IReadOnlyList<Member> Members { get; }

    public Team(string name, IReadOnlyList<Member> members)
        : base(name)
    {
        Members = members;
    }
}

public sealed class Member : OrganizationNode
{
    public Member(string name)
        : base(name)
    {
    }
}

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

public static class OrganizationSample
{
    public static IReadOnlyList<Department> LoadDepartments() =>
    [
        new("Sales",
        [
            new("Team A", [new("Emma Wilson"), new("Liam Chen")]),
            new("Team B", [new("Olivia Martinez"), new("Noah Johnson")])
        ]),
        new("Engineering",
        [
            new("Platform", [new("Ava Brown"), new("Ethan Davis")]),
            new("Apps", [new("Sophia Garcia"), new("Mason Miller")])
        ]),
        new("Support",
        [
            new("Customer care", [new("Isabella Lee"), new("Lucas Anderson")])
        ])
    ];
}
