namespace Template.WindowsApp.Views.Platform;

public sealed record EnvironmentSection(string Title, IReadOnlyList<EnvironmentEntry> Entries);
