namespace Template.WindowsApp.Views.Settings;

using System.Runtime.InteropServices;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class SettingsViewModel : AppViewModelBase
{
    public string ApplicationName { get; }

    public string VersionText { get; }

    public SettingsViewModel()
    {
        var name = typeof(SettingsViewModel).Assembly.GetName();
        ApplicationName = name.Name ?? string.Empty;
        VersionText = $"Version {name.Version?.ToString(3)} · {RuntimeInformation.FrameworkDescription}";
    }
}
