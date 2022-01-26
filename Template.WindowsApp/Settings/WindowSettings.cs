namespace Template.WindowsApp.Settings;

using System.Configuration;

public class MainWindowPlacement
{
    public int Left { get; set; }

    public int Top { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public bool Maximized { get; set; }
}

public class WindowSettings : ApplicationSettingsBase
{
    [UserScopedSetting]
    public MainWindowPlacement? MainWindowPlacement
    {
        get => (MainWindowPlacement)this[nameof(MainWindowPlacement)];
        set => this[nameof(MainWindowPlacement)] = value;
    }
}
