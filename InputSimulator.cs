using System;
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
            Click(button, ClickHoldMs);
        }

        public static void Click(MouseButtonKind button, int holdMs)
        {
            MouseDown(button);
            Thread.Sleep(holdMs);
            MouseUp(button);
        }

        public static void DoubleClick(MouseButtonKind button)
        {
            DoubleClick(button, ClickHoldMs, DoubleClickGapMs);
        }

        public static void DoubleClick(MouseButtonKind button, int holdMs, int gapMs)
        {
            Click(button, holdMs);
            Thread.Sleep(gapMs);
            Click(button, holdMs);
        }

        /// <summary>
        /// Sends the click directly to a specific window handle via PostMessage,
        /// bypassing screen z-order entirely. Unlike SendInput+SetCursorPos, this
        /// still reaches the target even if another window (including this app,
        /// when "mantener encima" is on) visually covers that screen position.
        /// It does not bypass UIPI: the target window still needs to be at the
        /// same or lower integrity level as this process.
        /// </summary>
        public static void PostClickToWindow(IntPtr hWnd, int clientX, int clientY, MouseButtonKind button, bool doubleClick, int holdMs, int gapMs)
        {
            if (hWnd == IntPtr.Zero || !NativeMethods.IsWindow(hWnd)) return;

            int lp = (clientY << 16) | (clientX & 0xFFFF);
            var lParam = new IntPtr(lp);
            uint downMsg, upMsg;
            IntPtr wParam;
            switch (button)
            {
                case MouseButtonKind.Right:
                    downMsg = (uint)NativeMethods.WM_RBUTTONDOWN;
                    upMsg = (uint)NativeMethods.WM_RBUTTONUP;
                    wParam = (IntPtr)NativeMethods.MK_RBUTTON;
                    break;
                case MouseButtonKind.Middle:
                    downMsg = (uint)NativeMethods.WM_MBUTTONDOWN;
                    upMsg = (uint)NativeMethods.WM_MBUTTONUP;
                    wParam = (IntPtr)NativeMethods.MK_MBUTTON;
                    break;
                default:
                    downMsg = (uint)NativeMethods.WM_LBUTTONDOWN;
                    upMsg = (uint)NativeMethods.WM_LBUTTONUP;
                    wParam = (IntPtr)NativeMethods.MK_LBUTTON;
                    break;
            }

            NativeMethods.PostMessage(hWnd, downMsg, wParam, lParam);
            Thread.Sleep(holdMs);
            NativeMethods.PostMessage(hWnd, upMsg, IntPtr.Zero, lParam);

            if (doubleClick)
            {
                Thread.Sleep(gapMs);
                NativeMethods.PostMessage(hWnd, downMsg, wParam, lParam);
                Thread.Sleep(holdMs);
                NativeMethods.PostMessage(hWnd, upMsg, IntPtr.Zero, lParam);
            }
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
