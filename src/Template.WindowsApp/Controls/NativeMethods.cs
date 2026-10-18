namespace Template.WindowsApp.Controls;

using System.Runtime.InteropServices;

internal static partial class NativeMethods
{
    public const int WmGetMinMaxInfo = 0x0024;

    public const int WmNcHitTest = 0x0084;

    public const int WmNcMouseMove = 0x00A0;

    public const int WmNcLButtonDown = 0x00A1;

    public const int WmNcLButtonUp = 0x00A2;

    public const int WmNcLButtonDoubleClick = 0x00A3;

    public const int WmNcMouseLeave = 0x02A2;

    public const int HtMaxButton = 9;

    private const uint MonitorDefaultToNearest = 0x00000002;

    private const uint TrackMouseLeave = 0x00000002;

    private const uint TrackMouseNonClient = 0x00000010;

    [StructLayout(LayoutKind.Sequential)]
    private struct Point
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MinMaxInfo
    {
        public Point Reserved;
        public Point MaxSize;
        public Point MaxPosition;
        public Point MinTrackSize;
        public Point MaxTrackSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MonitorInfo
    {
        public int Size;
        public Rect Monitor;
        public Rect Work;
        public uint Flags;
    }

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
    private static partial IntPtr MonitorFromWindow(IntPtr hwnd, uint flags);

    [LibraryImport("user32.dll", EntryPoint = "GetMonitorInfoW")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetMonitorInfo(IntPtr monitor, ref MonitorInfo info);

    [LibraryImport("user32.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool TrackMouseEvent(ref TrackMouseEventInfo info);

    public static void FitMaximizedToWorkArea(IntPtr hwnd, IntPtr lParam)
    {
        var monitor = MonitorFromWindow(hwnd, MonitorDefaultToNearest);
        if (monitor == IntPtr.Zero)
        {
            return;
        }

        var info = new MonitorInfo { Size = Marshal.SizeOf<MonitorInfo>() };
        if (!GetMonitorInfo(monitor, ref info))
        {
            return;
        }

        var mmi = Marshal.PtrToStructure<MinMaxInfo>(lParam);
        mmi.MaxPosition.X = info.Work.Left - info.Monitor.Left;
        mmi.MaxPosition.Y = info.Work.Top - info.Monitor.Top;
        mmi.MaxSize.X = info.Work.Right - info.Work.Left;
        mmi.MaxSize.Y = info.Work.Bottom - info.Work.Top;
        Marshal.StructureToPtr(mmi, lParam, false);
    }

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
