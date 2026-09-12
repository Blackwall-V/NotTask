using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace AutoClicker
{
    internal enum MacroEventKind
    {
        Move,
        MouseDown,
        MouseUp,
        KeyDown,
        KeyUp
    }

    internal class MacroEvent
    {
        public long TimeMs;
        public MacroEventKind Kind;
        public int X;
        public int Y;
        public MouseButtonKind Button;
        public ushort Vk;

        public string ToLine()
        {
            return string.Join(",", new[]
            {
                TimeMs.ToString(CultureInfo.InvariantCulture),
                Kind.ToString(),
                X.ToString(CultureInfo.InvariantCulture),
                Y.ToString(CultureInfo.InvariantCulture),
                Button.ToString(),
                Vk.ToString(CultureInfo.InvariantCulture)
            });
        }

        public static MacroEvent FromLine(string line)
        {
            var parts = line.Split(',');
            if (parts.Length != 6) return null;
            return new MacroEvent
            {
                TimeMs = long.Parse(parts[0], CultureInfo.InvariantCulture),
                Kind = (MacroEventKind)Enum.Parse(typeof(MacroEventKind), parts[1]),
                X = int.Parse(parts[2], CultureInfo.InvariantCulture),
                Y = int.Parse(parts[3], CultureInfo.InvariantCulture),
                Button = (MouseButtonKind)Enum.Parse(typeof(MouseButtonKind), parts[4]),
                Vk = ushort.Parse(parts[5], CultureInfo.InvariantCulture)
            };
        }
    }

    /// <summary>
    /// Records global mouse/keyboard activity via low-level hooks and plays
    /// it back with the original relative timing, similar to TinyTask.
    /// The four app hotkeys (F6-F9) are never captured into the macro since
    /// they control the app itself.
    /// </summary>
    internal class MacroEngine
    {
        private const int MinMoveIntervalMs = 15;
        public static readonly HashSet<uint> ReservedVks = new HashSet<uint> { 0x75, 0x76, 0x77, 0x78 }; // F6-F9

        private LowLevelProc _mouseProc;
        private LowLevelProc _keyboardProc;
        private IntPtr _mouseHook = IntPtr.Zero;
        private IntPtr _keyboardHook = IntPtr.Zero;
        private Stopwatch _stopwatch;
        private long _lastMoveRecordedMs = -1000;
        private readonly object _lock = new object();

        public List<MacroEvent> Events { get; private set; }
        public bool IsRecording { get; private set; }
        public bool IsPlaying { get; private set; }

        private volatile bool _stopPlaybackRequested;
        private Thread _playbackThread;

        public event Action PlaybackFinished;

        public void StartRecording()
        {
            if (IsRecording) return;
            Events = new List<MacroEvent>();
            _stopwatch = Stopwatch.StartNew();
            _lastMoveRecordedMs = -1000;

            _mouseProc = MouseHookCallback;
            _keyboardProc = KeyboardHookCallback;
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                _mouseHook = NativeMethods.SetWindowsHookEx(NativeMethods.WH_MOUSE_LL, _mouseProc,
                    NativeMethods.GetModuleHandle(curModule.ModuleName), 0);
                _keyboardHook = NativeMethods.SetWindowsHookEx(NativeMethods.WH_KEYBOARD_LL, _keyboardProc,
                    NativeMethods.GetModuleHandle(curModule.ModuleName), 0);
            }
            IsRecording = true;
        }

        public void LoadEvents(List<MacroEvent> events)
        {
            Events = events;
        }

        public void StopRecording()
        {
            if (!IsRecording) return;
            if (_mouseHook != IntPtr.Zero) { NativeMethods.UnhookWindowsHookEx(_mouseHook); _mouseHook = IntPtr.Zero; }
            if (_keyboardHook != IntPtr.Zero) { NativeMethods.UnhookWindowsHookEx(_keyboardHook); _keyboardHook = IntPtr.Zero; }
            IsRecording = false;
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && IsRecording)
            {
                var data = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                int msg = wParam.ToInt32();
                long t = _stopwatch.ElapsedMilliseconds;

                lock (_lock)
                {
                    switch (msg)
                    {
                        case NativeMethods.WM_MOUSEMOVE:
                            if (t - _lastMoveRecordedMs >= MinMoveIntervalMs)
                            {
                                Events.Add(new MacroEvent { TimeMs = t, Kind = MacroEventKind.Move, X = data.pt.X, Y = data.pt.Y });
                                _lastMoveRecordedMs = t;
                            }
                            break;
                        case NativeMethods.WM_LBUTTONDOWN:
                            Events.Add(new MacroEvent { TimeMs = t, Kind = MacroEventKind.MouseDown, X = data.pt.X, Y = data.pt.Y, Button = MouseButtonKind.Left });
                            break;
                        case NativeMethods.WM_LBUTTONUP:
                            Events.Add(new MacroEvent { TimeMs = t, Kind = MacroEventKind.MouseUp, X = data.pt.X, Y = data.pt.Y, Button = MouseButtonKind.Left });
                            break;
                        case NativeMethods.WM_RBUTTONDOWN:
                            Events.Add(new MacroEvent { TimeMs = t, Kind = MacroEventKind.MouseDown, X = data.pt.X, Y = data.pt.Y, Button = MouseButtonKind.Right });
                            break;
                        case NativeMethods.WM_RBUTTONUP:
                            Events.Add(new MacroEvent { TimeMs = t, Kind = MacroEventKind.MouseUp, X = data.pt.X, Y = data.pt.Y, Button = MouseButtonKind.Right });
                            break;
                        case NativeMethods.WM_MBUTTONDOWN:
                            Events.Add(new MacroEvent { TimeMs = t, Kind = MacroEventKind.MouseDown, X = data.pt.X, Y = data.pt.Y, Button = MouseButtonKind.Middle });
                            break;
                        case NativeMethods.WM_MBUTTONUP:
                            Events.Add(new MacroEvent { TimeMs = t, Kind = MacroEventKind.MouseUp, X = data.pt.X, Y = data.pt.Y, Button = MouseButtonKind.Middle });
                            break;
                    }
                }
            }
            return NativeMethods.CallNextHookEx(_mouseHook, nCode, wParam, lParam);
        }

        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && IsRecording)
            {
                var data = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
                int msg = wParam.ToInt32();

                if (!ReservedVks.Contains(data.vkCode))
                {
                    long t = _stopwatch.ElapsedMilliseconds;
                    lock (_lock)
                    {
                        if (msg == NativeMethods.WM_KEYDOWN || msg == NativeMethods.WM_SYSKEYDOWN)
                        {
                            Events.Add(new MacroEvent { TimeMs = t, Kind = MacroEventKind.KeyDown, Vk = (ushort)data.vkCode });
                        }
                        else if (msg == NativeMethods.WM_KEYUP || msg == NativeMethods.WM_SYSKEYUP)
                        {
                            Events.Add(new MacroEvent { TimeMs = t, Kind = MacroEventKind.KeyUp, Vk = (ushort)data.vkCode });
                        }
                    }
                }
            }
            return NativeMethods.CallNextHookEx(_keyboardHook, nCode, wParam, lParam);
        }

        public void StartPlayback(List<MacroEvent> events, int loopCount, bool loopForever, double speed, Action<string> statusCallback)
        {
            if (IsPlaying || events == null || events.Count == 0) return;
            _stopPlaybackRequested = false;
            IsPlaying = true;

            _playbackThread = new Thread(() =>
            {
                try
                {
                    int completed = 0;
                    while (!_stopPlaybackRequested && (loopForever || completed < loopCount))
                    {
                        long lastT = 0;
                        foreach (var ev in events)
                        {
                            if (_stopPlaybackRequested) break;

                            long delta = ev.TimeMs - lastT;
                            lastT = ev.TimeMs;
                            long scaledDelta = speed > 0 ? (long)(delta / speed) : delta;
                            SleepInterruptible(scaledDelta);
                            if (_stopPlaybackRequested) break;

                            switch (ev.Kind)
                            {
                                case MacroEventKind.Move:
                                    InputSimulator.MoveTo(ev.X, ev.Y);
                                    break;
                                case MacroEventKind.MouseDown:
                                    InputSimulator.MoveTo(ev.X, ev.Y);
                                    InputSimulator.MouseDown(ev.Button);
                                    break;
                                case MacroEventKind.MouseUp:
                                    InputSimulator.MoveTo(ev.X, ev.Y);
                                    InputSimulator.MouseUp(ev.Button);
                                    break;
                                case MacroEventKind.KeyDown:
                                    InputSimulator.KeyDown(ev.Vk);
                                    break;
                                case MacroEventKind.KeyUp:
                                    InputSimulator.KeyUp(ev.Vk);
                                    break;
                            }
                        }
                        completed++;
                        if (statusCallback != null)
                        {
                            statusCallback(loopForever
                                ? string.Format("Reproduciendo... (vuelta {0})", completed)
                                : string.Format("Reproduciendo... ({0}/{1})", completed, loopCount));
                        }
                    }
                }
                finally
                {
                    IsPlaying = false;
                    if (PlaybackFinished != null) PlaybackFinished();
                }
            });
            _playbackThread.IsBackground = true;
            _playbackThread.Start();
        }

        public void StopPlayback()
        {
            _stopPlaybackRequested = true;
        }

        private void SleepInterruptible(long ms)
        {
            long remaining = ms;
            while (remaining > 0 && !_stopPlaybackRequested)
            {
                int chunk = (int)Math.Min(remaining, 20);
                Thread.Sleep(chunk);
                remaining -= chunk;
            }
        }

        public static void Save(List<MacroEvent> events, string path)
        {
            using (var writer = new StreamWriter(path, false))
            {
                foreach (var ev in events)
                {
                    writer.WriteLine(ev.ToLine());
                }
            }
        }

        public static List<MacroEvent> Load(string path)
        {
            var result = new List<MacroEvent>();
            foreach (var line in File.ReadAllLines(path))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var ev = MacroEvent.FromLine(line);
                if (ev != null) result.Add(ev);
            }
            return result;
        }
    }
}
