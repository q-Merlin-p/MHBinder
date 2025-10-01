using System;
using System.Reflection;
using System.Runtime.InteropServices;
using ClickableTransparentOverlay;

namespace MHbinder;

public static class WindowUtils
{
    private const int GWL_EXSTYLE = -20;
    private const uint WS_EX_TOOLWINDOW = 0x00000080;
    private const uint WS_EX_APPWINDOW = 0x00040000;

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern nint FindWindowW(string? lpClassName, string? lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(nint hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern nint GetWindowLongPtr64(nint hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
    private static extern int GetWindowLong32(nint hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    private static extern nint SetWindowLongPtr64(nint hWnd, int nIndex, nint dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongW")]
    private static extern int SetWindowLong32(nint hWnd, int nIndex, int dwNewLong);

    private static nint GetWindowLongPtr(nint hWnd, int nIndex) =>
        IntPtr.Size == 8 ? GetWindowLongPtr64(hWnd, nIndex) : (nint)GetWindowLong32(hWnd, nIndex);

    private static nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong) =>
        IntPtr.Size == 8 ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong) : (nint)SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32());
    public static void HideFromAltTab(nint hwnd)
    {
        if (hwnd == 0) return;
        ulong ex = (ulong)GetWindowLongPtr(hwnd, GWL_EXSTYLE);
        ex &= ~WS_EX_APPWINDOW;
        ex |= WS_EX_TOOLWINDOW;
        SetWindowLongPtr(hwnd, GWL_EXSTYLE, (nint)ex);
    }
    public static nint TryGetOverlayHwnd(Overlay overlay, string? windowTitleHint = null)
    {
        var t = overlay.GetType();

        foreach (var name in new[] { "Hwnd", "HWND", "Handle", "WindowHandle" })
        {
            var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null && (p.PropertyType == typeof(nint) || p.PropertyType == typeof(IntPtr)))
            {
                var val = p.GetValue(overlay);
                if (val is nint ni) return ni;
                if (val is IntPtr ip) return ip;
            }
        }

        foreach (var name in new[] { "_hWnd", "hWnd", "_hwnd", "hwnd", "_windowHandle", "windowHandle" })
        {
            var f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null && (f.FieldType == typeof(nint) || f.FieldType == typeof(IntPtr)))
            {
                var val = f.GetValue(overlay);
                if (val is nint ni) return ni;
                if (val is IntPtr ip) return ip;
            }
        }

        if (!string.IsNullOrWhiteSpace(windowTitleHint))
        {
            var hwnd = FindWindowW(null, windowTitleHint);
            if (hwnd != 0)
            {
                GetWindowThreadProcessId(hwnd, out var pid);
                if (pid == (uint)Environment.ProcessId) return hwnd;
            }
        }

        return 0;
    }
}
