namespace Template.WindowsApp.Controls;

using System.Windows.Automation;
using System.Windows.Automation.Peers;

public sealed class ContentDialogAutomationPeer : FrameworkElementAutomationPeer
{
    public ContentDialogAutomationPeer(ContentDialog owner)
        : base(owner)
    {
    }

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Pane;

    protected override string GetClassNameCore() => nameof(ContentDialog);

    protected override string GetNameCore()
    {
        var name = AutomationProperties.GetName(Owner);
        return String.IsNullOrEmpty(name) ? ((ContentDialog)Owner).Title ?? string.Empty : name;
    }

    protected override bool IsDialogCore() => true;
}
