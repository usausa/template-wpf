namespace Template.WindowsApp.Views.Forms;

public static class OrderLineCsv
{
    private static readonly string[] Headers = ["Item code", "Description", "Quantity", "Unit price", "Discount (%)", "Amount", "Delivery", "Status", "Note"];

    private static readonly char[] QuotedCharacters = [',', '"', '\r', '\n'];

    public static string Format(IEnumerable<OrderLineValues> lines)
    {
        var builder = new StringBuilder();
        AppendRow(builder, Headers);
        foreach (var line in lines)
        {
            AppendRow(builder,
            [
                line.Code,
                line.Description,
                Number(line.Quantity),
                Number(line.UnitPrice),
                Number(line.Discount),
                Number(line.Quantity * line.UnitPrice * (100 - line.Discount) / 100 is { } amount ? Math.Round(amount, 2) : null),
                line.Delivery?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty,
                line.Status.ToString(),
                line.Note
            ]);
        }
        return builder.ToString();
    }

    private static string Number(int? value) => value?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;

    private static string Number(decimal? value) => value?.ToString("0.00", CultureInfo.InvariantCulture) ?? string.Empty;

    private static void AppendRow(StringBuilder builder, string[] fields)
    {
        for (var i = 0; i < fields.Length; i++)
        {
            if (i > 0)
            {
                builder.Append(',');
            }

            var field = fields[i];
            if (field.IndexOfAny(QuotedCharacters) >= 0)
            {
                builder.Append('"').Append(field.Replace("\"", "\"\"", StringComparison.Ordinal)).Append('"');
            }
            else
            {
                builder.Append(field);
            }
        }
        builder.Append("\r\n");
    }
}
