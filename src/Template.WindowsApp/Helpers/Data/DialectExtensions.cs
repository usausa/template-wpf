namespace Template.WindowsApp.Helpers.Data;

using Smart.Data;

public static class DialectExtensions
{
    public static string? Match(this IDialect dialect, string? keyword) =>
        String.IsNullOrWhiteSpace(keyword) ? null : $"%{dialect.LikeEscape(keyword.Trim())}%";
}
