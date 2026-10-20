namespace Template.WindowsApp.Views.Forms;

using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class ValidationViewModel : AppViewModelBase, IShellCommandProvider
{
    [Required(ErrorMessage = "Required")]
    [ObservableProperty]
    public partial string Text1 { get; set; }

    [ObservableProperty]
    public partial string Text2 { get; set; }

    [ObservableProperty]
    public partial string Email { get; set; }

    [Compare(nameof(Email), ErrorMessage = "Does not match Email")]
    [ObservableProperty]
    public partial string ConfirmEmail { get; set; }

    [ObservableProperty]
    public partial string RuleEmail { get; set; }

    [ObservableProperty]
    public partial string Quantity { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [ObservableProperty]
    public partial string Name { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [ObservableProperty]
    public partial string FormEmail { get; set; }

    [Range(18, 120, ErrorMessage = "Enter a number from 18 to 120.")]
    [ObservableProperty]
    public partial string Age { get; set; }

    [ObservableProperty]
    public partial string? LastSubmitted { get; set; }

    public EventRequest FocusRequest { get; } = new();

    public EventRequest ClearRulesRequest { get; } = new();

    public IObserveCommand ErrorCommand { get; }

    public IObserveCommand ClearCommand { get; }

    public IObserveCommand FocusCommand { get; }

    public IObserveCommand SubmitCommand { get; }

    public IObserveCommand ResetCommand { get; }

    public ValidationViewModel()
    {
        Text1 = string.Empty;
        Text2 = string.Empty;
        Email = string.Empty;
        ConfirmEmail = string.Empty;
        RuleEmail = string.Empty;
        Quantity = string.Empty;
        Name = string.Empty;
        FormEmail = string.Empty;
        Age = string.Empty;

        SubscribeEmail(_ =>
        {
            if (!String.IsNullOrEmpty(ConfirmEmail))
            {
                Revalidate(nameof(ConfirmEmail));
            }
        });
        SubscribeConfirmEmail(_ => Revalidate(nameof(ConfirmEmail)));
        SubscribeName(_ => Revalidate(nameof(Name)));
        SubscribeFormEmail(_ => Revalidate(nameof(FormEmail)));
        SubscribeAge(_ => Revalidate(nameof(Age)));

        ErrorCommand = MakeDelegateCommand(() => Errors.AddError(nameof(Text2), "Manual error"));
        ClearCommand = MakeDelegateCommand(() => Errors.ClearErrors(nameof(Text2)));
        FocusCommand = MakeDelegateCommand(FocusRequest.Request);
        SubmitCommand = MakeDelegateCommand(Submit, CanSubmit);
        ResetCommand = MakeDelegateCommand(Reset);
    }

    private void Revalidate(string name)
    {
        Errors.ClearErrors(name);
        Validate(name);
    }

    private bool CanSubmit() =>
        ValidationHelper.IsValid(this, nameof(Name)) &&
        ValidationHelper.IsValid(this, nameof(FormEmail)) &&
        ValidationHelper.IsValid(this, nameof(Age));

    private void Submit()
    {
        LastSubmitted = Name;
        ClearForm();
    }

    private void ClearForm()
    {
        Name = string.Empty;
        FormEmail = string.Empty;
        Age = string.Empty;
        Errors.ClearErrors(nameof(Name));
        Errors.ClearErrors(nameof(FormEmail));
        Errors.ClearErrors(nameof(Age));
    }

    private void Reset()
    {
        Text1 = string.Empty;
        Text2 = string.Empty;
        Email = string.Empty;
        ConfirmEmail = string.Empty;
        ClearForm();
        LastSubmitted = null;
        Errors.ClearAllErrors();
        ClearRulesRequest.Request();
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("Validation:Error", "Error", "Validation", "\uE783", ErrorCommand),
        new("Validation:Clear", "Clear", "Validation", "\uE894", ClearCommand),
        new("Validation:Focus", "Focus", "Validation", "\uE8AC", FocusCommand),
        new("Validation:Submit", "Submit", "Validation", "\uE724", SubmitCommand),
        new("Validation:Reset", "Reset", "Validation", "\uE72C", ResetCommand)
    ];
}
