  using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ConKeyboard;

public sealed class KeyboardHook : IDisposable
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int VK_TAB = 0x09;
    private const int VK_SHIFT = 0x10;
    private const int VK_MENU = 0x12; // Alt

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
    [DllImport("user32.dll")]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);
    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
    [DllImport("kernel32.dll")]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    private readonly LowLevelKeyboardProc _proc;
    private IntPtr _hookId = IntPtr.Zero;

    public bool Enabled { get; set; }

    public KeyboardHook()
    {
        _proc = HookCallback;
        using var process = Process.GetCurrentProcess();
        using var module = process.MainModule;
        _hookId = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, GetModuleHandle(module.ModuleName), 0);
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && Enabled)
        {
            int vk = Marshal.ReadInt32(lParam);
            int msg = (int)wParam;

            bool tabHeld = IsDown(VK_TAB);
            bool altHeld = IsDown(VK_MENU);
            bool shiftHeld = IsDown(VK_SHIFT);

            // Comfort feature: swallow plain Tab so you can HOLD it.
            // Alt+Tab still works for switching windows!
            if (vk == VK_TAB && !altHeld)
                return (IntPtr)1;

            // ★★★ PRIMARY RULE: Hold TAB + key => print YOUR glyph ★★★
            if (tabHeld && (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN))
            {
                string text = KeyMapper.Map(vk, shiftHeld);
                if (text != null)
                {
                    InputSimulator.SendString(text);
                    return (IntPtr)1; // eat the English letter
                }
            }
        }
        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    private static bool IsDown(int vk) => (GetAsyncKeyState(vk) & 0x8000) != 0;

    public void Dispose()
    {
        if (_hookId != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
        }
    }
}
