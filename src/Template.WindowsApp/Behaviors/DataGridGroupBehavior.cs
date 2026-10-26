namespace Template.WindowsApp.Behaviors;

using System.Windows.Controls;

using BunnyTail.XamlProperty;

using Microsoft.Xaml.Behaviors;

public sealed partial class DataGridGroupBehavior : Behavior<DataGrid>
{
    [DependencyProperty(PropertyChanged = nameof(OnPropertyNameChanged))]
    public partial string? PropertyName { get; set; }

    protected override void OnAttached()
    {
        base.OnAttached();
        Apply();
    }

    private void OnPropertyNameChanged() => Apply();

    private void Apply()
    {
        if (AssociatedObject?.Items.GroupDescriptions is not { } groups)
        {
            return;
        }

        groups.Clear();
        if (!String.IsNullOrEmpty(PropertyName))
        {
            groups.Add(new PropertyGroupDescription(PropertyName));
        }
    }
}
