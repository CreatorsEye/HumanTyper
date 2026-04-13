using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Collections.Generic;

namespace CeAutoTyper
{
    public class MainForm : Form
    {
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int GWL_STYLE = -16;
        private const int WS_CAPTION = 0x00C00000;
        private const int WS_THICKFRAME = 0x00040000;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        private const int HOTKEY_START = 1;
        private const int HOTKEY_PAUSE = 2;
        private const int HOTKEY_RESUME = 3;
        private const int HOTKEY_STOP = 4;

        private Panel titleBar;
        private Label lblTitle;
        private Button btnMinimize, btnClose;

        private RichTextBox txtInput;
        private ComboBox cmbSpeed;
        private ComboBox cmbMistakes;
        private ComboBox cmbFixTypo;
        private Button btnStart, btnStop, btnPause, btnSetHotkeys;
        private RichTextBox txtShell;
        private Label lblHotkeys;
        private TypingEngine engine;
        private NotifyIcon trayIcon;
        private bool isTyping = false;
        private Random rand = new Random();

        private Dictionary<string, Keys> hotkeys = new Dictionary<string, Keys>
        {
            { "Start", Keys.F5 },
            { "Pause", Keys.F8 },
            { "Resume", Keys.F9 },
            { "Stop", Keys.F10 }
        };

        public MainForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.DoubleBuffered = true;
            
            SetWindowLong(this.Handle, GWL_STYLE, GetWindowLong(this.Handle, GWL_STYLE) & ~WS_CAPTION & ~WS_THICKFRAME);
            
            if (Environment.OSVersion.Version.Major >= 10)
            {
                int useDarkMode = 1;
                DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDarkMode, sizeof(int));
            }

            LoadHotkeys();

            try
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CeAutoTyper.ce.ico"))
                {
                    if (stream != null) this.Icon = new Icon(stream);
                }
            }
            catch { }

            this.Text = "CE Auto Typer v2.0";
            this.Size = new Size(720, 640);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(18, 18, 22);
            this.MinimumSize = new Size(680, 580);

            try
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CeAutoTyper.ce.ico"))
                {
                    if (stream != null)
                    {
                        trayIcon = new NotifyIcon();
                        trayIcon.Icon = new Icon(stream);
                        trayIcon.Text = "CE Auto Typer v2.0";
                        trayIcon.Visible = true;
                        trayIcon.DoubleClick += (s, e) => { this.Show(); this.WindowState = FormWindowState.Normal; };
                    }
                }
            }
            catch { }

            InitializeCustomTitleBar();
            InitializeContent();
            RegisterHotkeys();
        }

        private void InitializeCustomTitleBar()
        {
            titleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = Color.FromArgb(28, 28, 35)
            };
            titleBar.MouseDown += TitleBar_MouseDown;

            lblTitle = new Label
            {
                Text = "CE Auto Typer v2.0",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 10),
                AutoSize = true
            };
            lblTitle.MouseDown += TitleBar_MouseDown;

            btnMinimize = new Button
            {
                Text = "─",
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Size = new Size(45, 45),
                Location = new Point(this.Width - 90, 0),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

            btnClose = new Button
            {
                Text = "✕",
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Size = new Size(45, 45),
                Location = new Point(this.Width - 45, 0),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Application.Exit();

            titleBar.Controls.Add(lblTitle);
            titleBar.Controls.Add(btnMinimize);
            titleBar.Controls.Add(btnClose);
            this.Controls.Add(titleBar);
        }

        private void InitializeContent()
        {
            int yOffset = 60;

            // Text input
            Label lblInput = new Label
            {
                Text = "Text to Type",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(180, 180, 190),
                Location = new Point(20, yOffset),
                AutoSize = true
            };
            this.Controls.Add(lblInput);

            txtInput = new RichTextBox
            {
                Location = new Point(20, yOffset + 25),
                Size = new Size(680, 120),
                Font = new Font("Consolas", 11),
                BackColor = Color.FromArgb(35, 35, 42),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                Text = "Line 1\nLine 2\nLine 3\nLine 4"
            };
            this.Controls.Add(txtInput);

            yOffset += 170;

            // Settings panel
            Panel settingsPanel = new Panel
            {
                Location = new Point(20, yOffset),
                Size = new Size(680, 50),
                BackColor = Color.FromArgb(28, 28, 35)
            };

            // Speed dropdown
            Label lblSpeed = new Label
            {
                Text = "Speed:",
                ForeColor = Color.White,
                Location = new Point(12, 14),
                Size = new Size(45, 22),
                Font = new Font("Segoe UI", 9)
            };
            settingsPanel.Controls.Add(lblSpeed);

            cmbSpeed = new ComboBox();
            cmbSpeed.Location = new Point(55, 12);
            cmbSpeed.Size = new Size(100, 24);
            cmbSpeed.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSpeed.FlatStyle = FlatStyle.Flat;
            cmbSpeed.BackColor = Color.FromArgb(45, 45, 52);
            cmbSpeed.ForeColor = Color.White;
            cmbSpeed.Font = new Font("Segoe UI", 9);
            cmbSpeed.Items.AddRange(new string[] { "Very Slow", "Slow", "Medium", "Fast", "Very Fast" });
            cmbSpeed.SelectedIndex = 2;
            settingsPanel.Controls.Add(cmbSpeed);

            // Mistakes dropdown
            Label lblMistakes = new Label
            {
                Text = "Mistakes:",
                ForeColor = Color.White,
                Location = new Point(170, 14),
                Size = new Size(55, 22),
                Font = new Font("Segoe UI", 9)
            };
            settingsPanel.Controls.Add(lblMistakes);

            cmbMistakes = new ComboBox();
            cmbMistakes.Location = new Point(225, 12);
            cmbMistakes.Size = new Size(100, 24);
            cmbMistakes.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMistakes.FlatStyle = FlatStyle.Flat;
            cmbMistakes.BackColor = Color.FromArgb(45, 45, 52);
            cmbMistakes.ForeColor = Color.White;
            cmbMistakes.Font = new Font("Segoe UI", 9);
            cmbMistakes.Items.AddRange(new string[] { "None", "Few", "Some", "Many", "Lots", "Random" });
            cmbMistakes.SelectedIndex = 0;
            cmbMistakes.SelectedIndexChanged += (s, e) => UpdateFixTypoState();
            settingsPanel.Controls.Add(cmbMistakes);

            // Fix Typo dropdown
            Label lblFixTypo = new Label
            {
                Text = "Fix Typo:",
                ForeColor = Color.White,
                Location = new Point(340, 14),
                Size = new Size(55, 22),
                Font = new Font("Segoe UI", 9)
            };
            settingsPanel.Controls.Add(lblFixTypo);

            cmbFixTypo = new ComboBox();
            cmbFixTypo.Location = new Point(395, 12);
            cmbFixTypo.Size = new Size(80, 24);
            cmbFixTypo.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFixTypo.FlatStyle = FlatStyle.Flat;
            cmbFixTypo.BackColor = Color.FromArgb(45, 45, 52);
            cmbFixTypo.ForeColor = Color.White;
            cmbFixTypo.Font = new Font("Segoe UI", 9);
            cmbFixTypo.Items.AddRange(new string[] { "Yes", "No" });
            cmbFixTypo.SelectedIndex = 0;
            settingsPanel.Controls.Add(cmbFixTypo);

            // Set Hotkeys button
            btnSetHotkeys = new Button
            {
                Text = "⚙ Hotkeys",
                Location = new Point(500, 10),
                Size = new Size(160, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(60, 60, 70),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnSetHotkeys.FlatAppearance.BorderSize = 0;
            btnSetHotkeys.Click += BtnSetHotkeys_Click!;
            settingsPanel.Controls.Add(btnSetHotkeys);

            this.Controls.Add(settingsPanel);
            yOffset += 60;

            // Hotkeys display
            lblHotkeys = new Label
            {
                Location = new Point(20, yOffset),
                Size = new Size(450, 25),
                ForeColor = Color.FromArgb(150, 150, 160),
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleLeft
            };
            UpdateHotkeyDisplay();
            this.Controls.Add(lblHotkeys);

            yOffset += 30;

            // Control buttons
            btnStart = new Button
            {
                Text = "▶ START TYPING",
                Location = new Point(20, yOffset),
                Size = new Size(160, 45),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnStart.FlatAppearance.BorderSize = 0;
            btnStart.Click += BtnStart_Click!;
            this.Controls.Add(btnStart);

            btnStop = new Button
            {
                Text = "■ STOP",
                Location = new Point(190, yOffset),
                Size = new Size(100, 45),
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Visible = false
            };
            btnStop.FlatAppearance.BorderSize = 0;
            btnStop.Click += (s, e) => { engine.Stop(); ResetUI(); };
            this.Controls.Add(btnStop);

            btnPause = new Button
            {
                Text = "⏸ PAUSE",
                Location = new Point(300, yOffset),
                Size = new Size(110, 45),
                BackColor = Color.FromArgb(245, 158, 11),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Visible = false
            };
            btnPause.FlatAppearance.BorderSize = 0;
            btnPause.Click += (s, e) => 
            { 
                engine.Paused = !engine.Paused;
                btnPause.Text = engine.Paused ? "▶ RESUME" : "⏸ PAUSE";
                txtShell.Text = engine.Paused ? "⏸ Paused" : "▶ Typing...";
            };
            this.Controls.Add(btnPause);

            yOffset += 55;

            // Shell output
            Label lblOutput = new Label
            {
                Text = "Console Output",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(180, 180, 190),
                Location = new Point(20, yOffset),
                AutoSize = true
            };
            this.Controls.Add(lblOutput);

            txtShell = new RichTextBox
            {
                Location = new Point(20, yOffset + 25),
                Size = new Size(680, 200),
                Font = new Font("Consolas", 10),
                BackColor = Color.Black,
                ForeColor = Color.Lime,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                Text = "Ready."
            };
            this.Controls.Add(txtShell);

            engine = new TypingEngine();
        }

        private void UpdateFixTypoState()
        {
            bool mistakesNone = (cmbMistakes.SelectedIndex == 0);
            cmbFixTypo.Enabled = !mistakesNone;
            if (mistakesNone)
                cmbFixTypo.SelectedIndex = 0;
        }

        private int GetMistakeCount()
        {
            switch (cmbMistakes.SelectedIndex)
            {
                case 0: return 0;
                case 1: return rand.Next(1, 3);
                case 2: return rand.Next(3, 5);
                case 3: return rand.Next(5, 8);
                case 4: return rand.Next(8, 11);
                case 5: return rand.Next(1, 11);
                default: return 0;
            }
        }

        private bool GetFixTypo()
        {
            return cmbFixTypo.SelectedIndex == 0;
        }

        private void UpdateHotkeyDisplay()
        {
            lblHotkeys.Text = $"⌨ Hotkeys: {hotkeys["Start"]}=Start | {hotkeys["Pause"]}=Pause | {hotkeys["Resume"]}=Resume | {hotkeys["Stop"]}=Stop";
        }

        private void LoadHotkeys()
        {
            try
            {
                string path = Path.Combine(Application.StartupPath, "hotkeys.cfg");
                if (File.Exists(path))
                {
                    string[] lines = File.ReadAllLines(path);
                    foreach (string line in lines)
                    {
                        string[] parts = line.Split('=');
                        if (parts.Length == 2 && hotkeys.ContainsKey(parts[0]))
                        {
                            if (Enum.TryParse(parts[1], out Keys key))
                                hotkeys[parts[0]] = key;
                        }
                    }
                }
            }
            catch { }
        }

        private void SaveHotkeys()
        {
            try
            {
                string path = Path.Combine(Application.StartupPath, "hotkeys.cfg");
                List<string> lines = new List<string>();
                foreach (var kv in hotkeys)
                    lines.Add($"{kv.Key}={kv.Value}");
                File.WriteAllLines(path, lines);
            }
            catch { }
        }

        private void BtnSetHotkeys_Click(object sender, EventArgs e)
        {
            using (var hotkeyForm = new Form())
            {
                hotkeyForm.Text = "Set Hotkeys";
                hotkeyForm.Size = new Size(350, 300);
                hotkeyForm.StartPosition = FormStartPosition.CenterParent;
                hotkeyForm.BackColor = Color.FromArgb(28, 28, 35);
                hotkeyForm.ForeColor = Color.White;
                hotkeyForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                hotkeyForm.MaximizeBox = false;
                hotkeyForm.MinimizeBox = false;
                hotkeyForm.TopMost = true;

                FlowLayoutPanel panel = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20) };
                hotkeyForm.Controls.Add(panel);

                Dictionary<string, ComboBox> combos = new Dictionary<string, ComboBox>();

                foreach (var key in hotkeys.Keys)
                {
                    Label lbl = new Label 
                    { 
                        Text = $"{key}:", 
                        ForeColor = Color.White, 
                        Width = 80,
                        Font = new Font("Segoe UI", 10)
                    };
                    
                    ComboBox cmb = new ComboBox();
                    cmb.Width = 150;
                    cmb.DropDownStyle = ComboBoxStyle.DropDownList;
                    cmb.FlatStyle = FlatStyle.Flat;
                    cmb.BackColor = Color.FromArgb(45, 45, 52);
                    cmb.ForeColor = Color.White;
                    
                    foreach (Keys k in Enum.GetValues(typeof(Keys)))
                    {
                        cmb.Items.Add(k);
                    }
                    cmb.SelectedItem = hotkeys[key];
                    combos[key] = cmb;
                    
                    panel.Controls.Add(lbl);
                    panel.Controls.Add(cmb);
                }

                Button btnSave = new Button 
                { 
                    Text = "SAVE & APPLY", 
                    Dock = DockStyle.Bottom, 
                    Height = 40, 
                    BackColor = Color.FromArgb(34, 197, 94), 
                    ForeColor = Color.White, 
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnSave.FlatAppearance.BorderSize = 0;
                btnSave.Click += (s, ev) => 
                {
                    foreach (var key in hotkeys.Keys)
                    {
                        hotkeys[key] = (Keys)combos[key].SelectedItem;
                    }
                    SaveHotkeys();
                    
                    UnregisterHotkeys();
                    RegisterHotkeys();
                    UpdateHotkeyDisplay();
                    
                    hotkeyForm.Close();
                };
                hotkeyForm.Controls.Add(btnSave);
                hotkeyForm.ShowDialog();
            }
        }

        private void RegisterHotkeys()
        {
            try
            {
                NativeWinMethods.RegisterHotKey(this.Handle, HOTKEY_START, 0, (uint)hotkeys["Start"]);
                NativeWinMethods.RegisterHotKey(this.Handle, HOTKEY_PAUSE, 0, (uint)hotkeys["Pause"]);
                NativeWinMethods.RegisterHotKey(this.Handle, HOTKEY_RESUME, 0, (uint)hotkeys["Resume"]);
                NativeWinMethods.RegisterHotKey(this.Handle, HOTKEY_STOP, 0, (uint)hotkeys["Stop"]);
            }
            catch (Exception ex)
            {
                txtShell.Text = $"Hotkey error: {ex.Message}";
            }
        }

        private void UnregisterHotkeys()
        {
            NativeWinMethods.UnregisterHotKey(this.Handle, HOTKEY_START);
            NativeWinMethods.UnregisterHotKey(this.Handle, HOTKEY_PAUSE);
            NativeWinMethods.UnregisterHotKey(this.Handle, HOTKEY_RESUME);
            NativeWinMethods.UnregisterHotKey(this.Handle, HOTKEY_STOP);
        }

        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312)
            {
                int id = m.WParam.ToInt32();
                switch (id)
                {
                    case HOTKEY_START:
                        if (!isTyping && btnStart.Visible)
                            this.Invoke(new Action(() => BtnStart_Click(null, null)));
                        break;
                    case HOTKEY_PAUSE:
                        if (isTyping && !engine.Paused && btnPause.Visible)
                            this.Invoke(new Action(() => { engine.Paused = true; btnPause.Text = "▶ RESUME"; txtShell.Text = "⏸ Paused"; }));
                        break;
                    case HOTKEY_RESUME:
                        if (isTyping && engine.Paused && btnPause.Visible)
                            this.Invoke(new Action(() => { engine.Paused = false; btnPause.Text = "⏸ PAUSE"; txtShell.Text = "▶ Typing..."; }));
                        break;
                    case HOTKEY_STOP:
                        if (isTyping)
                            this.Invoke(new Action(() => { engine.Stop(); ResetUI(); }));
                        break;
                }
            }
            base.WndProc(ref m);
        }

        private async void BtnStart_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtInput.Text))
            {
                txtShell.Text = "Error: No text to type.";
                return;
            }

            this.TopMost = true;

            isTyping = true;
            engine.StopTyping = false;
            engine.Paused = false;
            
            btnStart.Visible = false;
            btnStop.Visible = true;
            btnPause.Visible = true;
            btnPause.Text = "⏸ PAUSE";

            for (int i = 5; i > 0; i--)
            {
                if (engine.StopTyping) break;
                txtShell.Text = $"Starting in {i}...";
                await Task.Delay(1000);
            }

            if (!engine.StopTyping)
            {
                txtShell.Text = "▶ Typing...";
                int speedIndex = cmbSpeed.SelectedIndex;
                int mistakeCount = GetMistakeCount();
                bool fixTypo = GetFixTypo();
                engine.SetSpeed(speedIndex);
                engine.SetMistakeCount(mistakeCount, fixTypo);
                await engine.TypeTextAsync(txtInput.Text);
            }

            ResetUI();
        }

        private void ResetUI()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(ResetUI));
                return;
            }
            
            this.TopMost = false;
            
            isTyping = false;
            btnStart.Visible = true;
            btnStop.Visible = false;
            btnPause.Visible = false;
            
            if (engine.StopTyping)
                txtShell.Text = "■ Stopped!";
            else if (!string.IsNullOrWhiteSpace(txtInput.Text))
                txtShell.Text = "✓ Complete!";
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            
            if (btnMinimize != null && btnClose != null)
            {
                btnMinimize.Location = new Point(this.ClientSize.Width - 90, 0);
                btnClose.Location = new Point(this.ClientSize.Width - 45, 0);
            }
            
            if (this.WindowState == FormWindowState.Minimized && trayIcon != null)
            {
                this.Hide();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            UnregisterHotkeys();
            trayIcon?.Dispose();
            engine.Stop();
            base.OnFormClosing(e);
        }

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;
    }

    // =========================================================
    // TYPING ENGINE
    // =========================================================
    public class TypingEngine
    {
        private readonly Random rand = new();
        private bool stopRequested = false;
        private bool paused = false;
        private int currentMinDelay = 80;
        private int currentMaxDelay = 160;
        private int targetMistakes = 0;
        private int mistakesMade = 0;
        private bool fixTypo = true;

        public bool Paused
        {
            get => paused;
            set => paused = value;
        }

        public bool StopTyping
        {
            get => stopRequested;
            set => stopRequested = value;
        }

        public void SetSpeed(int speedIndex)
        {
            switch (speedIndex)
            {
                case 0: currentMinDelay = 250; currentMaxDelay = 450; break;
                case 1: currentMinDelay = 150; currentMaxDelay = 250; break;
                case 2: currentMinDelay = 80; currentMaxDelay = 160; break;
                case 3: currentMinDelay = 40; currentMaxDelay = 90; break;
                case 4: currentMinDelay = 15; currentMaxDelay = 45; break;
                default: currentMinDelay = 80; currentMaxDelay = 160; break;
            }
        }

        public void SetMistakeCount(int count, bool fix)
        {
            targetMistakes = count;
            mistakesMade = 0;
            fixTypo = fix;
        }

        private int GetDelay() => rand.Next(currentMinDelay, currentMaxDelay);

        private string MakeTypo(string word)
        {
            if (word.Length <= 2) return word;
            int type = rand.Next(1, 4);
            string typo = word;
            switch (type)
            {
                case 1:
                    int pos1 = rand.Next(0, word.Length);
                    typo = word.Insert(pos1, word[pos1].ToString());
                    break;
                case 2:
                    int pos2 = rand.Next(0, word.Length - 1);
                    typo = word.Remove(pos2, 1);
                    break;
                case 3:
                    int pos3 = rand.Next(0, word.Length - 2);
                    char c1 = word[pos3];
                    char c2 = word[pos3 + 1];
                    typo = word.Remove(pos3, 2).Insert(pos3, $"{c2}{c1}");
                    break;
            }
            return typo;
        }

        public async Task TypeTextAsync(string text)
        {
            stopRequested = false;
            mistakesMade = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (stopRequested) break;
                while (paused && !stopRequested) await Task.Delay(50);
                if (stopRequested) break;

                char c = text[i];

                if (c == '\n')
                {
                    SendEnterKey();
                    await Task.Delay(GetDelay());
                    continue;
                }
                if (c == '\r') continue;

                bool makeMistake = targetMistakes > 0 && mistakesMade < targetMistakes && rand.Next(100) < 40 && char.IsLetter(c);
                if (makeMistake && i < text.Length - 1)
                {
                    int start = i, end = i;
                    while (start > 0 && char.IsLetter(text[start - 1])) start--;
                    while (end < text.Length - 1 && char.IsLetter(text[end + 1])) end++;
                    string word = text.Substring(start, end - start + 1);
                    if (word.Length > 2)
                    {
                        string typo = MakeTypo(word);
                        
                        foreach (char tc in typo)
                        {
                            if (stopRequested) break;
                            SendUnicodeChar(tc);
                            await Task.Delay(GetDelay());
                        }
                        
                        if (!stopRequested && fixTypo)
                        {
                            await Task.Delay(rand.Next(300, 600));
                            
                            for (int b = 0; b < typo.Length; b++)
                            {
                                SendBackspace();
                                await Task.Delay(rand.Next(80, 150));
                            }
                            
                            await Task.Delay(rand.Next(150, 300));
                            
                            foreach (char wc in word)
                            {
                                if (stopRequested) break;
                                SendUnicodeChar(wc);
                                await Task.Delay(GetDelay());
                            }
                        }
                        
                        mistakesMade++;
                        i = end;
                        continue;
                    }
                }

                SendUnicodeChar(c);
                await Task.Delay(GetDelay());
            }
        }

        private void SendBackspace()
        {
            var inputs = new INPUT[2];
            inputs[0] = CreateKeyInput(0x08, 0x0000);
            inputs[1] = CreateKeyInput(0x08, 0x0002);
            SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        private void SendEnterKey()
        {
            var inputs = new INPUT[2];
            inputs[0] = CreateKeyInput(0x0D, 0x0000);
            inputs[1] = CreateKeyInput(0x0D, 0x0002);
            SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        private INPUT CreateKeyInput(ushort vk, uint flags)
        {
            return new INPUT
            {
                type = 1,
                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = vk,
                        wScan = 0,
                        dwFlags = flags,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };
        }

        public void Stop() => stopRequested = true;

        private void SendUnicodeChar(char ch)
        {
            var inputs = new INPUT[2];
            inputs[0] = CreateKeyboardInput(ch, 0x0004);
            inputs[1] = CreateKeyboardInput(ch, 0x0004 | 0x0002);
            SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        private INPUT CreateKeyboardInput(char ch, uint flags) => new INPUT
        {
            type = 1,
            U = new InputUnion { ki = new KEYBDINPUT { wScan = ch, dwFlags = flags } }
        };

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT { public uint type; public InputUnion U; }
        [StructLayout(LayoutKind.Explicit, Size = 32)]
        private struct InputUnion { [FieldOffset(0)] public KEYBDINPUT ki; }
        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT { public ushort wVk; public ushort wScan; public uint dwFlags; public uint time; public IntPtr dwExtraInfo; }

        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
    }

    internal static class NativeWinMethods
    {
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}