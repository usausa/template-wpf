namespace Template.WindowsApp.Models;

public sealed record EnvironmentEntry(string Name, string Value, bool IsCode = false);
