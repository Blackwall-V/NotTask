using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;

namespace AutoClicker
{
    public partial class MainForm : Form
    {
        private const int HOTKEY_AUTOCLICK = 1;
        private const int HOTKEY_SEQUENCE = 2;
        private const int HOTKEY_RECORD = 3;
        private const int HOTKEY_PLAY = 4;
        private const uint VK_F6 = 0x75, VK_F7 = 0x76, VK_F8 = 0x77, VK_F9 = 0x78;

        // ---- Autoclicker state ----
        private volatile bool _autoClickRunning;
        private Thread _autoClickThread;
        private int _fixedX, _fixedY;
        private bool _hasFixedPos;
        private IntPtr _fixedHwnd;
        private int _fixedClientX, _fixedClientY;

        // ---- Multi-position state ----
        private volatile bool _sequenceRunning;
        private Thread _sequenceThread;
        private readonly List<Point> _positions = new List<Point>();

        // ---- Macro state ----
        private readonly MacroEngine _macroEngine = new MacroEngine();

        public MainForm()
        {
            InitializeComponent();
            TopMost = chkTopMost.Checked;
            chkTopMost.CheckedChanged += (s, e) => TopMost = chkTopMost.Checked;

            bool isAdmin = IsRunningAsAdministrator();
            lblElevation.Visible = isAdmin;
            btnRunAsAdmin.Visible = !isAdmin;
            btnRunAsAdmin.Click += btnRunAsAdmin_Click;

            _macroEngine.PlaybackFinished += () => BeginInvoke(new Action(() =>
            {
                btnPlay.Text = "Reproducir (F9)";
                lblMacroInfo.Text = "Reproduccion detenida.";
            }));
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            NativeMethods.RegisterHotKey(Handle, HOTKEY_AUTOCLICK, NativeMethods.MOD_NOREPEAT, VK_F6);
            NativeMethods.RegisterHotKey(Handle, HOTKEY_SEQUENCE, NativeMethods.MOD_NOREPEAT, VK_F7);
            NativeMethods.RegisterHotKey(Handle, HOTKEY_RECORD, NativeMethods.MOD_NOREPEAT, VK_F8);
            NativeMethods.RegisterHotKey(Handle, HOTKEY_PLAY, NativeMethods.MOD_NOREPEAT, VK_F9);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _autoClickRunning = false;
            _sequenceRunning = false;
            _macroEngine.StopPlayback();
            _macroEngine.StopRecording();
            NativeMethods.UnregisterHotKey(Handle, HOTKEY_AUTOCLICK);
            NativeMethods.UnregisterHotKey(Handle, HOTKEY_SEQUENCE);
            NativeMethods.UnregisterHotKey(Handle, HOTKEY_RECORD);
            NativeMethods.UnregisterHotKey(Handle, HOTKEY_PLAY);
            base.OnFormClosing(e);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_HOTKEY)
            {
                int id = m.WParam.ToInt32();
                switch (id)
                {
                    case HOTKEY_AUTOCLICK: ToggleAutoClicker(); break;
                    case HOTKEY_SEQUENCE: ToggleSequence(); break;
                    case HOTKEY_RECORD: ToggleRecording(); break;
                    case HOTKEY_PLAY: TogglePlayback(); break;
                }
            }
            base.WndProc(ref m);
        }

        // ================= AUTOCLICKER =================

        private MouseButtonKind GetAutoClickButton()
        {
            if (radAutoRight.Checked) return MouseButtonKind.Right;
            if (radAutoMiddle.Checked) return MouseButtonKind.Middle;
            return MouseButtonKind.Left;
        }

        private long GetAutoClickIntervalMs()
        {
            long h = (long)numAutoHours.Value;
            long mi = (long)numAutoMinutes.Value;
            long s = (long)numAutoSeconds.Value;
            long ms = (long)numAutoMillis.Value;
            return h * 3600000L + mi * 60000L + s * 1000L + ms;
        }

        private void ToggleAutoClicker()
        {
            if (_autoClickRunning) StopAutoClicker();
            else StartAutoClicker();
        }

        private void StartAutoClicker()
        {
            if (_autoClickRunning) return;

            long intervalMs = GetAutoClickIntervalMs();
            if (intervalMs < 1) intervalMs = 1;

            if (radAutoFixedPos.Checked && !_hasFixedPos)
            {
                MessageBox.Show(this, "Primero capturá una posición fija con el botón 'Capturar posición'.",
                    "AutoClicker", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MouseButtonKind button = GetAutoClickButton();
            bool doubleClick = chkAutoDoubleClick.Checked;
            bool fixedPos = radAutoFixedPos.Checked;
            int fx = _fixedX, fy = _fixedY;
            bool infinite = radAutoUntilStopped.Checked;
            int count = (int)numAutoCount.Value;
            int holdMs = (int)numAutoClickHold.Value;
            bool useWindowMsg = fixedPos && chkAutoUseWindowMsg.Checked;
            IntPtr targetHwnd = _fixedHwnd;
            int clientX = _fixedClientX, clientY = _fixedClientY;

            if (useWindowMsg && (targetHwnd == IntPtr.Zero || !NativeMethods.IsWindow(targetHwnd)))
            {
                MessageBox.Show(this, "La ventana capturada ya no existe. Volvé a capturar la posición.",
                    "AutoClicker", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _autoClickRunning = true;
            btnAutoStart.Text = "Detener (F6)";
            lblAutoStatus.Text = "Clickeando...";

            _autoClickThread = new Thread(() =>
            {
                int done = 0;
                while (_autoClickRunning && (infinite || done < count))
                {
                    if (useWindowMsg)
                    {
                        InputSimulator.PostClickToWindow(targetHwnd, clientX, clientY, button, doubleClick, holdMs, InputSimulator.DoubleClickGapMs);
                    }
                    else
                    {
                        if (fixedPos) InputSimulator.MoveTo(fx, fy);
                        if (doubleClick) InputSimulator.DoubleClick(button, holdMs, InputSimulator.DoubleClickGapMs);
                        else InputSimulator.Click(button, holdMs);
                    }
                    done++;

                    int shown = done;
                    BeginInvoke(new Action(() => lblAutoStatus.Text =
                        infinite ? string.Format("Clickeando... ({0} clicks)", shown)
                                 : string.Format("Clickeando... ({0}/{1})", shown, count)));

                    SleepInterruptible(intervalMs, () => _autoClickRunning);
                }
                _autoClickRunning = false;
                BeginInvoke(new Action(() =>
                {
                    btnAutoStart.Text = "Iniciar (F6)";
                    lblAutoStatus.Text = "Detenido.";
                }));
            });
            _autoClickThread.IsBackground = true;
            _autoClickThread.Start();
        }

        private void StopAutoClicker()
        {
            _autoClickRunning = false;
        }

        private void btnAutoCapturePos_Click(object sender, EventArgs e)
        {
            StartCountdownCapture(3, lblAutoCapturedPos, (x, y) =>
            {
                _fixedX = x; _fixedY = y; _hasFixedPos = true;

                var screenPt = new POINT { X = x, Y = y };
                _fixedHwnd = NativeMethods.WindowFromPoint(screenPt);
                var clientPt = screenPt;
                NativeMethods.ScreenToClient(_fixedHwnd, ref clientPt);
                _fixedClientX = clientPt.X;
                _fixedClientY = clientPt.Y;

                lblAutoCapturedPos.Text = string.Format("Posición capturada: ({0}, {1})", x, y);
            });
        }

        private void btnAutoStart_Click(object sender, EventArgs e)
        {
            ToggleAutoClicker();
        }

        // ================= MULTI-POSICION =================

        private void ToggleSequence()
        {
            if (_sequenceRunning) StopSequence();
            else StartSequence();
        }

        private MouseButtonKind GetSequenceButton()
        {
            if (cmbSeqButton.SelectedIndex == 1) return MouseButtonKind.Right;
            if (cmbSeqButton.SelectedIndex == 2) return MouseButtonKind.Middle;
            return MouseButtonKind.Left;
        }

        private void StartSequence()
        {
            if (_sequenceRunning) return;
            if (_positions.Count == 0)
            {
                MessageBox.Show(this, "Agregá al menos una posición.", "Multi-posición",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var positionsCopy = new List<Point>(_positions);
            int intervalMs = (int)numSeqInterval.Value;
            MouseButtonKind button = GetSequenceButton();
            bool infinite = radSeqUntilStopped.Checked;
            int loops = (int)numSeqLoops.Value;

            _sequenceRunning = true;
            btnSeqStart.Text = "Detener (F7)";
            lblSeqStatus.Text = "Ejecutando secuencia...";

            _sequenceThread = new Thread(() =>
            {
                int loopsDone = 0;
                while (_sequenceRunning && (infinite || loopsDone < loops))
                {
                    for (int i = 0; i < positionsCopy.Count && _sequenceRunning; i++)
                    {
                        var p = positionsCopy[i];
                        InputSimulator.MoveTo(p.X, p.Y);
                        InputSimulator.Click(button);

                        int idx = i + 1;
                        BeginInvoke(new Action(() => lblSeqStatus.Text =
                            string.Format("Click {0}/{1} en ({2},{3})", idx, positionsCopy.Count, p.X, p.Y)));

                        SleepInterruptible(intervalMs, () => _sequenceRunning);
                    }
                    loopsDone++;
                }
                _sequenceRunning = false;
                BeginInvoke(new Action(() =>
                {
                    btnSeqStart.Text = "Iniciar (F7)";
                    lblSeqStatus.Text = "Detenido.";
                }));
            });
            _sequenceThread.IsBackground = true;
            _sequenceThread.Start();
        }

        private void StopSequence()
        {
            _sequenceRunning = false;
        }

        private void btnSeqAddPos_Click(object sender, EventArgs e)
        {
            StartCountdownCapture(3, lblSeqCaptureStatus, (x, y) =>
            {
                _positions.Add(new Point(x, y));
                lstPositions.Items.Add(string.Format("{0}: ({1}, {2})", lstPositions.Items.Count + 1, x, y));
                lblSeqCaptureStatus.Text = "Posición agregada.";
            });
        }

        private void btnSeqRemovePos_Click(object sender, EventArgs e)
        {
            int idx = lstPositions.SelectedIndex;
            if (idx < 0) return;
            _positions.RemoveAt(idx);
            RebuildPositionsList();
        }

        private void btnSeqClearPos_Click(object sender, EventArgs e)
        {
            _positions.Clear();
            RebuildPositionsList();
        }

        private void RebuildPositionsList()
        {
            lstPositions.Items.Clear();
            for (int i = 0; i < _positions.Count; i++)
            {
                var p = _positions[i];
                lstPositions.Items.Add(string.Format("{0}: ({1}, {2})", i + 1, p.X, p.Y));
            }
        }

        private void btnSeqStart_Click(object sender, EventArgs e)
        {
            ToggleSequence();
        }

        // ================= MACRO (grabar/reproducir) =================

        private void ToggleRecording()
        {
            if (_macroEngine.IsPlaying) return;

            if (_macroEngine.IsRecording)
            {
                _macroEngine.StopRecording();
                btnRecord.Text = "Grabar (F8)";
                lblMacroInfo.Text = string.Format("Grabados {0} eventos.", _macroEngine.Events.Count);
                btnPlay.Enabled = _macroEngine.Events.Count > 0;
                btnSaveMacro.Enabled = _macroEngine.Events.Count > 0;
            }
            else
            {
                _macroEngine.StartRecording();
                btnRecord.Text = "Detener grabación (F8)";
                lblMacroInfo.Text = "Grabando... (F8 para detener)";
                btnPlay.Enabled = false;
                btnSaveMacro.Enabled = false;
            }
        }

        private void TogglePlayback()
        {
            if (_macroEngine.IsRecording) return;

            if (_macroEngine.IsPlaying)
            {
                _macroEngine.StopPlayback();
            }
            else
            {
                if (_macroEngine.Events == null || _macroEngine.Events.Count == 0)
                {
                    MessageBox.Show(this, "No hay ninguna macro grabada o cargada.", "Macro",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                bool infinite = radPlayUntilStopped.Checked;
                int count = radPlayOnce.Checked ? 1 : (int)numPlayCount.Value;
                double speed = (double)numPlaySpeed.Value;
                btnPlay.Text = "Detener (F9)";
                lblMacroInfo.Text = "Reproduciendo...";
                _macroEngine.StartPlayback(_macroEngine.Events, count, infinite, speed, status =>
                {
                    BeginInvoke(new Action(() => lblMacroInfo.Text = status));
                });
            }
        }

        private void btnRecord_Click(object sender, EventArgs e)
        {
            ToggleRecording();
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            TogglePlayback();
        }

        private void btnSaveMacro_Click(object sender, EventArgs e)
        {
            if (_macroEngine.Events == null || _macroEngine.Events.Count == 0) return;
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "Macro AutoClicker (*.actm)|*.actm";
                dlg.FileName = "macro.actm";
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    MacroEngine.Save(_macroEngine.Events, dlg.FileName);
                    lblMacroInfo.Text = "Macro guardada en " + dlg.FileName;
                }
            }
        }

        private void btnLoadMacro_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "Macro AutoClicker (*.actm)|*.actm";
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    var loaded = MacroEngine.Load(dlg.FileName);
                    _macroEngine.LoadEvents(loaded);
                    lblMacroInfo.Text = string.Format("Macro cargada: {0} eventos.", loaded.Count);
                    btnPlay.Enabled = loaded.Count > 0;
                    btnSaveMacro.Enabled = loaded.Count > 0;
                }
            }
        }

        // ================= PRIVILEGIOS =================

        private static bool IsRunningAsAdministrator()
        {
            using (var identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        private void btnRunAsAdmin_Click(object sender, EventArgs e)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = Application.ExecutablePath,
                    UseShellExecute = true,
                    Verb = "runas"
                };
                Process.Start(psi);
                Application.Exit();
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // El usuario canceló el cuadro de UAC.
            }
        }

        // ================= HELPERS =================

        private void StartCountdownCapture(int seconds, Label feedbackLabel, Action<int, int> onCaptured)
        {
            int remaining = seconds;
            feedbackLabel.Text = string.Format("Mové el mouse al destino... {0}", remaining);
            var timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000;
            timer.Tick += (s, e) =>
            {
                remaining--;
                if (remaining <= 0)
                {
                    timer.Stop();
                    timer.Dispose();
                    var p = InputSimulator.GetCursorPos();
                    onCaptured(p.X, p.Y);
                }
                else
                {
                    feedbackLabel.Text = string.Format("Mové el mouse al destino... {0}", remaining);
                }
            };
            timer.Start();
        }

        private static void SleepInterruptible(long ms, Func<bool> shouldContinue)
        {
            long remaining = ms;
            while (remaining > 0 && shouldContinue())
            {
                int chunk = (int)Math.Min(remaining, 20);
                Thread.Sleep(chunk);
                remaining -= chunk;
            }
        }
    }
}
