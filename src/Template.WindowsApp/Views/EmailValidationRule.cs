namespace Template.WindowsApp.Views;

using System.Windows.Controls;

using ValidationResult = System.Windows.Controls.ValidationResult;

public sealed class EmailValidationRule : ValidationRule
{
    private static readonly EmailAddressAttribute Attribute = new();

    public string? ErrorMessage { get; set; }

    public override ValidationResult Validate(object? value, CultureInfo cultureInfo) =>
        (value is string { Length: > 0 } text) && !Attribute.IsValid(text) ? new ValidationResult(false, ErrorMessage) : ValidationResult.ValidResult;
}
