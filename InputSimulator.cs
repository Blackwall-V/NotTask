using System.Threading;

namespace AutoClicker
{
    internal enum MouseButtonKind
    {
        Left,
        Right,
        Middle
    }

    /// <summary>
    /// Simulates mouse and keyboard input using SendInput (the same API real
    /// input devices report through), combined with SetCursorPos so the OS
    /// cursor position always matches what the target window sees. This is
    /// the most compatible approach available without kernel drivers; games
    /// protected by kernel-level anti-cheat (EAC, BattlEye, Vanguard, etc.)
    /// intentionally ignore SendInput and are out of scope by design.
    /// </summary>
    internal static class InputSimulator
    {
        // Minimum time the button stays "down" before the "up" event, in ms.
        // Some games coalesce/ignore instantaneous down+up pairs.
        public const int ClickHoldMs = 12;
        public const int DoubleClickGapMs = 80;

        public static void MoveTo(int x, int y)
        {
            NativeMethods.SetCursorPos(x, y);
        }

        public static POINT GetCursorPos()
        {
            POINT p;
            NativeMethods.GetCursorPos(out p);
            return p;
        }

        public static void Click(MouseButtonKind button)
        {
            MouseDown(button);
            Thread.Sleep(ClickHoldMs);
            MouseUp(button);
        }

        public static void DoubleClick(MouseButtonKind button)
        {
            Click(button);
            Thread.Sleep(DoubleClickGapMs);
            Click(button);
        }

        public static void MouseDown(MouseButtonKind button)
        {
            SendMouse(DownFlag(button));
        }

        public static void MouseUp(MouseButtonKind button)
        {
            SendMouse(UpFlag(button));
        }

        private static uint DownFlag(MouseButtonKind button)
        {
            switch (button)
            {
                case MouseButtonKind.Right: return NativeMethods.MOUSEEVENTF_RIGHTDOWN;
                case MouseButtonKind.Middle: return NativeMethods.MOUSEEVENTF_MIDDLEDOWN;
                default: return NativeMethods.MOUSEEVENTF_LEFTDOWN;
            }
        }

        private static uint UpFlag(MouseButtonKind button)
        {
            switch (button)
            {
                case MouseButtonKind.Right: return NativeMethods.MOUSEEVENTF_RIGHTUP;
                case MouseButtonKind.Middle: return NativeMethods.MOUSEEVENTF_MIDDLEUP;
                default: return NativeMethods.MOUSEEVENTF_LEFTUP;
            }
        }

        private static void SendMouse(uint flags)
        {
            var input = new INPUT
            {
                type = NativeMethods.INPUT_MOUSE,
                U = new INPUTUNION
                {
                    mi = new MOUSEINPUT
                    {
                        dx = 0,
                        dy = 0,
                        mouseData = 0,
                        dwFlags = flags,
                        time = 0,
                        dwExtraInfo = System.IntPtr.Zero
                    }
                }
            };
            NativeMethods.SendInput(1, new[] { input }, System.Runtime.InteropServices.Marshal.SizeOf(typeof(INPUT)));
        }

        public static void KeyDown(ushort vk)
        {
            SendKey(vk, 0);
        }

        public static void KeyUp(ushort vk)
        {
            SendKey(vk, NativeMethods.KEYEVENTF_KEYUP);
        }

        private static void SendKey(ushort vk, uint flags)
        {
            ushort scan = (ushort)NativeMethods.MapVirtualKey(vk, NativeMethods.MAPVK_VK_TO_VSC);
            var input = new INPUT
            {
                type = NativeMethods.INPUT_KEYBOARD,
                U = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = vk,
                        wScan = scan,
                        dwFlags = flags,
                        time = 0,
                        dwExtraInfo = System.IntPtr.Zero
                    }
                }
            };
            NativeMethods.SendInput(1, new[] { input }, System.Runtime.InteropServices.Marshal.SizeOf(typeof(INPUT)));
        }
    }
}
