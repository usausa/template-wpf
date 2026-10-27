namespace Template.WindowsApp.Models.Sample;

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public sealed class FolderNode
{
    public string Name { get; }

    public int Depth { get; }

    public bool IsPlaceholder { get; }

    public bool IsLoaded { get; set; }

    public ObservableCollection<FolderNode> Children { get; } = [];

    public FolderNode(string name, int depth, bool hasChildren)
    {
        Name = name;
        Depth = depth;
        if (hasChildren)
        {
            Children.Add(new FolderNode());
        }
    }

    private FolderNode()
    {
        Name = string.Empty;
        IsPlaceholder = true;
    }
}

//--------------------------------------------------------------------------------
// Sample
//--------------------------------------------------------------------------------

public static class FolderSample
{
    private const int LeafDepth = 2;

    private static readonly string[] SubfolderNames = ["Archive", "Drafts", "Shared"];

    private static readonly Dictionary<string, string[]> FolderNames = new()
    {
        ["Projects"] = ["Alpha", "Beta", "Gamma"],
        ["Documents"] = ["Contracts", "Invoices", "Reports"],
        ["Pictures"] = ["2024", "2025", "2026"]
    };

    public static IReadOnlyList<FolderNode> LoadRoots() =>
    [
        new("Projects", 0, true),
        new("Documents", 0, true),
        new("Pictures", 0, true)
    ];

    public static IReadOnlyList<FolderNode> LoadChildren(FolderNode folder)
    {
        var depth = folder.Depth + 1;
        return (folder.Depth == 0 ? FolderNames[folder.Name] : SubfolderNames)
            .Select(x => new FolderNode(x, depth, depth < LeafDepth))
            .ToArray();
    }
}
