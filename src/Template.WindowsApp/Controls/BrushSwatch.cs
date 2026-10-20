namespace Template.WindowsApp.Controls;

using System.Windows.Controls;

using BunnyTail.XamlProperty;

public sealed partial class BrushSwatch : Border
{
    [DependencyProperty(PropertyChanged = nameof(OnBrushKeyChanged))]
    public partial string? BrushKey { get; set; }

    private void OnBrushKeyChanged()
    {
        if (BrushKey is null)
        {
            ClearValue(BackgroundProperty);
        }
        else
        {
            SetResourceReference(BackgroundProperty, BrushKey);
        }
    }
}
