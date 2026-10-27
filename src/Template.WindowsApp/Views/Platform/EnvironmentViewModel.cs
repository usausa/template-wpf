namespace Template.WindowsApp.Views.Platform;

using System.Configuration;
using System.Runtime.InteropServices;

using Microsoft.Extensions.Hosting;

using Template.WindowsApp.Components;
using Template.WindowsApp.Models;
using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class EnvironmentViewModel : AppViewModelBase, IShellCommandProvider
{
    private const double DefaultDpi = 96;

    private const double SampleNumber = 1234567.89;

    private const decimal SampleAmount = 1234.5m;

    public EnvironmentSection SystemSection { get; }

    public EnvironmentSection RuntimeSection { get; }

    public EnvironmentSection DisplaySection { get; }

    public EnvironmentSection CultureSection { get; }

    public EnvironmentSection PathSection { get; }

    [ObservableProperty]
    public partial string? Copied { get; set; }

    public IObserveCommand CopyAllCommand { get; }

    public IObserveCommand OpenCommand { get; }

    public EnvironmentViewModel(
        TimeProvider timeProvider,
        IHostEnvironment hostEnvironment,
        IClipboard clipboard,
        IExplorerLauncher explorerLauncher)
    {
        SystemSection = new EnvironmentSection(
            "Operating system",
            [
                new("Name", RuntimeInformation.OSDescription),
                new("Version", Environment.OSVersion.Version.ToString()),
                new("Architecture", RuntimeInformation.OSArchitecture.ToString()),
                new("Processors", Environment.ProcessorCount.ToString(CultureInfo.CurrentCulture))
            ]);

        RuntimeSection = new EnvironmentSection(
            "Runtime",
            [
                new(".NET", RuntimeInformation.FrameworkDescription),
                new("Runtime ID", RuntimeInformation.RuntimeIdentifier, true),
                new("Process", $"{RuntimeInformation.ProcessArchitecture} · {(Environment.Is64BitProcess ? "64-bit" : "32-bit")}"),
                new("Environment", hostEnvironment.EnvironmentName)
            ]);

        var scale = NativeMethods.GetDpiForSystem() / DefaultDpi;
        var workArea = SystemParameters.WorkArea;
        DisplaySection = new EnvironmentSection(
            "Display",
            [
                new("Primary screen", FormatPixels(SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight, scale)),
                new("Work area", FormatPixels(workArea.Width, workArea.Height, scale)),
                new("Scale", scale.ToString("P0", CultureInfo.CurrentCulture)),
                new("High contrast", SystemParameters.HighContrast ? "On" : "Off")
            ]);

        var culture = CultureInfo.CurrentCulture;
        var uiCulture = CultureInfo.CurrentUICulture;
        CultureSection = new EnvironmentSection(
            "Culture",
            [
                new("Culture", $"{culture.Name} · {culture.DisplayName}"),
                new("UI culture", $"{uiCulture.Name} · {uiCulture.DisplayName}"),
                new("Date", timeProvider.GetLocalNow().ToString("D", culture)),
                new("Number", SampleNumber.ToString("N2", culture)),
                new("Currency", SampleAmount.ToString("C", culture)),
                new("Time zone", timeProvider.LocalTimeZone.DisplayName)
            ]);

        PathSection = new EnvironmentSection(
            "Paths",
            [
                new("Application", AppContext.BaseDirectory, true),
                new("Settings", ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath, true),
                new("Local app data", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), true),
                new("Temp", Path.GetTempPath(), true)
            ]);

        CopyAllCommand = MakeDelegateCommand(() =>
        {
            EnvironmentSection[] sections = [SystemSection, RuntimeSection, DisplaySection, CultureSection, PathSection];
            clipboard.SetText(MakeText(sections));
            Copied = $"{sections.Sum(static x => x.Entries.Count)} values";
        });
        OpenCommand = MakeDelegateCommand<string>(explorerLauncher.Open);
    }

    private static string FormatPixels(double width, double height, double scale) =>
        String.Create(CultureInfo.CurrentCulture, $"{width * scale:0} × {height * scale:0}");

    private static string MakeText(IEnumerable<EnvironmentSection> sections)
    {
        var builder = new StringBuilder();
        foreach (var section in sections)
        {
            builder.AppendLine(section.Title);
            foreach (var entry in section.Entries)
            {
                builder.AppendLine(CultureInfo.InvariantCulture, $"{entry.Name}: {entry.Value}");
            }
            builder.AppendLine();
        }
        return builder.ToString().TrimEnd();
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("Environment:CopyAll", "Copy all", "Environment", "\uE8C8", CopyAllCommand)
    ];
}
