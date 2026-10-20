namespace Template.WindowsApp.Views.Forms;

public sealed partial class WizardStepItem : ObservableObject
{
    public WizardStep Step { get; }

    public int Number { get; }

    [ObservableProperty]
    public partial WizardStepState State { get; set; }

    public WizardStepItem(WizardStep step, int number)
    {
        Step = step;
        Number = number;
    }
}
