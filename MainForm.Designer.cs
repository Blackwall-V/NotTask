using System.Drawing;
using System.Windows.Forms;

namespace AutoClicker
{
    public partial class MainForm
    {
        private TabControl tabControl;
        private Label lblLegend;

        // Autoclicker tab
        private RadioButton radAutoLeft, radAutoRight, radAutoMiddle;
        private CheckBox chkAutoDoubleClick;
        private NumericUpDown numAutoHours, numAutoMinutes, numAutoSeconds, numAutoMillis;
        private RadioButton radAutoCurrentPos, radAutoFixedPos;
        private Button btnAutoCapturePos;
        private Label lblAutoCapturedPos;
        private RadioButton radAutoUntilStopped, radAutoCount;
        private NumericUpDown numAutoCount;
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
            ClientSize = new Size(560, 500);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

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
                Height = 26,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "F6: Autoclicker   |   F7: Multi-posición   |   F8: Grabar macro   |   F9: Reproducir macro",
                ForeColor = SystemColors.GrayText
            };

            Controls.Add(tabControl);
            Controls.Add(lblLegend);
        }

        private void BuildAutoTab(TabPage tab)
        {
            var grpButton = new GroupBox { Text = "Tipo de click", Location = new Point(12, 12), Size = new Size(250, 90) };
            radAutoLeft = new RadioButton { Text = "Izquierdo", Location = new Point(15, 22), Checked = true, AutoSize = true };
            radAutoRight = new RadioButton { Text = "Derecho", Location = new Point(15, 46), AutoSize = true };
            radAutoMiddle = new RadioButton { Text = "Medio (rueda)", Location = new Point(15, 70), AutoSize = true };
            chkAutoDoubleClick = new CheckBox { Text = "Doble click", Location = new Point(140, 22), AutoSize = true };
            grpButton.Controls.Add(radAutoLeft);
            grpButton.Controls.Add(radAutoRight);
            grpButton.Controls.Add(radAutoMiddle);
            grpButton.Controls.Add(chkAutoDoubleClick);

            var grpInterval = new GroupBox { Text = "Intervalo entre clicks", Location = new Point(274, 12), Size = new Size(270, 90) };
            numAutoHours = new NumericUpDown { Location = new Point(15, 25), Width = 50, Maximum = 23 };
            numAutoMinutes = new NumericUpDown { Location = new Point(80, 25), Width = 50, Maximum = 59 };
            numAutoSeconds = new NumericUpDown { Location = new Point(145, 25), Width = 50, Maximum = 59, Value = 1 };
            numAutoMillis = new NumericUpDown { Location = new Point(15, 55), Width = 65, Maximum = 999 };
            grpInterval.Controls.Add(new Label { Text = "hs", Location = new Point(15, 8), AutoSize = true });
            grpInterval.Controls.Add(new Label { Text = "min", Location = new Point(80, 8), AutoSize = true });
            grpInterval.Controls.Add(new Label { Text = "seg", Location = new Point(145, 8), AutoSize = true });
            grpInterval.Controls.Add(new Label { Text = "ms", Location = new Point(85, 58), AutoSize = true });
            grpInterval.Controls.Add(numAutoHours);
            grpInterval.Controls.Add(numAutoMinutes);
            grpInterval.Controls.Add(numAutoSeconds);
            grpInterval.Controls.Add(numAutoMillis);

            var grpPos = new GroupBox { Text = "Posición del click", Location = new Point(12, 112), Size = new Size(532, 90) };
            radAutoCurrentPos = new RadioButton { Text = "Posición actual del cursor", Location = new Point(15, 22), Checked = true, AutoSize = true };
            radAutoFixedPos = new RadioButton { Text = "Posición fija:", Location = new Point(15, 46), AutoSize = true };
            btnAutoCapturePos = new Button { Text = "Capturar posición (3s)", Location = new Point(140, 43), Width = 170 };
            lblAutoCapturedPos = new Label { Text = "Sin capturar.", Location = new Point(320, 48), AutoSize = true };
            btnAutoCapturePos.Click += btnAutoCapturePos_Click;
            grpPos.Controls.Add(radAutoCurrentPos);
            grpPos.Controls.Add(radAutoFixedPos);
            grpPos.Controls.Add(btnAutoCapturePos);
            grpPos.Controls.Add(lblAutoCapturedPos);

            var grpRepeat = new GroupBox { Text = "Repetición", Location = new Point(12, 210), Size = new Size(532, 70) };
            radAutoUntilStopped = new RadioButton { Text = "Hasta detener (F6)", Location = new Point(15, 25), Checked = true, AutoSize = true };
            radAutoCount = new RadioButton { Text = "Cantidad de clicks:", Location = new Point(190, 25), AutoSize = true };
            numAutoCount = new NumericUpDown { Location = new Point(340, 23), Width = 80, Maximum = 1000000, Minimum = 1, Value = 10 };
            grpRepeat.Controls.Add(radAutoUntilStopped);
            grpRepeat.Controls.Add(radAutoCount);
            grpRepeat.Controls.Add(numAutoCount);

            btnAutoStart = new Button { Text = "Iniciar (F6)", Location = new Point(12, 292), Size = new Size(150, 34) };
            btnAutoStart.Click += btnAutoStart_Click;
            lblAutoStatus = new Label { Text = "Detenido.", Location = new Point(175, 300), AutoSize = true };

            var note = new Label
            {
                Location = new Point(12, 335),
                Size = new Size(532, 60),
                ForeColor = SystemColors.GrayText,
                Text = "Nota: los clicks se simulan con SendInput a nivel de sistema operativo, más confiable que la posición del mouse por software. Algunos juegos con anti-cheat de kernel (EAC, BattlEye, Vanguard) bloquean intencionalmente esta técnica."
            };

            tab.Controls.Add(grpButton);
            tab.Controls.Add(grpInterval);
            tab.Controls.Add(grpPos);
            tab.Controls.Add(grpRepeat);
            tab.Controls.Add(btnAutoStart);
            tab.Controls.Add(lblAutoStatus);
            tab.Controls.Add(note);
        }

        private void BuildMacroTab(TabPage tab)
        {
            btnRecord = new Button { Text = "Grabar (F8)", Location = new Point(12, 15), Size = new Size(170, 34) };
            btnRecord.Click += btnRecord_Click;
            btnPlay = new Button { Text = "Reproducir (F9)", Location = new Point(192, 15), Size = new Size(170, 34), Enabled = false };
            btnPlay.Click += btnPlay_Click;

            lblMacroInfo = new Label { Location = new Point(12, 58), Size = new Size(530, 40), Text = "Sin grabar ni cargar ninguna macro." };

            var grpPlay = new GroupBox { Text = "Reproducción", Location = new Point(12, 105), Size = new Size(532, 100) };
            radPlayOnce = new RadioButton { Text = "Una vez", Location = new Point(15, 22), Checked = true, AutoSize = true };
            radPlayCount = new RadioButton { Text = "N veces:", Location = new Point(110, 22), AutoSize = true };
            numPlayCount = new NumericUpDown { Location = new Point(200, 20), Width = 70, Minimum = 1, Maximum = 100000, Value = 5 };
            radPlayUntilStopped = new RadioButton { Text = "Hasta detener (F9)", Location = new Point(290, 22), AutoSize = true };
            grpPlay.Controls.Add(new Label { Text = "Velocidad (x):", Location = new Point(15, 55), AutoSize = true });
            numPlaySpeed = new NumericUpDown
            {
                Location = new Point(115, 53),
                Width = 70,
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

            btnSaveMacro = new Button { Text = "Guardar macro...", Location = new Point(12, 215), Size = new Size(150, 30), Enabled = false };
            btnSaveMacro.Click += btnSaveMacro_Click;
            btnLoadMacro = new Button { Text = "Cargar macro...", Location = new Point(172, 215), Size = new Size(150, 30) };
            btnLoadMacro.Click += btnLoadMacro_Click;

            var note = new Label
            {
                Location = new Point(12, 260),
                Size = new Size(532, 90),
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
            lstPositions = new ListBox { Location = new Point(12, 15), Size = new Size(200, 230) };

            btnSeqAddPos = new Button { Text = "Agregar posición (3s)", Location = new Point(224, 15), Size = new Size(170, 30) };
            btnSeqAddPos.Click += btnSeqAddPos_Click;
            btnSeqRemovePos = new Button { Text = "Quitar seleccionada", Location = new Point(224, 50), Size = new Size(170, 30) };
            btnSeqRemovePos.Click += btnSeqRemovePos_Click;
            btnSeqClearPos = new Button { Text = "Limpiar todas", Location = new Point(224, 85), Size = new Size(170, 30) };
            btnSeqClearPos.Click += btnSeqClearPos_Click;
            lblSeqCaptureStatus = new Label { Location = new Point(224, 120), Size = new Size(300, 40), Text = "" };

            var lblButtonType = new Label { Text = "Tipo de click:", Location = new Point(224, 165), AutoSize = true };
            cmbSeqButton = new ComboBox { Location = new Point(310, 162), Width = 110, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSeqButton.Items.AddRange(new object[] { "Izquierdo", "Derecho", "Medio" });
            cmbSeqButton.SelectedIndex = 0;

            tab.Controls.Add(lstPositions);
            tab.Controls.Add(btnSeqAddPos);
            tab.Controls.Add(btnSeqRemovePos);
            tab.Controls.Add(btnSeqClearPos);
            tab.Controls.Add(lblSeqCaptureStatus);
            tab.Controls.Add(lblButtonType);
            tab.Controls.Add(cmbSeqButton);

            var grpInterval = new GroupBox { Text = "Intervalo entre clicks", Location = new Point(12, 255), Size = new Size(200, 60) };
            numSeqInterval = new NumericUpDown { Location = new Point(15, 25), Width = 90, Minimum = 10, Maximum = 3600000, Value = 500 };
            grpInterval.Controls.Add(numSeqInterval);
            grpInterval.Controls.Add(new Label { Text = "ms", Location = new Point(110, 27), AutoSize = true });

            var grpRepeat = new GroupBox { Text = "Repetición", Location = new Point(224, 255), Size = new Size(320, 65) };
            radSeqOnce = new RadioButton { Text = "Una vuelta", Location = new Point(10, 20), Checked = true, AutoSize = true };
            radSeqLoops = new RadioButton { Text = "N vueltas:", Location = new Point(100, 20), AutoSize = true };
            numSeqLoops = new NumericUpDown { Location = new Point(180, 18), Width = 60, Minimum = 1, Maximum = 100000, Value = 5 };
            radSeqUntilStopped = new RadioButton { Text = "Hasta detener (F7)", Location = new Point(10, 40), AutoSize = true };
            grpRepeat.Controls.Add(radSeqOnce);
            grpRepeat.Controls.Add(radSeqLoops);
            grpRepeat.Controls.Add(numSeqLoops);
            grpRepeat.Controls.Add(radSeqUntilStopped);

            btnSeqStart = new Button { Text = "Iniciar (F7)", Location = new Point(12, 325), Size = new Size(150, 34) };
            btnSeqStart.Click += btnSeqStart_Click;
            lblSeqStatus = new Label { Location = new Point(175, 333), Size = new Size(360, 20), Text = "Detenido." };

            tab.Controls.Add(grpInterval);
            tab.Controls.Add(grpRepeat);
            tab.Controls.Add(btnSeqStart);
            tab.Controls.Add(lblSeqStatus);
        }
    }
}
