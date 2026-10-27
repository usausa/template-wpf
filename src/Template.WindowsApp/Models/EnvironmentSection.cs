namespace Template.WindowsApp.Models;

public sealed record EnvironmentSection(string Title, IReadOnlyList<EnvironmentEntry> Entries);
