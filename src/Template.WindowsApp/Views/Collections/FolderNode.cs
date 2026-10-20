namespace Template.WindowsApp.Views.Collections;

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
