namespace Template.WindowsApp.Shell;

public interface IShellCommandProvider
{
    IEnumerable<ShellCommand> GetCommands();
}
