namespace Template.WindowsApp.Helpers;

using System.Windows.Controls;

using ValidationResult = System.Windows.Controls.ValidationResult;

public sealed class RangeValidationRule : ValidationRule
{
    public int Minimum { get; set; }

    public int Maximum { get; set; }

    public string? ErrorMessage { get; set; }

    public override ValidationResult Validate(object? value, CultureInfo cultureInfo) =>
        (value is string { Length: > 0 } text) && (!Int32.TryParse(text, NumberStyles.Integer, cultureInfo, out var number) || (number < Minimum) || (number > Maximum))
            ? new ValidationResult(false, ErrorMessage)
            : ValidationResult.ValidResult;
}
