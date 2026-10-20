namespace Template.WindowsApp.Controls;

using System.Windows.Automation.Peers;

public sealed class InfoBarAutomationPeer : FrameworkElementAutomationPeer
{
    public InfoBarAutomationPeer(InfoBar owner)
        : base(owner)
    {
    }

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.StatusBar;

    protected override string GetClassNameCore() => nameof(InfoBar);

    protected override string GetNameCore()
    {
        var name = base.GetNameCore();
        return String.IsNullOrEmpty(name) ? ((InfoBar)Owner).Title ?? string.Empty : name;
    }

    protected override string GetHelpTextCore()
    {
        var text = base.GetHelpTextCore();
        return String.IsNullOrEmpty(text) ? ((InfoBar)Owner).Message ?? string.Empty : text;
    }
}
