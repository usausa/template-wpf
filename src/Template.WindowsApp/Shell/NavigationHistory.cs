namespace Template.WindowsApp.Shell;

using Template.WindowsApp.Views;

public sealed class NavigationHistory
{
    private const int Capacity = 50;

    private readonly List<ViewId> entries = [];

    private int index = -1;

    private int? moving;

    public bool CanGoBack => index > 0;

    public bool CanGoForward => index < entries.Count - 1;

    public ViewId? Begin(int offset)
    {
        var target = index + offset;
        if ((target < 0) || (target >= entries.Count))
        {
            return null;
        }

        moving = target;
        return entries[target];
    }

    public void End() => moving = null;

    public void Visit(ViewId id)
    {
        if ((moving is { } target) && (entries[target] == id))
        {
            index = target;
            return;
        }

        if ((index >= 0) && (entries[index] == id))
        {
            return;
        }

        entries.RemoveRange(index + 1, entries.Count - index - 1);
        entries.Add(id);
        if (entries.Count > Capacity)
        {
            entries.RemoveAt(0);
        }
        index = entries.Count - 1;
    }
}
