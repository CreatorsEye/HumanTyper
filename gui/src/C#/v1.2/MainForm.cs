using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.IO;

namespace CeAutoTyper
{
    public class MainForm : Form
    {
        private RichTextBox txtInput;
        private ComboBox cmbSpeed;
        private Button btnStart, btnStop, btnPause;
        private RichTextBox txtShell;
        private TypingEngine engine;
        private NotifyIcon trayIcon;
        private bool isTyping = false;

        public MainForm()
        {
            // Load icon from embedded resource
            try
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CeAutoTyper.ce.ico"))
                {
                    if (stream != null) this.Icon = new Icon(stream);
                }
            }
            catch { }

            this.Text = "CE Auto Typer v1.2";
            this.Size = new Size(600, 520);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 35);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.ShowIcon = true;

            // Tray icon
            try
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CeAutoTyper.ce.ico"))
                {
                    if (stream != null)
                    {
                        trayIcon = new NotifyIcon();
                        trayIcon.Icon = new Icon(stream);
                        trayIcon.Text = "CE Auto Typer v1.2";
                        trayIcon.Visible = true;
                        trayIcon.DoubleClick += (s, e) => { this.Show(); this.WindowState = FormWindowState.Normal; };
                    }
                }
            }
            catch { }

            // Text input
            txtInput = new RichTextBox
            {
                Location = new Point(12, 12),
                Size = new Size(560, 120),
                Font = new Font("Consolas", 11),
                BackColor = Color.FromArgb(45, 45, 50),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Text = "Type or paste your text here..."
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
            cmbSpeed.SelectedIndex = 2; // Default = Medium

            // Start button
            btnStart = new Button
            {
                Text = "START TYPING",
                Location = new Point(12, 180),
                Size = new Size(140, 40),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnStart.FlatAppearance.BorderSize = 0;
            btnStart.Click += BtnStart_Click;

            // Stop button
            btnStop = new Button
            {
                Text = "STOP",
                Location = new Point(160, 180),
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

            // Pause button
            btnPause = new Button
            {
                Text = "PAUSE",
                Location = new Point(270, 180),
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

            // Shell output
            txtShell = new RichTextBox
            {
                Location = new Point(12, 235),
                Size = new Size(560, 240),
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
            Controls.Add(btnStart);
            Controls.Add(btnStop);
            Controls.Add(btnPause);
            Controls.Add(txtShell);

            engine = new TypingEngine();
        }

        private async void BtnStart_Click(object sender, EventArgs e)
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

            // Countdown
            for (int i = 5; i > 0; i--)
            {
                if (engine.StopTyping) break;
                txtShell.Text = $"Starting in {i}...";
                await Task.Delay(1000);
            }

            if (!engine.StopTyping)
            {
                txtShell.Text = "Typing...";
                
                // Set speed based on dropdown
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
            trayIcon?.Dispose();
            engine.Stop();
            base.OnFormClosing(e);
        }
    }

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
                case 0: // Very Slow
                    currentMinDelay = 250;
                    currentMaxDelay = 450;
                    break;
                case 1: // Slow
                    currentMinDelay = 150;
                    currentMaxDelay = 250;
                    break;
                case 2: // Medium (default)
                    currentMinDelay = 80;
                    currentMaxDelay = 160;
                    break;
                case 3: // Fast
                    currentMinDelay = 40;
                    currentMaxDelay = 90;
                    break;
                case 4: // Very Fast
                    currentMinDelay = 15;
                    currentMaxDelay = 45;
                    break;
                default: // Medium
                    currentMinDelay = 80;
                    currentMaxDelay = 160;
                    break;
            }
        }

        private int GetDelay() => rand.Next(currentMinDelay, currentMaxDelay);

        public async Task TypeTextAsync(string text)
        {
            stopRequested = false;
            
            foreach (char c in text)
            {
                if (stopRequested) break;
                
                while (paused && !stopRequested)
                {
                    await Task.Delay(50);
                }
                
                if (stopRequested) break;
                
                SendUnicodeChar(c);
                await Task.Delay(GetDelay());
            }
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
}