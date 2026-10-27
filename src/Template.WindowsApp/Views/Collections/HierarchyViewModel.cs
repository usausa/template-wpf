namespace Template.WindowsApp.Views.Collections;

using Template.WindowsApp.Models.Sample;
using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class HierarchyViewModel : AppViewModelBase, IShellCommandProvider
{
    private static readonly TimeSpan LoadDelay = TimeSpan.FromMilliseconds(500);

    public IReadOnlyList<Department> Departments { get; } = OrganizationSample.LoadDepartments();

    public IReadOnlyList<FolderNode> Folders { get; } = FolderSample.LoadRoots();

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
        foreach (var child in FolderSample.LoadChildren(folder))
        {
            folder.Children.Add(child);
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
