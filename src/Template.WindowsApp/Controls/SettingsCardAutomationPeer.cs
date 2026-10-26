namespace Template.WindowsApp.Controls;

using System.Windows.Automation;
using System.Windows.Automation.Peers;

public sealed class SettingsCardAutomationPeer : FrameworkElementAutomationPeer
{
    public SettingsCardAutomationPeer(SettingsCard owner)
        : base(owner)
    {
    }

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Group;

    protected override string GetClassNameCore() => nameof(SettingsCard);

    protected override string GetNameCore()
    {
        var name = AutomationProperties.GetName(Owner);
        return String.IsNullOrEmpty(name) ? ((SettingsCard)Owner).Header ?? string.Empty : name;
    }

    protected override string GetHelpTextCore()
    {
        var text = base.GetHelpTextCore();
        return String.IsNullOrEmpty(text) ? ((SettingsCard)Owner).Description ?? string.Empty : text;
    }
}
