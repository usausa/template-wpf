namespace Template.WindowsApp.Views;

using System.Windows.Controls;

using Template.WindowsApp.Models.Sample;

public sealed class OrderLineValidationRule : ValidationRule
{
    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        if ((value is BindingGroup group) && (group.Items.OfType<OrderLine>().FirstOrDefault() is { } line))
        {
            line.RevealErrors();
            if (line.HasErrors)
            {
                return new ValidationResult(false, line.ErrorText);
            }
        }

        return ValidationResult.ValidResult;
    }
}
