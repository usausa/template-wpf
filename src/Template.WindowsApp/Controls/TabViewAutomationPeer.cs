namespace Template.WindowsApp.Controls;

using System.Windows.Automation.Peers;

public sealed class TabViewAutomationPeer : TabControlAutomationPeer
{
    public TabViewAutomationPeer(TabView owner)
        : base(owner)
    {
    }

    protected override string GetClassNameCore() => nameof(TabView);

    protected override List<AutomationPeer>? GetChildrenCore()
    {
        var children = base.GetChildrenCore();
        if ((((TabView)Owner).AddTabButton is { IsVisible: true } button) && (UIElementAutomationPeer.CreatePeerForElement(button) is { } peer))
        {
            children ??= [];
            children.Add(peer);
        }
        return children;
    }
}
