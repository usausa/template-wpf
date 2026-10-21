namespace Template.WindowsApp;

public static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        using var instance = new SingleInstance(typeof(Program).Assembly.GetName().Name!);
        if (!instance.IsFirst)
        {
            instance.Send(args);
            return 0;
        }

        var app = new App
        {
            SingleInstance = instance
        };
        return app.Run();
    }
}
