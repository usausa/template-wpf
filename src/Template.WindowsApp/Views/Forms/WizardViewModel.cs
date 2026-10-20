namespace Template.WindowsApp.Views.Forms;

using Template.WindowsApp.Shell;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed partial class WizardViewModel : AppViewModelBase, IShellCommandProvider
{
    public IReadOnlyList<WizardStepItem> Steps { get; } =
    [
        new(WizardStep.Plan, 1),
        new(WizardStep.Details, 2),
        new(WizardStep.Confirm, 3)
    ];

    [ObservableProperty]
    public partial WizardStep Step { get; set; }

    [ObservableProperty]
    public partial WizardPlan Plan { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [ObservableProperty]
    public partial string Name { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [ObservableProperty]
    public partial string Email { get; set; }

    [ObservableProperty]
    public partial string Company { get; set; }

    [ObservableProperty]
    public partial string? SubmittedName { get; set; }

    [ObservableProperty]
    public partial WizardPlan SubmittedPlan { get; set; }

    public IObserveCommand BackCommand { get; }

    public IObserveCommand NextCommand { get; }

    public IObserveCommand FinishCommand { get; }

    public IObserveCommand ResetCommand { get; }

    public WizardViewModel()
    {
        Plan = WizardPlan.Standard;
        Name = string.Empty;
        Email = string.Empty;
        Company = string.Empty;
        UpdateSteps();

        SubscribeStep(_ => UpdateSteps());
        SubscribeName(_ => Revalidate(nameof(Name)));
        SubscribeEmail(_ => Revalidate(nameof(Email)));

        BackCommand = MakeDelegateCommand(() => Step--, () => Step != WizardStep.Plan);
        NextCommand = MakeDelegateCommand(() => Step++, CanNext);
        FinishCommand = MakeDelegateCommand(Finish, () => Step == WizardStep.Confirm);
        ResetCommand = MakeDelegateCommand(Reset);
    }

    private void UpdateSteps()
    {
        foreach (var item in Steps)
        {
            item.State = item.Step < Step ? WizardStepState.Done : item.Step == Step ? WizardStepState.Current : WizardStepState.Upcoming;
        }
    }

    private void Revalidate(string name)
    {
        Errors.ClearErrors(name);
        Validate(name);
    }

    private bool CanNext() =>
        Step switch
        {
            WizardStep.Plan => true,
            WizardStep.Details => ValidationHelper.IsValid(this, nameof(Name)) && ValidationHelper.IsValid(this, nameof(Email)),
            _ => false
        };

    private void Finish()
    {
        SubmittedName = Name;
        SubmittedPlan = Plan;
        ClearInput();
    }

    private void ClearInput()
    {
        Plan = WizardPlan.Standard;
        Name = string.Empty;
        Email = string.Empty;
        Company = string.Empty;
        Errors.ClearErrors(nameof(Name));
        Errors.ClearErrors(nameof(Email));
        Step = WizardStep.Plan;
    }

    private void Reset()
    {
        ClearInput();
        SubmittedName = null;
    }

    public IEnumerable<ShellCommand> GetCommands() =>
    [
        new("Wizard:Back", "Back", "Wizard", "\uE72B", BackCommand),
        new("Wizard:Next", "Next", "Wizard", "\uE72A", NextCommand),
        new("Wizard:Finish", "Finish", "Wizard", "\uE73E", FinishCommand),
        new("Wizard:Reset", "Reset", "Wizard", "\uE72C", ResetCommand)
    ];
}
