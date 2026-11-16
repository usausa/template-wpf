namespace Template.WindowsApp.Helpers;

using System.Runtime.InteropServices;

using Microsoft.Win32.SafeHandles;

internal static partial class NativeMethods
{
    public const int WmWindowPosChanging = 0x0046;

    public const int WmWindowPosChanged = 0x0047;

    public const int WmNcHitTest = 0x0084;

    public const int WmNcMouseMove = 0x00A0;

    public const int WmNcLButtonDown = 0x00A1;

    public const int WmNcLButtonUp = 0x00A2;

    public const int WmNcLButtonDoubleClick = 0x00A3;

    public const int WmSysCommand = 0x0112;

    public const int WmInitMenuPopup = 0x0117;

    public const int WmNcMouseLeave = 0x02A2;

    public const int HtMaxButton = 9;

    public const int HtBorder = 18;

    public const int ScSize = 0xF000;

    public const int ScMove = 0xF010;

    public const int ScMaximize = 0xF030;

    public const int ScClose = 0xF060;

    public const int ScRestore = 0xF120;

    private const int ScCommandMask = 0xFFF0;

    private const uint TrackMouseLeave = 0x00000002;

    private const uint TrackMouseNonClient = 0x00000010;

    private const uint MenuGrayed = 0x00000001;

    private const uint SwpNoSize = 0x0001;

    private const uint SwpNoMove = 0x0002;

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct TrackMouseEventInfo
    {
        public int Size;
        public uint Flags;
        public IntPtr Track;
        public uint HoverTime;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowPos
    {
        public IntPtr Hwnd;
        public IntPtr HwndInsertAfter;
        public int X;
        public int Y;
        public int Cx;
        public int Cy;
        public uint Flags;
    }

    [LibraryImport("user32.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool TrackMouseEvent(ref TrackMouseEventInfo info);

    [LibraryImport("user32.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static partial IntPtr GetSystemMenu(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool revert);

    [LibraryImport("user32.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static partial int EnableMenuItem(IntPtr menu, uint item, uint enable);

    [LibraryImport("user32.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool IsIconic(IntPtr hwnd);

    [LibraryImport("user32.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool IsZoomed(IntPtr hwnd);

    [LibraryImport("user32.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetWindowRect(IntPtr hwnd, out Rect rect);

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

    public static int ToSystemCommand(IntPtr wParam) => (int)(wParam.ToInt64() & ScCommandMask);

    public static bool IsSystemMenu(IntPtr hwnd, IntPtr menu) => menu == GetSystemMenu(hwnd, false);

    public static void GraySystemMenu(IntPtr hwnd, bool maximized)
    {
        var menu = GetSystemMenu(hwnd, false);
        if (menu == IntPtr.Zero)
        {
            return;
        }

        _ = EnableMenuItem(menu, ScMove, MenuGrayed);
        _ = EnableMenuItem(menu, ScSize, MenuGrayed);
        _ = EnableMenuItem(menu, ScMaximize, MenuGrayed);
        _ = EnableMenuItem(menu, ScClose, MenuGrayed);
        if (maximized)
        {
            _ = EnableMenuItem(menu, ScRestore, MenuGrayed);
        }
    }

    public static void ResetSystemMenu(IntPtr hwnd) => _ = GetSystemMenu(hwnd, true);

    public static void KeepWindowPos(IntPtr lParam, Rect bounds)
    {
        var pos = Marshal.PtrToStructure<WindowPos>(lParam);
        if ((pos.Flags & SwpNoMove) == 0)
        {
            pos.X = bounds.Left;
            pos.Y = bounds.Top;
        }
        if ((pos.Flags & SwpNoSize) == 0)
        {
            pos.Cx = bounds.Right - bounds.Left;
            pos.Cy = bounds.Bottom - bounds.Top;
        }
        Marshal.StructureToPtr(pos, lParam, false);
    }
}
