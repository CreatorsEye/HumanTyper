using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace CEautoTyper
{
    public class MainForm : Form
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        static extern short VkKeyScan(char ch);

        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        static extern bool ReleaseCapture();

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        const uint KEYEVENTF_KEYDOWN = 0x0000;
        const uint KEYEVENTF_KEYUP = 0x0002;

        private const int HOTKEY_START = 1;
        private const int HOTKEY_PAUSE = 2;
        private const int HOTKEY_RESUME = 3;
        private const int HOTKEY_STOP = 4;
        private const int HOTKEY_STATUS = 5;

        private TextBox txtInput;
        private ComboBox cmbSpeed;
        private ComboBox cmbMistakes;
        private Button btnStart;
        private Button btnStop;
        private Button btnPause;
        private Button btnNotepad;
        private Button btnClipboard;
        private Button btnDefault;
        private Button btnHotkeySettings;
        private RichTextBox txtLog;
        private Label lblStatus;
        private Label lblHotkeyDisplay;
        private Timer forceTopTimer;
        private bool isDropdownOpen = false;

        private string CurrentText = "";
        private int TotalWords = 0;
        private bool Paused = false;
        private bool StopTyping = false;
        private string CurrentSpeed = "Medium";
        private int[] CurrentMistakeRange = new int[] { 0, 0 };
        private bool IsTyping = false;
        private Random rand = new Random();

        private Dictionary<string, Keys> Hotkeys = new Dictionary<string, Keys>
        {
            { "Start", Keys.F5 },
            { "Pause", Keys.F8 },
            { "Resume", Keys.F9 },
            { "Stop", Keys.F10 },
            { "Status", Keys.F11 }
        };

        private Dictionary<string, int[]> SpeedProfiles = new Dictionary<string, int[]>
        {
            { "Very Slow", new int[] { 200, 300 } },
            { "Slow", new int[] { 120, 200 } },
            { "Medium", new int[] { 60, 120 } },
            { "Fast", new int[] { 30, 70 } },
            { "Very Fast", new int[] { 15, 40 } }
        };

        public MainForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            InitializeComponent();
            UpdateStatus();
            UpdateHotkeyDisplay();
            RegisterAllHotkeys();

            forceTopTimer = new Timer();
            forceTopTimer.Interval = 100;
            forceTopTimer.Tick += (s, e) =>
            {
                if (!isDropdownOpen && !IsTyping)
                {
                    SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                }
            };
            forceTopTimer.Start();

            this.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left && e.Y < 40)
                {
                    ReleaseCapture();
                    SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            };
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0111)
            {
                if ((int)m.WParam == 0xF001) isDropdownOpen = true;
                else if ((int)m.WParam == 0xF002) isDropdownOpen = false;
            }

            if (m.Msg == 0x0312)
            {
                int id = m.WParam.ToInt32();
                switch (id)
                {
                    case HOTKEY_START:
                        if (!IsTyping && !string.IsNullOrEmpty(txtInput.Text))
                            this.Invoke(new Action(() => BtnStart_Click(null, null)));
                        break;
                    case HOTKEY_PAUSE:
                        if (IsTyping && !Paused)
                            this.Invoke(new Action(() => { Paused = true; btnPause.Text = "RESUME"; AddLog("Paused (Hotkey)", Color.Yellow); UpdateStatus(); }));
                        break;
                    case HOTKEY_RESUME:
                        if (IsTyping && Paused)
                            this.Invoke(new Action(() => { Paused = false; btnPause.Text = "PAUSE"; AddLog("Resumed (Hotkey)", Color.Lime); UpdateStatus(); }));
                        break;
                    case HOTKEY_STOP:
                        if (IsTyping)
                            this.Invoke(new Action(() => { StopTyping = true; AddLog("Stopped (Hotkey)", Color.Red); }));
                        break;
                    case HOTKEY_STATUS:
                        this.Invoke(new Action(() => AddLog($"Status: {(Paused ? "PAUSED" : "RUNNING")}", Color.Cyan)));
                        break;
                }
            }
            base.WndProc(ref m);
        }

        private void RegisterAllHotkeys()
        {
            try
            {
                UnregisterAllHotkeys();
                RegisterHotKey(this.Handle, HOTKEY_START, 0, (uint)Hotkeys["Start"]);
                RegisterHotKey(this.Handle, HOTKEY_PAUSE, 0, (uint)Hotkeys["Pause"]);
                RegisterHotKey(this.Handle, HOTKEY_RESUME, 0, (uint)Hotkeys["Resume"]);
                RegisterHotKey(this.Handle, HOTKEY_STOP, 0, (uint)Hotkeys["Stop"]);
                RegisterHotKey(this.Handle, HOTKEY_STATUS, 0, (uint)Hotkeys["Status"]);
            }
            catch { }
        }

        private void UnregisterAllHotkeys()
        {
            try
            {
                UnregisterHotKey(this.Handle, HOTKEY_START);
                UnregisterHotKey(this.Handle, HOTKEY_PAUSE);
                UnregisterHotKey(this.Handle, HOTKEY_RESUME);
                UnregisterHotKey(this.Handle, HOTKEY_STOP);
                UnregisterHotKey(this.Handle, HOTKEY_STATUS);
            }
            catch { }
        }

        private void UpdateHotkeyDisplay()
        {
            if (lblHotkeyDisplay != null)
                lblHotkeyDisplay.Text = $"Start:{Hotkeys["Start"]}  Pause:{Hotkeys["Pause"]}  Resume:{Hotkeys["Resume"]}  Stop:{Hotkeys["Stop"]}  Status:{Hotkeys["Status"]}";
        }

        private void InitializeComponent()
        {
            this.Size = new Size(780, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(26, 26, 46);

            Panel titleBar = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(780, 40),
                BackColor = Color.FromArgb(40, 40, 60)
            };

            Label lblTitle = new Label
            {
                Text = "CE AUTO TYPER v1.0",
                Font = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.Cyan,
                Location = new Point(10, 8),
                Size = new Size(300, 25),
                BackColor = Color.Transparent
            };

            Button btnClose = new Button
            {
                Text = "X",
                Location = new Point(730, 8),
                Size = new Size(30, 25),
                BackColor = Color.FromArgb(200, 60, 60),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Close();

            Button btnMinimize = new Button
            {
                Text = "-",
                Location = new Point(695, 8),
                Size = new Size(30, 25),
                BackColor = Color.FromArgb(80, 80, 100),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

            titleBar.Controls.Add(lblTitle);
            titleBar.Controls.Add(btnMinimize);
            titleBar.Controls.Add(btnClose);

            titleBar.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            };

            lblStatus = new Label
            {
                Location = new Point(20, 50),
                Size = new Size(650, 20),
                ForeColor = Color.Lime
            };

            txtInput = new TextBox
            {
                Location = new Point(20, 80),
                Size = new Size(730, 100),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Text = "This is simple auto writer tool used for good propose.\nFree to use.\nGreat to achieve your goals.",
                BackColor = Color.FromArgb(15, 52, 96),
                ForeColor = Color.White,
                Font = new Font("Consolas", 10)
            };

            btnNotepad = CreateButton("Notepad", 20, 190, 100, Color.FromArgb(52, 152, 219));
            btnClipboard = CreateButton("Clipboard", 130, 190, 100, Color.FromArgb(155, 89, 182));
            btnDefault = CreateButton("Default", 240, 190, 100, Color.FromArgb(46, 204, 113));

            Label lblSpeed = new Label
            {
                Text = "Speed:",
                Location = new Point(20, 240),
                Size = new Size(50, 25),
                ForeColor = Color.White
            };

            cmbSpeed = new ComboBox
            {
                Location = new Point(80, 237),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(15, 52, 96),
                ForeColor = Color.White
            };
            cmbSpeed.Items.AddRange(new string[] { "Very Slow", "Slow", "Medium", "Fast", "Very Fast" });
            cmbSpeed.SelectedIndex = 2;
            cmbSpeed.SelectedIndexChanged += (s, e) => { CurrentSpeed = cmbSpeed.SelectedItem.ToString(); UpdateStatus(); };
            cmbSpeed.DropDown += (s, e) => isDropdownOpen = true;
            cmbSpeed.DropDownClosed += (s, e) => isDropdownOpen = false;

            Label lblMistakes = new Label
            {
                Text = "Mistakes:",
                Location = new Point(250, 240),
                Size = new Size(60, 25),
                ForeColor = Color.White
            };

            cmbMistakes = new ComboBox
            {
                Location = new Point(320, 237),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(15, 52, 96),
                ForeColor = Color.White
            };
            cmbMistakes.Items.AddRange(new string[] { "None", "Few", "Some", "Many", "Lots", "Random" });
            cmbMistakes.SelectedIndex = 0;
            cmbMistakes.SelectedIndexChanged += (s, e) => SetMistakeRange();
            cmbMistakes.DropDown += (s, e) => isDropdownOpen = true;
            cmbMistakes.DropDownClosed += (s, e) => isDropdownOpen = false;

            lblHotkeyDisplay = new Label
            {
                Location = new Point(20, 275),
                Size = new Size(550, 20),
                ForeColor = Color.Yellow
            };

            btnHotkeySettings = CreateButton("Set Hotkeys", 580, 270, 120, Color.FromArgb(155, 89, 182));
            btnHotkeySettings.Click += BtnHotkeySettings_Click;

            btnStart = CreateButton("START", 20, 310, 100, Color.FromArgb(46, 204, 113), 12, true);
            btnStop = CreateButton("STOP", 130, 310, 100, Color.FromArgb(231, 76, 60), 12, true);
            btnPause = CreateButton("PAUSE", 240, 310, 100, Color.FromArgb(241, 196, 15), 12, true);

            btnStart.Visible = true;
            btnStop.Visible = false;
            btnPause.Visible = false;

            txtLog = new RichTextBox
            {
                Location = new Point(20, 360),
                Size = new Size(730, 240),
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.Lime,
                Font = new Font("Consolas", 9)
            };

            this.Controls.AddRange(new Control[] {
                titleBar, lblStatus, txtInput,
                btnNotepad, btnClipboard, btnDefault,
                lblSpeed, cmbSpeed, lblMistakes, cmbMistakes,
                lblHotkeyDisplay, btnHotkeySettings,
                btnStart, btnStop, btnPause,
                txtLog
            });

            btnNotepad.Click += BtnNotepad_Click;
            btnClipboard.Click += BtnClipboard_Click;
            btnDefault.Click += BtnDefault_Click;
            btnStart.Click += BtnStart_Click;
            btnStop.Click += BtnStop_Click;
            btnPause.Click += BtnPause_Click;
        }

        private Button CreateButton(string text, int x, int y, int width, Color color, int fontSize = 10, bool bold = false)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 35),
                BackColor = color,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", fontSize, bold ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = Color.White
            };
        }

        private void BtnHotkeySettings_Click(object sender, EventArgs e)
        {
            forceTopTimer.Stop();

            Form hotkeyForm = new Form
            {
                Text = "Hotkey Settings",
                Size = new Size(350, 350),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(26, 26, 46),
                ForeColor = Color.White,
                TopMost = true
            };

            int yPos = 20;
            var newHotkeys = new Dictionary<string, Keys>(Hotkeys);

            foreach (var action in new string[] { "Start", "Pause", "Resume", "Stop", "Status" })
            {
                Label lbl = new Label
                {
                    Text = $"{action}:",
                    Location = new Point(20, yPos),
                    Size = new Size(60, 30),
                    ForeColor = Color.White,
                    TextAlign = ContentAlignment.MiddleLeft
                };

                Button btn = new Button
                {
                    Text = Hotkeys[action].ToString(),
                    Location = new Point(100, yPos),
                    Size = new Size(100, 30),
                    BackColor = Color.FromArgb(52, 152, 219),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    Tag = action
                };

                btn.Click += (s, args) =>
                {
                    Button clickedBtn = (Button)s;
                    string actionName = clickedBtn.Tag.ToString();

                    Form keyPressForm = new Form
                    {
                        Text = "Press any key",
                        Size = new Size(300, 150),
                        StartPosition = FormStartPosition.CenterParent,
                        FormBorderStyle = FormBorderStyle.FixedDialog,
                        ControlBox = false,
                        TopMost = true,
                        BackColor = Color.FromArgb(26, 26, 46)
                    };

                    Label lblPress = new Label
                    {
                        Text = $"Press new key for {actionName}",
                        Location = new Point(50, 30),
                        Size = new Size(200, 30),
                        TextAlign = ContentAlignment.MiddleCenter,
                        ForeColor = Color.White
                    };

                    Label lblEsc = new Label
                    {
                        Text = "Press ESC to cancel",
                        Location = new Point(50, 70),
                        Size = new Size(200, 20),
                        TextAlign = ContentAlignment.MiddleCenter,
                        ForeColor = Color.Gray
                    };

                    keyPressForm.Controls.Add(lblPress);
                    keyPressForm.Controls.Add(lblEsc);
                    keyPressForm.KeyPreview = true;

                    keyPressForm.KeyDown += (ks, ke) =>
                    {
                        if (ke.KeyCode != Keys.Escape)
                        {
                            clickedBtn.Text = ke.KeyCode.ToString();
                            newHotkeys[actionName] = ke.KeyCode;
                        }
                        keyPressForm.Close();
                    };

                    keyPressForm.ShowDialog();
                };

                hotkeyForm.Controls.Add(lbl);
                hotkeyForm.Controls.Add(btn);
                yPos += 40;
            }

            Button btnSave = new Button
            {
                Text = "Save",
                Location = new Point(80, yPos + 10),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(46, 204, 113),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White
            };

            Button btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(180, yPos + 10),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(231, 76, 60),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White
            };

            btnSave.Click += (s, args) =>
            {
                Hotkeys = new Dictionary<string, Keys>(newHotkeys);
                UpdateHotkeyDisplay();
                RegisterAllHotkeys();
                AddLog("Hotkeys updated successfully", Color.Lime);
                hotkeyForm.Close();
            };

            btnCancel.Click += (s, args) =>
            {
                hotkeyForm.Close();
            };

            hotkeyForm.Controls.Add(btnSave);
            hotkeyForm.Controls.Add(btnCancel);
            hotkeyForm.ShowDialog();
            forceTopTimer.Start();
        }

        private void SetMistakeRange()
        {
            switch (cmbMistakes.SelectedIndex)
            {
                case 0: CurrentMistakeRange = new int[] { 0, 0 }; break;
                case 1: CurrentMistakeRange = new int[] { 1, 2 }; break;
                case 2: CurrentMistakeRange = new int[] { 3, 4 }; break;
                case 3: CurrentMistakeRange = new int[] { 5, 7 }; break;
                case 4: CurrentMistakeRange = new int[] { 8, 10 }; break;
                case 5:
                    int min = rand.Next(0, 5);
                    int max = min + rand.Next(1, 4);
                    CurrentMistakeRange = new int[] { min, max };
                    break;
            }
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            if (lblStatus != null)
            {
                string status = IsTyping ? (Paused ? "PAUSED" : "TYPING") : "READY";
                lblStatus.Text = $"Status: {status} | Speed: {CurrentSpeed} | Mistakes: {CurrentMistakeRange[0]}-{CurrentMistakeRange[1]} | Words: {TotalWords}";
            }
        }

        private void AddLog(string message, Color color)
        {
            if (txtLog != null && !txtLog.IsDisposed)
            {
                txtLog.SelectionStart = txtLog.TextLength;
                txtLog.SelectionColor = color;
                txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
                txtLog.ScrollToCaret();
            }
        }

        private void BtnNotepad_Click(object sender, EventArgs e)
        {
            string tempFile = Path.GetTempFileName() + ".txt";
            Process notepad = Process.Start("notepad.exe", tempFile);
            notepad.WaitForExit();

            if (File.Exists(tempFile))
            {
                string text = File.ReadAllText(tempFile);
                File.Delete(tempFile);
                if (!string.IsNullOrEmpty(text))
                {
                    txtInput.Text = text;
                    CurrentText = text;
                    TotalWords = CurrentText.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
                    UpdateStatus();
                    AddLog($"Loaded {TotalWords} words from Notepad", Color.Cyan);
                }
            }
        }

        private void BtnClipboard_Click(object sender, EventArgs e)
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    string text = Clipboard.GetText();
                    txtInput.Text = text;
                    CurrentText = text;
                    TotalWords = CurrentText.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
                    UpdateStatus();
                    AddLog($"Loaded {TotalWords} words from clipboard", Color.Cyan);
                }
            }
            catch
            {
                AddLog("Failed to get clipboard", Color.Red);
            }
        }

        private void BtnDefault_Click(object sender, EventArgs e)
        {
            txtInput.Text = "This is simple auto writer tool used for good propose.\nFree to use.\nGreat to achieve your goals.";
            CurrentText = txtInput.Text;
            TotalWords = CurrentText.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
            UpdateStatus();
            AddLog("Default text loaded", Color.Green);
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtInput.Text))
            {
                AddLog("No text loaded!", Color.Red);
                return;
            }

            if (IsTyping)
            {
                AddLog("Already typing!", Color.Yellow);
                return;
            }

            CurrentText = txtInput.Text;
            TotalWords = CurrentText.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
            StopTyping = false;
            Paused = false;
            IsTyping = true;

            btnStart.Visible = false;
            btnStop.Visible = true;
            btnPause.Visible = true;
            btnPause.Text = "PAUSE";

            cmbSpeed.Enabled = false;
            cmbMistakes.Enabled = false;
            btnNotepad.Enabled = false;
            btnClipboard.Enabled = false;
            btnDefault.Enabled = false;
            btnHotkeySettings.Enabled = false;

            UpdateStatus();

            AddLog($"Ready to type: {TotalWords} words", Color.Yellow);
            AddLog("Click target window. Starting in 5 seconds...", Color.Orange);

            Thread typingThread = new Thread(TypingWorker);
            typingThread.Start();
        }

        private void TypingWorker()
        {
            Thread.Sleep(5000);

            if (StopTyping)
            {
                IsTyping = false;
                return;
            }

            int targetMistakes = rand.Next(CurrentMistakeRange[0], CurrentMistakeRange[1] + 1);
            int mistakesMade = 0;
            int wordCount = 0;

            this.Invoke(new Action(() => AddLog("Typing started...", Color.Lime)));

            string text = CurrentText;
            for (int i = 0; i < text.Length; i++)
            {
                if (StopTyping) break;

                while (Paused)
                {
                    Thread.Sleep(100);
                    if (StopTyping) break;
                }

                char c = text[i];

                if (c == '\n' || (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n'))
                {
                    if (c == '\r') i++;
                    keybd_event(0x0D, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
                    keybd_event(0x0D, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                    Thread.Sleep(400);

                    if (i + 1 < text.Length && text[i + 1] == ' ')
                    {
                        i++;
                        this.Invoke(new Action(() => AddLog("(removed leading space)", Color.Gray)));
                    }
                    continue;
                }
                else if (c == ' ')
                {
                    keybd_event(0x20, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
                    keybd_event(0x20, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                    Thread.Sleep(100);
                }
                else if (":;,.".IndexOf(c) >= 0)
                {
                    TypeChar(c);
                    Thread.Sleep(300);
                }
                else
                {
                    bool makeMistake = mistakesMade < targetMistakes && wordCount > 2 && rand.Next(100) < 30;

                    if (makeMistake)
                    {
                        string word = "";
                        int start = i;
                        while (start < text.Length && !char.IsWhiteSpace(text[start]) && text[start] != '\n' && text[start] != '\r')
                        {
                            word += text[start];
                            start++;
                        }

                        if (word.Length > 1)
                        {
                            string typo = MakeTypo(word);
                            this.Invoke(new Action(() => AddLog($"Typo: {typo}", Color.Red)));

                            foreach (char tc in typo)
                            {
                                if (StopTyping) break;
                                TypeChar(tc);
                                Thread.Sleep(rand.Next(SpeedProfiles[CurrentSpeed][0], SpeedProfiles[CurrentSpeed][1]));
                            }

                            if (!StopTyping)
                            {
                                Thread.Sleep(300);
                                for (int b = 0; b < typo.Length; b++)
                                {
                                    if (StopTyping) break;
                                    keybd_event(0x08, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
                                    keybd_event(0x08, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                                    Thread.Sleep(50);
                                }
                                Thread.Sleep(400);

                                foreach (char wc in word)
                                {
                                    if (StopTyping) break;
                                    TypeChar(wc);
                                    Thread.Sleep(rand.Next(SpeedProfiles[CurrentSpeed][0], SpeedProfiles[CurrentSpeed][1]));
                                }
                                mistakesMade++;
                                i = start - 1;
                                continue;
                            }
                        }
                    }

                    TypeChar(c);
                    Thread.Sleep(rand.Next(SpeedProfiles[CurrentSpeed][0], SpeedProfiles[CurrentSpeed][1]));
                }
            }

            this.Invoke(new Action(() =>
            {
                if (!StopTyping)
                {
                    AddLog($"Complete! Made {mistakesMade} mistakes", Color.Lime);
                }
                else
                {
                    AddLog("Stopped", Color.Red);
                }

                btnStart.Visible = true;
                btnStop.Visible = false;
                btnPause.Visible = false;

                cmbSpeed.Enabled = true;
                cmbMistakes.Enabled = true;
                btnNotepad.Enabled = true;
                btnClipboard.Enabled = true;
                btnDefault.Enabled = true;
                btnHotkeySettings.Enabled = true;

                IsTyping = false;
                UpdateStatus();
            }));
        }

        private string MakeTypo(string word)
        {
            if (word.Length <= 1) return word;
            int type = rand.Next(1, 4);
            string typo = word;

            switch (type)
            {
                case 1:
                    if (word.Length > 2)
                    {
                        int pos = rand.Next(1, word.Length);
                        typo = word.Substring(0, pos) + word[pos - 1] + word.Substring(pos - 1);
                    }
                    break;
                case 2:
                    if (word.Length > 3)
                    {
                        int pos = rand.Next(1, word.Length - 1);
                        typo = word.Remove(pos, 1);
                    }
                    break;
                case 3:
                    if (word.Length > 3)
                    {
                        int pos = rand.Next(0, word.Length - 2);
                        char c1 = word[pos];
                        char c2 = word[pos + 1];
                        typo = word.Remove(pos, 2).Insert(pos, $"{c2}{c1}");
                    }
                    break;
            }
            return typo;
        }

        private void TypeChar(char c)
        {
            try
            {
                short vk = VkKeyScan(c);
                byte key = (byte)(vk & 0xff);
                byte shift = (byte)((vk >> 8) & 0xff);

                if ((shift & 1) != 0)
                    keybd_event(0x10, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);

                keybd_event(key, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
                keybd_event(key, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

                if ((shift & 1) != 0)
                    keybd_event(0x10, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            }
            catch { }
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            StopTyping = true;
            Paused = false;
            AddLog("Stopping...", Color.Red);
        }

        private void BtnPause_Click(object sender, EventArgs e)
        {
            if (Paused)
            {
                Paused = false;
                btnPause.Text = "PAUSE";
                AddLog("Resumed", Color.Lime);
            }
            else
            {
                Paused = true;
                btnPause.Text = "RESUME";
                AddLog("Paused", Color.Yellow);
            }
            UpdateStatus();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            forceTopTimer?.Stop();
            UnregisterAllHotkeys();
            base.OnFormClosing(e);
        }
    }
}
