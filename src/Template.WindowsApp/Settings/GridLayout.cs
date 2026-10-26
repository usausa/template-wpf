namespace Template.WindowsApp.Settings;

public sealed class GridLayout
{
    public int FrozenColumnCount { get; init; }

    public IReadOnlyList<GridColumnLayout> Columns { get; init; } = [];
}
