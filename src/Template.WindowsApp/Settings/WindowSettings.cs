namespace Template.WindowsApp.Settings;

using System.Configuration;

public sealed class MainWindowPlacement
{
    public int Left { get; set; }

    public int Top { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public bool Maximized { get; set; }
}

public sealed class WindowSettings : ApplicationSettingsBase
{
    [UserScopedSetting]
    public MainWindowPlacement? MainWindowPlacement
    {
        get => (MainWindowPlacement)this[nameof(MainWindowPlacement)];
        set => this[nameof(MainWindowPlacement)] = value;
    }

    [UserScopedSetting]
    [DefaultSettingValue("True")]
    public bool IsPaneOpen
    {
        get => (bool)this[nameof(IsPaneOpen)];
        set => this[nameof(IsPaneOpen)] = value;
    }

    [UserScopedSetting]
    public string? RecentCommands
    {
        get => (string?)this[nameof(RecentCommands)];
        set => this[nameof(RecentCommands)] = value;
    }

    [UserScopedSetting]
    public string? CommandCounts
    {
        get => (string?)this[nameof(CommandCounts)];
        set => this[nameof(CommandCounts)] = value;
    }

    [UserScopedSetting]
    public string? RecentViews
    {
        get => (string?)this[nameof(RecentViews)];
        set => this[nameof(RecentViews)] = value;
    }

    [UserScopedSetting]
    public string? FavoriteViews
    {
        get => (string?)this[nameof(FavoriteViews)];
        set => this[nameof(FavoriteViews)] = value;
    }

    [UserScopedSetting]
    public string? GridLayouts
    {
        get => (string?)this[nameof(GridLayouts)];
        set => this[nameof(GridLayouts)] = value;
    }

    [UserScopedSetting]
    [DefaultSettingValue("Light")]
    public string Theme
    {
        get => (string)this[nameof(Theme)];
        set => this[nameof(Theme)] = value;
    }

    [UserScopedSetting]
    [DefaultSettingValue("Home")]
    public string StartPage
    {
        get => (string)this[nameof(StartPage)];
        set => this[nameof(StartPage)] = value;
    }

    [UserScopedSetting]
    public string? LastView
    {
        get => (string?)this[nameof(LastView)];
        set => this[nameof(LastView)] = value;
    }

    [UserScopedSetting]
    [DefaultSettingValue("True")]
    public bool ShowsPopups
    {
        get => (bool)this[nameof(ShowsPopups)];
        set => this[nameof(ShowsPopups)] = value;
    }

    [UserScopedSetting]
    [DefaultSettingValue("True")]
    public bool ShowsInformational
    {
        get => (bool)this[nameof(ShowsInformational)];
        set => this[nameof(ShowsInformational)] = value;
    }

    [UserScopedSetting]
    [DefaultSettingValue("True")]
    public bool ShowsSuccess
    {
        get => (bool)this[nameof(ShowsSuccess)];
        set => this[nameof(ShowsSuccess)] = value;
    }

    [UserScopedSetting]
    [DefaultSettingValue("True")]
    public bool ShowsWarning
    {
        get => (bool)this[nameof(ShowsWarning)];
        set => this[nameof(ShowsWarning)] = value;
    }

    [UserScopedSetting]
    [DefaultSettingValue("True")]
    public bool ShowsError
    {
        get => (bool)this[nameof(ShowsError)];
        set => this[nameof(ShowsError)] = value;
    }

    [UserScopedSetting]
    [DefaultSettingValue("True")]
    public bool UpgradeRequired
    {
        get => (bool)this[nameof(UpgradeRequired)];
        set => this[nameof(UpgradeRequired)] = value;
    }

    [UserScopedSetting]
    public string? LastVersion
    {
        get => (string?)this[nameof(LastVersion)];
        set => this[nameof(LastVersion)] = value;
    }
}
