namespace Template.WindowsApp.Views.Fundamentals;

using BunnyTail.XamlProperty;

public sealed partial class ProfileTile
{
    [DependencyProperty]
    public partial string? DisplayName { get; set; }

    [DependencyProperty]
    public partial string? Role { get; set; }

    [DependencyProperty]
    public partial string? Glyph { get; set; }

    public ProfileTile()
    {
        InitializeComponent();
    }
}
