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
        private RichTextBox txtInput;
        private ComboBox cmbSpeed;
        private Button btnStart, btnStop, btnPause, btnSetHotkeys;
        private RichTextBox txtShell;
        private Label lblHotkeys;
        private TypingEngine engine;
        private NotifyIcon trayIcon;
        private bool isTyping = false;

        private const int HOTKEY_START = 1;
        private const int HOTKEY_PAUSE = 2;
        private const int HOTKEY_RESUME = 3;
        private const int HOTKEY_STOP = 4;

        private Dictionary<string, Keys> hotkeys = new Dictionary<string, Keys>
        {
            { "Start", Keys.F5 },
            { "Pause", Keys.F8 },
            { "Resume", Keys.F9 },
            { "Stop", Keys.F10 }
        };

        public MainForm()
        {
            LoadHotkeys();

            try
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CeAutoTyper.ce.ico"))
                {
                    if (stream != null) this.Icon = new Icon(stream);
                }
            }
            catch { }

            this.Text = "CE Auto Typer v1.5";
            this.Size = new Size(600, 560);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 35);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.ShowIcon = true;

            try
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CeAutoTyper.ce.ico"))
                {
                    if (stream != null)
                    {
                        trayIcon = new NotifyIcon();
                        trayIcon.Icon = new Icon(stream);
                        trayIcon.Text = "CE Auto Typer v1.5";
                        trayIcon.Visible = true;
                        trayIcon.DoubleClick += (s, e) => { this.Show(); this.WindowState = FormWindowState.Normal; };
                    }
                }
            }
            catch { }

            txtInput = new RichTextBox
            {
                Location = new Point(12, 12),
                Size = new Size(560, 120),
                Font = new Font("Consolas", 11),
                BackColor = Color.FromArgb(45, 45, 50),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Text = "Line 1\nLine 2\nLine 3\nLine 4"
            };

            // Speed dropdown
            Label lblSpeed = new Label
            {
                Text = "Speed:",
                Location = new Point(12, 145),
                Size = new Size(50, 25),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10)
            };

            cmbSpeed = new ComboBox
            {
                Location = new Point(65, 143),
                Size = new Size(120, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(45, 45, 50),
                ForeColor = Color.White
            };
            cmbSpeed.Items.AddRange(new string[] { "Very Slow", "Slow", "Medium", "Fast", "Very Fast" });
            cmbSpeed.SelectedIndex = 2;

            // Hotkeys label (moved down to Y=175)
            lblHotkeys = new Label
            {
                Location = new Point(12, 175),
                Size = new Size(370, 25),
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleLeft
            };
            UpdateHotkeyDisplay();

            // Set Hotkeys button (aligned with label)
            btnSetHotkeys = new Button
            {
                Text = "Set Hotkeys",
                Location = new Point(450, 173),
                Size = new Size(120, 27),
                BackColor = Color.FromArgb(75, 85, 99),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnSetHotkeys.FlatAppearance.BorderSize = 0;
            btnSetHotkeys.Click += BtnSetHotkeys_Click!;

            // Control buttons (moved down to Y=210)
            btnStart = new Button
            {
                Text = "START TYPING",
                Location = new Point(12, 210),
                Size = new Size(140, 40),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnStart.FlatAppearance.BorderSize = 0;
            btnStart.Click += BtnStart_Click!;

            btnStop = new Button
            {
                Text = "STOP",
                Location = new Point(160, 210),
                Size = new Size(100, 40),
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Visible = false
            };
            btnStop.FlatAppearance.BorderSize = 0;
            btnStop.Click += (s, e) => 
            { 
                engine.Stop();
                ResetUI();
            };

            btnPause = new Button
            {
                Text = "PAUSE",
                Location = new Point(270, 210),
                Size = new Size(100, 40),
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
                btnPause.Text = engine.Paused ? "RESUME" : "PAUSE";
                txtShell.Text = engine.Paused ? "Paused" : "Typing...";
            };

            // Shell output (moved down to Y=265)
            txtShell = new RichTextBox
            {
                Location = new Point(12, 265),
                Size = new Size(560, 210),
                Font = new Font("Consolas", 10),
                BackColor = Color.Black,
                ForeColor = Color.Lime,
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true,
                Text = "Ready."
            };

            Controls.Add(txtInput);
            Controls.Add(lblSpeed);
            Controls.Add(cmbSpeed);
            Controls.Add(lblHotkeys);
            Controls.Add(btnSetHotkeys);
            Controls.Add(btnStart);
            Controls.Add(btnStop);
            Controls.Add(btnPause);
            Controls.Add(txtShell);

            engine = new TypingEngine();
            RegisterHotkeys();
        }

        private void UpdateHotkeyDisplay()
        {
            lblHotkeys.Text = $"Hotkeys: {hotkeys["Start"]}=Start | {hotkeys["Pause"]}=Pause | {hotkeys["Resume"]}=Resume | {hotkeys["Stop"]}=Stop";
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
                hotkeyForm.BackColor = Color.FromArgb(31, 41, 55);
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
                    
                    ComboBox cmb = new ComboBox 
                    { 
                        Width = 150, 
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        BackColor = Color.FromArgb(45, 45, 50),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat
                    };
                    
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
                            this.Invoke(new Action(() => { engine.Paused = true; btnPause.Text = "RESUME"; txtShell.Text = "Paused"; }));
                        break;
                    case HOTKEY_RESUME:
                        if (isTyping && engine.Paused && btnPause.Visible)
                            this.Invoke(new Action(() => { engine.Paused = false; btnPause.Text = "PAUSE"; txtShell.Text = "Typing..."; }));
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

            isTyping = true;
            engine.StopTyping = false;
            engine.Paused = false;
            
            btnStart.Visible = false;
            btnStop.Visible = true;
            btnPause.Visible = true;
            btnPause.Text = "PAUSE";

            for (int i = 5; i > 0; i--)
            {
                if (engine.StopTyping) break;
                txtShell.Text = $"Starting in {i}...";
                await Task.Delay(1000);
            }

            if (!engine.StopTyping)
            {
                txtShell.Text = "Typing...";
                int speedIndex = cmbSpeed.SelectedIndex;
                engine.SetSpeed(speedIndex);
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
            
            isTyping = false;
            btnStart.Visible = true;
            btnStop.Visible = false;
            btnPause.Visible = false;
            
            if (engine.StopTyping)
                txtShell.Text = "Stopped!";
            else if (!string.IsNullOrWhiteSpace(txtInput.Text))
                txtShell.Text = "Complete!";
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
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
    }

    // TypingEngine class unchanged (no mistakes)
    public class TypingEngine
    {
        private readonly Random rand = new();
        private bool stopRequested = false;
        private bool paused = false;
        private int currentMinDelay = 80;
        private int currentMaxDelay = 160;

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

        private int GetDelay() => rand.Next(currentMinDelay, currentMaxDelay);

        public async Task TypeTextAsync(string text)
        {
            stopRequested = false;
            
            foreach (char c in text)
            {
                if (stopRequested) break;
                while (paused && !stopRequested) await Task.Delay(50);
                if (stopRequested) break;
                
                if (c == '\n') SendEnterKey();
                else if (c == '\r') continue;
                else SendUnicodeChar(c);
                
                await Task.Delay(GetDelay());
            }
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