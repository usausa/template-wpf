namespace Template.WindowsApp.Components;

using System.Diagnostics;

public sealed class ExplorerLauncher : IExplorerLauncher
{
    private const string ExplorerFileName = "explorer.exe";

    public void Open(string path)
    {
        if (File.Exists(path))
        {
            Start($"/select,\"{path}\"");
            return;
        }

        var folder = path;
        while (!String.IsNullOrEmpty(folder) && !Directory.Exists(folder))
        {
            folder = Path.GetDirectoryName(folder);
        }
        if (!String.IsNullOrEmpty(folder))
        {
            Start($"\"{folder}\"");
        }
    }

    private static void Start(string arguments)
    {
        using var process = Process.Start(ExplorerFileName, arguments);
    }
}
