namespace Template.WindowsApp.Views;

using System.Reflection;

using Smart.Mvvm.Resolver;

internal static class ValidationHelper
{
    [ThreadStatic]
    private static List<ValidationResult>? validationResults;

    public static void Validate(object target, string name, ErrorInfo errors)
    {
        var context = MakeContext(target, name);
        validationResults ??= [];

        if (!Validator.TryValidateProperty(FindProperty(target, name).GetValue(target), context, validationResults))
        {
            errors.AddError(name, validationResults[0].ErrorMessage!);
        }

        validationResults.Clear();
    }

    public static bool IsValid(object target, string name) =>
        Validator.TryValidateProperty(FindProperty(target, name).GetValue(target), MakeContext(target, name), null);

    private static ValidationContext MakeContext(object target, string name) =>
        new(target, ResolveProvider.Default, null)
        {
            MemberName = name
        };

    private static PropertyInfo FindProperty(object target, string name) =>
        target.GetType().GetProperty(name) ?? throw new InvalidOperationException($"Property is not found. type=[{target.GetType()}], name=[{name}]");
}
