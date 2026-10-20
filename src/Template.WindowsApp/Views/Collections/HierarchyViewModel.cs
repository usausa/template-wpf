namespace Template.WindowsApp.Views.Collections;

using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class HierarchyViewModel : AppViewModelBase, IShellCommandProvider
{
    private const int LeafDepth = 2;

    private static readonly TimeSpan LoadDelay = TimeSpan.FromMilliseconds(500);

    private static readonly string[] SubfolderNames = ["Archive", "Drafts", "Shared"];

    private static readonly Dictionary<string, string[]> FolderNames = new()
    {
        ["Projects"] = ["Alpha", "Beta", "Gamma"],
        ["Documents"] = ["Contracts", "Invoices", "Reports"],
        ["Pictures"] = ["2024", "2025", "2026"]
    };

    public IReadOnlyList<Department> Departments { get; } =
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

    public IReadOnlyList<FolderNode> Folders { get; } =
    [
        new("Projects", 0, true),
        new("Documents", 0, true),
        new("Pictures", 0, true)
    ];

    [ObservableProperty]
    public partial object? SelectedNode { get; set; }

    [ObservableProperty]
    public partial string? SelectedPath { get; set; }

    [ObservableProperty]
    public partial int LoadedCount { get; set; }

    public IObserveCommand LoadCommand { get; }

    public IObserveCommand ExpandAllCommand { get; }

    public IObserveCommand CollapseAllCommand { get; }

    public HierarchyViewModel()
    {
        SubscribeSelectedNode(x => SelectedPath = MakePath(x));

        LoadCommand = MakeAsyncCommand<FolderNode>(CommandMode.Simple, LoadAsync);
        ExpandAllCommand = MakeDelegateCommand(() => SetExpanded(true));
        CollapseAllCommand = MakeDelegateCommand(() => SetExpanded(false));
    }

    private async Task LoadAsync(FolderNode folder)
    {
        if (folder.IsPlaceholder || folder.IsLoaded)
        {
            return;
        }

        folder.IsLoaded = true;
        await Task.Delay(LoadDelay).ConfigureAwait(true);

        folder.Children.Clear();
        var depth = folder.Depth + 1;
        foreach (var name in folder.Depth == 0 ? FolderNames[folder.Name] : SubfolderNames)
        {
            folder.Children.Add(new FolderNode(name, depth, depth < LeafDepth));
        }

        LoadedCount++;
    }

    private string? MakePath(object? node)
    {
        foreach (var department in Departments)
        {
            if (ReferenceEquals(department, node))
            {
                return department.Name;
            }

            foreach (var team in department.Teams)
            {
                if (ReferenceEquals(team, node))
                {
                    return $"{department.Name} › {team.Name}";
                }

                foreach (var member in team.Members)
                {
                    if (ReferenceEquals(member, node))
                    {
                        return $"{department.Name} › {team.Name} › {member.Name}";
                    }
                }
            }
        }

        return null;
    }

    private void SetExpanded(bool value)
    {
        foreach (var department in Departments)
        {
            department.IsExpanded = value;
            foreach (var team in department.Teams)
            {
                team.IsExpanded = value;
            }
        }
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("Hierarchy:ExpandAll", "Expand all", "Hierarchy", "\uE70D", ExpandAllCommand),
        new("Hierarchy:CollapseAll", "Collapse all", "Hierarchy", "\uE70E", CollapseAllCommand)
    ];
}
