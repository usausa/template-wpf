namespace Template.WindowsApp.Helpers;

using System.Runtime.InteropServices;

using Microsoft.Win32.SafeHandles;

internal static partial class NativeMethods
{
    public const int WmNcHitTest = 0x0084;

    public const int WmNcMouseMove = 0x00A0;

    public const int WmNcLButtonDown = 0x00A1;

    public const int WmNcLButtonUp = 0x00A2;

    public const int WmNcLButtonDoubleClick = 0x00A3;

    public const int WmNcMouseLeave = 0x02A2;

    public const int HtMaxButton = 9;

    private const uint TrackMouseLeave = 0x00000002;

    private const uint TrackMouseNonClient = 0x00000010;

    [StructLayout(LayoutKind.Sequential)]
    private struct TrackMouseEventInfo
    {
        public int Size;
        public uint Flags;
        public IntPtr Track;
        public uint HoverTime;
    }

    [LibraryImport("user32.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool TrackMouseEvent(ref TrackMouseEventInfo info);

    [LibraryImport("user32.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static partial uint GetDpiForSystem();

    [LibraryImport("user32.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool AllowSetForegroundWindow(uint processId);

    [LibraryImport("kernel32.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetNamedPipeServerProcessId(SafePipeHandle pipe, out uint serverProcessId);

    public static void TrackNonClientMouseLeave(IntPtr hwnd)
    {
        var info = new TrackMouseEventInfo
        {
            Size = Marshal.SizeOf<TrackMouseEventInfo>(),
            Flags = TrackMouseLeave | TrackMouseNonClient,
            Track = hwnd
        };
        _ = TrackMouseEvent(ref info);
    }
}
