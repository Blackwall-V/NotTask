using System.Drawing;
using System.Windows.Forms;

namespace AutoClicker
{
    public partial class MainForm
    {
        private TabControl tabControl;
        private CheckBox chkTopMost;
        private Label lblElevation;
        private Button btnRunAsAdmin;

        // Autoclicker tab (minimal) + settings dialog
        private Button btnAutoStart;
        private Label lblAutoStatus;
        private Button btnAutoSettings;
        private Form _autoSettingsForm;
        private RadioButton radAutoLeft, radAutoRight, radAutoMiddle;
        private CheckBox chkAutoDoubleClick;
        private NumericUpDown numAutoHours, numAutoMinutes, numAutoSeconds, numAutoMillis;
        private RadioButton radAutoCurrentPos, radAutoFixedPos;
        private Button btnAutoCapturePos;
        private Label lblAutoCapturedPos;
        private CheckBox chkAutoUseWindowMsg;
        private RadioButton radAutoUntilStopped, radAutoCount;
        private NumericUpDown numAutoCount;
        private NumericUpDown numAutoClickHold;

        // Macro tab (minimal) + settings dialog
        private Button btnRecord, btnPlay;
        private Label lblMacroInfo;
        private Button btnMacroSettings;
        private Form _macroSettingsForm;
        private RadioButton radPlayOnce, radPlayCount, radPlayUntilStopped;
        private NumericUpDown numPlayCount;
        private NumericUpDown numPlaySpeed;
        private Button btnSaveMacro, btnLoadMacro;

        // Multi-position tab (minimal) + settings dialog
        private ListBox lstPositions;
        private Button btnSeqAddPos, btnSeqStart;
        private Label lblSeqCaptureStatus, lblSeqStatus;
        private Button btnSeqSettings;
        private Form _seqSettingsForm;
        private Button btnSeqRemovePos, btnSeqClearPos;
        private NumericUpDown numSeqInterval, numSeqLoops;
        private RadioButton radSeqOnce, radSeqLoops, radSeqUntilStopped;
        private ComboBox cmbSeqButton;

        private void InitializeComponent()
        {
            AutoScaleMode = AutoScaleMode.None;
            Text = "AutoClicker Tool";
            ClientSize = new Size(340, 236);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 8.25F);

            var topPanel = new Panel { Dock = DockStyle.Top, Height = 24 };
            chkTopMost = new CheckBox
            {
                Text = "Encima de todo",
                Location = new Point(6, 4),
                AutoSize = true,
                Checked = true
            };
            lblElevation = new Label
            {
                Text = "Admin ✓",
                Location = new Point(255, 5),
                AutoSize = true,
                ForeColor = Color.SeaGreen,
                Visible = false
            };
            btnRunAsAdmin = new Button
            {
                Text = "Reiniciar como admin",
                Location = new Point(150, 1),
                Size = new Size(184, 20),
                Visible = false
            };
            topPanel.Controls.Add(chkTopMost);
            topPanel.Controls.Add(lblElevation);
            topPanel.Controls.Add(btnRunAsAdmin);

            tabControl = new TabControl { Dock = DockStyle.Fill };

            var tabAuto = new TabPage("Autoclicker");
            var tabMacro = new TabPage("Macro");
            var tabSeq = new TabPage("Multi-posición");
            tabControl.TabPages.Add(tabAuto);
            tabControl.TabPages.Add(tabMacro);
            tabControl.TabPages.Add(tabSeq);

            BuildAutoTab(tabAuto);
            BuildMacroTab(tabMacro);
            BuildSeqTab(tabSeq);

            Controls.Add(tabControl);
            Controls.Add(topPanel);
        }

        private static Form CreateSettingsDialog(string title, int width, int height, out Button btnClose)
        {
            var form = new Form
            {
                Text = title,
                ClientSize = new Size(width, height),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.CenterParent,
                Font = new Font("Segoe UI", 8.25F),
                AutoScaleMode = AutoScaleMode.None
            };
            btnClose = new Button
            {
                Text = "Cerrar",
                Size = new Size(90, 26),
                Location = new Point(width - 100, height - 36)
            };
            btnClose.Click += (s, e) => form.Hide();
            form.Controls.Add(btnClose);
            form.CancelButton = btnClose;
            return form;
        }

        private void BuildAutoTab(TabPage tab)
        {
            btnAutoStart = new Button { Text = "Iniciar (F6)", Location = new Point(10, 10), Size = new Size(140, 32) };
            btnAutoStart.Click += btnAutoStart_Click;
            lblAutoStatus = new Label { Text = "Detenido.", Location = new Point(160, 18), AutoSize = true };
            btnAutoSettings = new Button { Text = "⚙ Configurar", Location = new Point(10, 50), Size = new Size(140, 26) };

            tab.Controls.Add(btnAutoStart);
            tab.Controls.Add(lblAutoStatus);
            tab.Controls.Add(btnAutoSettings);

            Button closeBtn;
            _autoSettingsForm = CreateSettingsDialog("Configuración - Autoclicker", 470, 420, out closeBtn);
            btnAutoSettings.Click += (s, e) => _autoSettingsForm.ShowDialog(this);

            var grpButton = new GroupBox { Text = "Tipo de click", Location = new Point(10, 10), Size = new Size(210, 74) };
            radAutoLeft = new RadioButton { Text = "Izquierdo", Location = new Point(10, 16), Checked = true, AutoSize = true };
            radAutoRight = new RadioButton { Text = "Derecho", Location = new Point(10, 36), AutoSize = true };
            radAutoMiddle = new RadioButton { Text = "Medio (rueda)", Location = new Point(10, 56), AutoSize = true };
            chkAutoDoubleClick = new CheckBox { Text = "Doble click", Location = new Point(110, 16), AutoSize = true };
            grpButton.Controls.Add(radAutoLeft);
            grpButton.Controls.Add(radAutoRight);
            grpButton.Controls.Add(radAutoMiddle);
            grpButton.Controls.Add(chkAutoDoubleClick);

            var grpInterval = new GroupBox { Text = "Intervalo entre clicks", Location = new Point(228, 10), Size = new Size(226, 74) };
            grpInterval.Controls.Add(new Label { Text = "hs", Location = new Point(8, 20), AutoSize = true });
            numAutoHours = new NumericUpDown { Location = new Point(28, 18), Width = 40, Maximum = 23 };
            grpInterval.Controls.Add(new Label { Text = "min", Location = new Point(74, 20), AutoSize = true });
            numAutoMinutes = new NumericUpDown { Location = new Point(96, 18), Width = 40, Maximum = 59 };
            grpInterval.Controls.Add(new Label { Text = "seg", Location = new Point(146, 20), AutoSize = true });
            numAutoSeconds = new NumericUpDown { Location = new Point(168, 18), Width = 40, Maximum = 59, Value = 1 };
            grpInterval.Controls.Add(new Label { Text = "ms", Location = new Point(8, 48), AutoSize = true });
            numAutoMillis = new NumericUpDown { Location = new Point(30, 46), Width = 55, Maximum = 999 };
            grpInterval.Controls.Add(numAutoHours);
            grpInterval.Controls.Add(numAutoMinutes);
            grpInterval.Controls.Add(numAutoSeconds);
            grpInterval.Controls.Add(numAutoMillis);

            var grpPos = new GroupBox { Text = "Posición del click", Location = new Point(10, 90), Size = new Size(444, 88) };
            radAutoCurrentPos = new RadioButton { Text = "Posición actual del cursor", Location = new Point(10, 16), Checked = true, AutoSize = true };
            radAutoFixedPos = new RadioButton { Text = "Posición fija:", Location = new Point(10, 36), AutoSize = true };
            btnAutoCapturePos = new Button { Text = "Capturar posición (3s)", Location = new Point(110, 33), Size = new Size(150, 24) };
            lblAutoCapturedPos = new Label { Text = "Sin capturar.", Location = new Point(268, 37), Size = new Size(170, 16) };
            chkAutoUseWindowMsg = new CheckBox
            {
                Text = "Enviar directo a la ventana (ignora si esta app tapa el punto)",
                Location = new Point(10, 62),
                AutoSize = true,
                Enabled = false
            };
            btnAutoCapturePos.Click += btnAutoCapturePos_Click;
            radAutoFixedPos.CheckedChanged += (s, e) => chkAutoUseWindowMsg.Enabled = radAutoFixedPos.Checked;
            grpPos.Controls.Add(radAutoCurrentPos);
            grpPos.Controls.Add(radAutoFixedPos);
            grpPos.Controls.Add(btnAutoCapturePos);
            grpPos.Controls.Add(lblAutoCapturedPos);
            grpPos.Controls.Add(chkAutoUseWindowMsg);

            var grpRepeat = new GroupBox { Text = "Repetición", Location = new Point(10, 184), Size = new Size(444, 46) };
            radAutoUntilStopped = new RadioButton { Text = "Hasta detener (F6)", Location = new Point(10, 16), Checked = true, AutoSize = true };
            radAutoCount = new RadioButton { Text = "Cantidad de clicks:", Location = new Point(170, 16), AutoSize = true };
            numAutoCount = new NumericUpDown { Location = new Point(300, 14), Width = 70, Maximum = 1000000, Minimum = 1, Value = 10 };
            grpRepeat.Controls.Add(radAutoUntilStopped);
            grpRepeat.Controls.Add(radAutoCount);
            grpRepeat.Controls.Add(numAutoCount);

            var lblHold = new Label { Text = "Duración del click (ms):", Location = new Point(10, 240), AutoSize = true };
            numAutoClickHold = new NumericUpDown { Location = new Point(152, 237), Width = 55, Minimum = 1, Maximum = 500, Value = 20 };

            var note = new Label
            {
                Location = new Point(10, 272),
                Size = new Size(444, 100),
                Font = new Font("Segoe UI", 7.5F),
                ForeColor = SystemColors.GrayText,
                Text = "SendInput es más confiable que mover el mouse por software. \"Enviar directo a la ventana\" evita que esta misma app tape el click, pero ninguno de los dos evita el anti-cheat de kernel (EAC, BattlEye, Vanguard) ni el bloqueo de Windows (UIPI) entre procesos con distinto privilegio."
            };

            _autoSettingsForm.Controls.Add(grpButton);
            _autoSettingsForm.Controls.Add(grpInterval);
            _autoSettingsForm.Controls.Add(grpPos);
            _autoSettingsForm.Controls.Add(grpRepeat);
            _autoSettingsForm.Controls.Add(lblHold);
            _autoSettingsForm.Controls.Add(numAutoClickHold);
            _autoSettingsForm.Controls.Add(note);
        }

        private void BuildMacroTab(TabPage tab)
        {
            btnRecord = new Button { Text = "Grabar (F8)", Location = new Point(10, 10), Size = new Size(140, 28) };
            btnRecord.Click += btnRecord_Click;
            btnPlay = new Button { Text = "Reproducir (F9)", Location = new Point(160, 10), Size = new Size(140, 28), Enabled = false };
            btnPlay.Click += btnPlay_Click;
            lblMacroInfo = new Label { Location = new Point(10, 46), Size = new Size(300, 40), Text = "Sin grabar ni cargar ninguna macro." };
            btnMacroSettings = new Button { Text = "⚙ Configurar", Location = new Point(10, 92), Size = new Size(140, 26) };

            tab.Controls.Add(btnRecord);
            tab.Controls.Add(btnPlay);
            tab.Controls.Add(lblMacroInfo);
            tab.Controls.Add(btnMacroSettings);

            Button closeBtn;
            _macroSettingsForm = CreateSettingsDialog("Configuración - Macro", 470, 270, out closeBtn);
            btnMacroSettings.Click += (s, e) => _macroSettingsForm.ShowDialog(this);

            var grpPlay = new GroupBox { Text = "Reproducción", Location = new Point(10, 10), Size = new Size(444, 70) };
            radPlayOnce = new RadioButton { Text = "Una vez", Location = new Point(10, 16), Checked = true, AutoSize = true };
            radPlayCount = new RadioButton { Text = "N veces:", Location = new Point(90, 16), AutoSize = true };
            numPlayCount = new NumericUpDown { Location = new Point(168, 14), Width = 55, Minimum = 1, Maximum = 100000, Value = 5 };
            radPlayUntilStopped = new RadioButton { Text = "Hasta detener (F9)", Location = new Point(232, 16), AutoSize = true };
            grpPlay.Controls.Add(new Label { Text = "Velocidad (x):", Location = new Point(10, 44), AutoSize = true });
            numPlaySpeed = new NumericUpDown
            {
                Location = new Point(100, 42),
                Width = 55,
                DecimalPlaces = 2,
                Increment = 0.1M,
                Minimum = 0.1M,
                Maximum = 5.0M,
                Value = 1.0M
            };
            grpPlay.Controls.Add(radPlayOnce);
            grpPlay.Controls.Add(radPlayCount);
            grpPlay.Controls.Add(numPlayCount);
            grpPlay.Controls.Add(radPlayUntilStopped);
            grpPlay.Controls.Add(numPlaySpeed);

            btnSaveMacro = new Button { Text = "Guardar macro...", Location = new Point(10, 90), Size = new Size(140, 26), Enabled = false };
            btnSaveMacro.Click += btnSaveMacro_Click;
            btnLoadMacro = new Button { Text = "Cargar macro...", Location = new Point(160, 90), Size = new Size(140, 26) };
            btnLoadMacro.Click += btnLoadMacro_Click;

            var note = new Label
            {
                Location = new Point(10, 126),
                Size = new Size(444, 90),
                Font = new Font("Segoe UI", 7.5F),
                ForeColor = SystemColors.GrayText,
                Text = "Graba movimientos de mouse, clicks y teclas con su tiempo real, y los reproduce igual (como TinyTask). " +
                       "Las teclas F6-F9 nunca se graban porque son los atajos de esta app. Guardá la macro como archivo .actm para reutilizarla."
            };

            _macroSettingsForm.Controls.Add(grpPlay);
            _macroSettingsForm.Controls.Add(btnSaveMacro);
            _macroSettingsForm.Controls.Add(btnLoadMacro);
            _macroSettingsForm.Controls.Add(note);
        }

        private void BuildSeqTab(TabPage tab)
        {
            lstPositions = new ListBox { Location = new Point(10, 8), Size = new Size(160, 160) };
            btnSeqAddPos = new Button { Text = "Agregar (3s)", Location = new Point(180, 8), Size = new Size(140, 26) };
            btnSeqAddPos.Click += btnSeqAddPos_Click;
            lblSeqCaptureStatus = new Label { Location = new Point(180, 36), Size = new Size(140, 30), Text = "" };
            btnSeqStart = new Button { Text = "Iniciar (F7)", Location = new Point(180, 68), Size = new Size(140, 28) };
            btnSeqStart.Click += btnSeqStart_Click;
            lblSeqStatus = new Label { Location = new Point(180, 100), Size = new Size(140, 50), Text = "Detenido." };
            btnSeqSettings = new Button { Text = "⚙ Configurar", Location = new Point(180, 154), Size = new Size(140, 26) };

            tab.Controls.Add(lstPositions);
            tab.Controls.Add(btnSeqAddPos);
            tab.Controls.Add(lblSeqCaptureStatus);
            tab.Controls.Add(btnSeqStart);
            tab.Controls.Add(lblSeqStatus);
            tab.Controls.Add(btnSeqSettings);

            Button closeBtn;
            _seqSettingsForm = CreateSettingsDialog("Configuración - Multi-posición", 410, 250, out closeBtn);
            btnSeqSettings.Click += (s, e) => _seqSettingsForm.ShowDialog(this);

            btnSeqRemovePos = new Button { Text = "Quitar seleccionada", Location = new Point(10, 10), Size = new Size(180, 26) };
            btnSeqRemovePos.Click += btnSeqRemovePos_Click;
            btnSeqClearPos = new Button { Text = "Limpiar todas", Location = new Point(200, 10), Size = new Size(180, 26) };
            btnSeqClearPos.Click += btnSeqClearPos_Click;

            var lblButtonType = new Label { Text = "Tipo de click:", Location = new Point(10, 50), AutoSize = true };
            cmbSeqButton = new ComboBox { Location = new Point(90, 47), Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSeqButton.Items.AddRange(new object[] { "Izquierdo", "Derecho", "Medio" });
            cmbSeqButton.SelectedIndex = 0;

            var grpInterval = new GroupBox { Text = "Intervalo entre clicks", Location = new Point(10, 84), Size = new Size(180, 46) };
            numSeqInterval = new NumericUpDown { Location = new Point(10, 18), Width = 80, Minimum = 10, Maximum = 3600000, Value = 500 };
            grpInterval.Controls.Add(numSeqInterval);
            grpInterval.Controls.Add(new Label { Text = "ms", Location = new Point(96, 20), AutoSize = true });

            var grpRepeat = new GroupBox { Text = "Repetición", Location = new Point(200, 84), Size = new Size(200, 52) };
            radSeqOnce = new RadioButton { Text = "Una vuelta", Location = new Point(8, 14), Checked = true, AutoSize = true };
            radSeqLoops = new RadioButton { Text = "N vueltas:", Location = new Point(84, 14), AutoSize = true };
            numSeqLoops = new NumericUpDown { Location = new Point(155, 12), Width = 40, Minimum = 1, Maximum = 100000, Value = 5 };
            radSeqUntilStopped = new RadioButton { Text = "Hasta detener (F7)", Location = new Point(8, 32), AutoSize = true };
            grpRepeat.Controls.Add(radSeqOnce);
            grpRepeat.Controls.Add(radSeqLoops);
            grpRepeat.Controls.Add(numSeqLoops);
            grpRepeat.Controls.Add(radSeqUntilStopped);

            _seqSettingsForm.Controls.Add(btnSeqRemovePos);
            _seqSettingsForm.Controls.Add(btnSeqClearPos);
            _seqSettingsForm.Controls.Add(lblButtonType);
            _seqSettingsForm.Controls.Add(cmbSeqButton);
            _seqSettingsForm.Controls.Add(grpInterval);
            _seqSettingsForm.Controls.Add(grpRepeat);
        }
    }
}
