namespace Template.WindowsApp.Views.Collections;

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
