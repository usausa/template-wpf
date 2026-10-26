namespace Template.WindowsApp.Settings;

public sealed class GridColumnLayout
{
    public string Key { get; init; } = string.Empty;

    public bool IsVisible { get; init; } = true;

    public double? Width { get; init; }
}
