using System.Drawing;
using System.Windows.Forms;

namespace AutoClicker
{
    public partial class MainForm
    {
        private TabControl tabControl;
        private Label lblLegend;
        private CheckBox chkTopMost;
        private Label lblElevation;
        private Button btnRunAsAdmin;

        // Autoclicker tab
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
        private Button btnAutoStart;
        private Label lblAutoStatus;

        // Macro tab
        private Button btnRecord, btnPlay, btnSaveMacro, btnLoadMacro;
        private RadioButton radPlayOnce, radPlayCount, radPlayUntilStopped;
        private NumericUpDown numPlayCount;
        private NumericUpDown numPlaySpeed;
        private Label lblMacroInfo;

        // Multi-position tab
        private ListBox lstPositions;
        private Button btnSeqAddPos, btnSeqRemovePos, btnSeqClearPos, btnSeqStart;
        private Label lblSeqCaptureStatus, lblSeqStatus;
        private NumericUpDown numSeqInterval, numSeqLoops;
        private RadioButton radSeqOnce, radSeqLoops, radSeqUntilStopped;
        private ComboBox cmbSeqButton;

        private void InitializeComponent()
        {
            Text = "AutoClicker Tool";
            ClientSize = new Size(480, 460);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 8.25F);

            var topPanel = new Panel { Dock = DockStyle.Top, Height = 48 };
            chkTopMost = new CheckBox
            {
                Text = "Mantener encima de otras ventanas",
                Location = new Point(8, 4),
                AutoSize = true,
                Checked = true
            };
            lblElevation = new Label
            {
                Text = "Ejecutando como administrador ✓",
                Location = new Point(8, 26),
                AutoSize = true,
                ForeColor = Color.SeaGreen,
                Visible = false
            };
            btnRunAsAdmin = new Button
            {
                Text = "Reiniciar como administrador",
                Location = new Point(8, 23),
                Size = new Size(200, 22),
                Visible = false
            };
            topPanel.Controls.Add(chkTopMost);
            topPanel.Controls.Add(lblElevation);
            topPanel.Controls.Add(btnRunAsAdmin);

            tabControl = new TabControl { Dock = DockStyle.Fill };

            var tabAuto = new TabPage("Autoclicker");
            var tabMacro = new TabPage("Macro (grabar/reproducir)");
            var tabSeq = new TabPage("Multi-posición");
            tabControl.TabPages.Add(tabAuto);
            tabControl.TabPages.Add(tabMacro);
            tabControl.TabPages.Add(tabSeq);

            BuildAutoTab(tabAuto);
            BuildMacroTab(tabMacro);
            BuildSeqTab(tabSeq);

            lblLegend = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 20,
                Font = new Font("Segoe UI", 7.5F),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "F6 Autoclick   |   F7 Multi-posición   |   F8 Grabar   |   F9 Reproducir",
                ForeColor = SystemColors.GrayText
            };

            Controls.Add(tabControl);
            Controls.Add(lblLegend);
            Controls.Add(topPanel);
        }

        private void BuildAutoTab(TabPage tab)
        {
            var grpButton = new GroupBox { Text = "Tipo de click", Location = new Point(8, 6), Size = new Size(210, 74) };
            radAutoLeft = new RadioButton { Text = "Izquierdo", Location = new Point(10, 16), Checked = true, AutoSize = true };
            radAutoRight = new RadioButton { Text = "Derecho", Location = new Point(10, 36), AutoSize = true };
            radAutoMiddle = new RadioButton { Text = "Medio (rueda)", Location = new Point(10, 56), AutoSize = true };
            chkAutoDoubleClick = new CheckBox { Text = "Doble click", Location = new Point(110, 16), AutoSize = true };
            grpButton.Controls.Add(radAutoLeft);
            grpButton.Controls.Add(radAutoRight);
            grpButton.Controls.Add(radAutoMiddle);
            grpButton.Controls.Add(chkAutoDoubleClick);

            var grpInterval = new GroupBox { Text = "Intervalo entre clicks", Location = new Point(226, 6), Size = new Size(226, 74) };
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

            var grpPos = new GroupBox { Text = "Posición del click", Location = new Point(8, 86), Size = new Size(444, 88) };
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

            var grpRepeat = new GroupBox { Text = "Repetición", Location = new Point(8, 180), Size = new Size(444, 46) };
            radAutoUntilStopped = new RadioButton { Text = "Hasta detener (F6)", Location = new Point(10, 16), Checked = true, AutoSize = true };
            radAutoCount = new RadioButton { Text = "Cantidad de clicks:", Location = new Point(170, 16), AutoSize = true };
            numAutoCount = new NumericUpDown { Location = new Point(300, 14), Width = 70, Maximum = 1000000, Minimum = 1, Value = 10 };
            grpRepeat.Controls.Add(radAutoUntilStopped);
            grpRepeat.Controls.Add(radAutoCount);
            grpRepeat.Controls.Add(numAutoCount);

            var lblHold = new Label { Text = "Duración del click (ms):", Location = new Point(8, 234), AutoSize = true };
            numAutoClickHold = new NumericUpDown { Location = new Point(150, 231), Width = 55, Minimum = 1, Maximum = 500, Value = 20 };

            btnAutoStart = new Button { Text = "Iniciar (F6)", Location = new Point(8, 262), Size = new Size(120, 28) };
            btnAutoStart.Click += btnAutoStart_Click;
            lblAutoStatus = new Label { Text = "Detenido.", Location = new Point(136, 268), AutoSize = true };

            var note = new Label
            {
                Location = new Point(8, 296),
                Size = new Size(444, 60),
                Font = new Font("Segoe UI", 7.5F),
                ForeColor = SystemColors.GrayText,
                Text = "SendInput es más confiable que mover el mouse por software. \"Enviar directo a la ventana\" evita que esta misma app tape el click, pero ninguno de los dos evita el anti-cheat de kernel (EAC, BattlEye, Vanguard) ni el bloqueo de Windows (UIPI) entre procesos con distinto privilegio."
            };

            tab.Controls.Add(grpButton);
            tab.Controls.Add(grpInterval);
            tab.Controls.Add(grpPos);
            tab.Controls.Add(grpRepeat);
            tab.Controls.Add(lblHold);
            tab.Controls.Add(numAutoClickHold);
            tab.Controls.Add(btnAutoStart);
            tab.Controls.Add(lblAutoStatus);
            tab.Controls.Add(note);
        }

        private void BuildMacroTab(TabPage tab)
        {
            btnRecord = new Button { Text = "Grabar (F8)", Location = new Point(8, 8), Size = new Size(145, 28) };
            btnRecord.Click += btnRecord_Click;
            btnPlay = new Button { Text = "Reproducir (F9)", Location = new Point(160, 8), Size = new Size(145, 28), Enabled = false };
            btnPlay.Click += btnPlay_Click;

            lblMacroInfo = new Label { Location = new Point(8, 42), Size = new Size(444, 30), Text = "Sin grabar ni cargar ninguna macro." };

            var grpPlay = new GroupBox { Text = "Reproducción", Location = new Point(8, 76), Size = new Size(444, 70) };
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

            btnSaveMacro = new Button { Text = "Guardar macro...", Location = new Point(8, 154), Size = new Size(140, 26), Enabled = false };
            btnSaveMacro.Click += btnSaveMacro_Click;
            btnLoadMacro = new Button { Text = "Cargar macro...", Location = new Point(154, 154), Size = new Size(140, 26) };
            btnLoadMacro.Click += btnLoadMacro_Click;

            var note = new Label
            {
                Location = new Point(8, 190),
                Size = new Size(444, 90),
                Font = new Font("Segoe UI", 7.5F),
                ForeColor = SystemColors.GrayText,
                Text = "Graba movimientos de mouse, clicks y teclas con su tiempo real, y los reproduce igual (como TinyTask). " +
                       "Las teclas F6-F9 nunca se graban porque son los atajos de esta app. Guardá la macro como archivo .actm para reutilizarla."
            };

            tab.Controls.Add(btnRecord);
            tab.Controls.Add(btnPlay);
            tab.Controls.Add(lblMacroInfo);
            tab.Controls.Add(grpPlay);
            tab.Controls.Add(btnSaveMacro);
            tab.Controls.Add(btnLoadMacro);
            tab.Controls.Add(note);
        }

        private void BuildSeqTab(TabPage tab)
        {
            lstPositions = new ListBox { Location = new Point(8, 8), Size = new Size(180, 190) };

            btnSeqAddPos = new Button { Text = "Agregar posición (3s)", Location = new Point(198, 8), Size = new Size(150, 26) };
            btnSeqAddPos.Click += btnSeqAddPos_Click;
            btnSeqRemovePos = new Button { Text = "Quitar seleccionada", Location = new Point(198, 38), Size = new Size(150, 26) };
            btnSeqRemovePos.Click += btnSeqRemovePos_Click;
            btnSeqClearPos = new Button { Text = "Limpiar todas", Location = new Point(198, 68), Size = new Size(150, 26) };
            btnSeqClearPos.Click += btnSeqClearPos_Click;
            lblSeqCaptureStatus = new Label { Location = new Point(198, 98), Size = new Size(246, 34), Text = "" };

            var lblButtonType = new Label { Text = "Tipo de click:", Location = new Point(198, 140), AutoSize = true };
            cmbSeqButton = new ComboBox { Location = new Point(270, 137), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSeqButton.Items.AddRange(new object[] { "Izquierdo", "Derecho", "Medio" });
            cmbSeqButton.SelectedIndex = 0;

            tab.Controls.Add(lstPositions);
            tab.Controls.Add(btnSeqAddPos);
            tab.Controls.Add(btnSeqRemovePos);
            tab.Controls.Add(btnSeqClearPos);
            tab.Controls.Add(lblSeqCaptureStatus);
            tab.Controls.Add(lblButtonType);
            tab.Controls.Add(cmbSeqButton);

            var grpInterval = new GroupBox { Text = "Intervalo entre clicks", Location = new Point(8, 204), Size = new Size(180, 46) };
            numSeqInterval = new NumericUpDown { Location = new Point(10, 18), Width = 80, Minimum = 10, Maximum = 3600000, Value = 500 };
            grpInterval.Controls.Add(numSeqInterval);
            grpInterval.Controls.Add(new Label { Text = "ms", Location = new Point(96, 20), AutoSize = true });

            var grpRepeat = new GroupBox { Text = "Repetición", Location = new Point(198, 204), Size = new Size(250, 52) };
            radSeqOnce = new RadioButton { Text = "Una vuelta", Location = new Point(8, 14), Checked = true, AutoSize = true };
            radSeqLoops = new RadioButton { Text = "N vueltas:", Location = new Point(84, 14), AutoSize = true };
            numSeqLoops = new NumericUpDown { Location = new Point(155, 12), Width = 50, Minimum = 1, Maximum = 100000, Value = 5 };
            radSeqUntilStopped = new RadioButton { Text = "Hasta detener (F7)", Location = new Point(8, 32), AutoSize = true };
            grpRepeat.Controls.Add(radSeqOnce);
            grpRepeat.Controls.Add(radSeqLoops);
            grpRepeat.Controls.Add(numSeqLoops);
            grpRepeat.Controls.Add(radSeqUntilStopped);

            btnSeqStart = new Button { Text = "Iniciar (F7)", Location = new Point(8, 262), Size = new Size(130, 30) };
            btnSeqStart.Click += btnSeqStart_Click;
            lblSeqStatus = new Label { Location = new Point(146, 269), Size = new Size(300, 20), Text = "Detenido." };

            tab.Controls.Add(grpInterval);
            tab.Controls.Add(grpRepeat);
            tab.Controls.Add(btnSeqStart);
            tab.Controls.Add(lblSeqStatus);
        }
    }
}
