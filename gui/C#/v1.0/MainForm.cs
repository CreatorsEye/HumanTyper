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
        private Button btnStart;
        private TypingEngine engine;
        private NotifyIcon trayIcon;

        public MainForm()
        {
            // Load icon from embedded resource (INSIDE the EXE)
            try
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CeAutoTyper.ce.ico"))
                {
                    if (stream != null)
                    {
                        this.Icon = new Icon(stream);
                    }
                }
            }
            catch { }

            this.Text = "CE Auto Typer v1.0";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 35);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.ShowIcon = true;

            // Tray icon from embedded resource
            try
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CeAutoTyper.ce.ico"))
                {
                    if (stream != null)
                    {
                        trayIcon = new NotifyIcon();
                        trayIcon.Icon = new Icon(stream);
                        trayIcon.Text = "CE Auto Typer v1.0";
                        trayIcon.Visible = true;
                        trayIcon.DoubleClick += (s, e) => { this.Show(); this.WindowState = FormWindowState.Normal; };
                    }
                }
            }
            catch { }

            txtInput = new RichTextBox
            {
                Location = new Point(12, 12),
                Size = new Size(560, 250),
                Font = new Font("Consolas", 11),
                BackColor = Color.FromArgb(45, 45, 50),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Text = "Type or paste your text here..."
            };

            btnStart = new Button
            {
                Text = "START TYPING",
                Location = new Point(12, 280),
                Size = new Size(140, 40),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnStart.FlatAppearance.BorderSize = 0;
            btnStart.Click += BtnStart_Click;

            Controls.Add(txtInput);
            Controls.Add(btnStart);

            engine = new TypingEngine();
        }

        private async void BtnStart_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtInput.Text)) return;

            btnStart.Enabled = false;
            btnStart.Text = "WAIT...";
            
            for (int i = 5; i > 0; i--)
            {
                btnStart.Text = $"STARTING IN {i}...";
                await Task.Delay(1000);
            }
            
            btnStart.Text = "TYPING...";
            await engine.TypeTextAsync(txtInput.Text);
            
            btnStart.Text = "START TYPING";
            btnStart.Enabled = true;
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
        
        public async Task TypeTextAsync(string text)
        {
            stopRequested = false;
            foreach (char c in text)
            {
                if (stopRequested) break;
                SendUnicodeChar(c);
                await Task.Delay(rand.Next(80, 160));
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
