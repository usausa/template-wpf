namespace Template.WindowsApp.Converters;

using Template.WindowsApp.Models.Sample;

public sealed class ServerGroupSummaryConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not IEnumerable items)
        {
            return null;
        }

        var servers = items.OfType<ServerRow>().ToList();
        var cpu = servers.Where(static x => x.Cpu is not null).Select(static x => x.Cpu!.Value).ToList();
        var average = cpu.Count > 0 ? String.Create(culture, $"{cpu.Average():0}%") : "-";
        return String.Create(culture, $"Servers {servers.Count} · CPU average {average} · Errors {servers.Sum(static x => x.Errors ?? 0)}");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
