# v1.5 – Added Customizable Hotkeys + Single Instance

## 🎯 What This Version Teaches

Building on v1.4, this version adds:

- ✅ Customizable Hotkeys – click "Set Hotkeys" to change any key
- ✅ Hotkey configuration saved to "hotkeys.cfg" (persists after restart)
- ✅ Single Instance – only one copy of the app can run at a time
- ✅ If you try to open another, it brings the existing window to front
- ✅ Mutex for process isolation
- ✅ Settings persistence via file I/O

---

## 📁 Files in This Version

| File | Purpose |
|------|---------|
| `Program.cs` | Application entry point with Mutex |
| `MainForm.cs` | Main UI and typing logic |
| `CeAutoTyper.csproj` | Project configuration |
| `ce.ico` | Application icon |
| `hotkeys.cfg` | Saved hotkey settings (created at runtime) |

---

## 🔑 Key Code Concepts

### 1. Single instance with Mutex

```csharp
private static Mutex mutex = null;

static void Main()
{
    const string appName = "CEAutoTyper";
    bool createdNew;
    mutex = new Mutex(true, appName, out createdNew);

    if (!createdNew)
    {
        IntPtr hWnd = NativeMethods.FindWindow(null, "CE Auto Typer v1.5");
        if (hWnd != IntPtr.Zero)
        {
            NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE);
            NativeMethods.SetForegroundWindow(hWnd);
        }
        return;
    }

    Application.Run(new MainForm());
    mutex.ReleaseMutex();
}
```
2. Hotkey dictionary with default values
```csharp
private Dictionary<string, Keys> hotkeys = new Dictionary<string, Keys>
{
    { "Start", Keys.F5 },
    { "Pause", Keys.F8 },
    { "Resume", Keys.F9 },
    { "Stop", Keys.F10 }
};
```
3. Loading hotkeys from file
```csharp
private void LoadHotkeys()
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
```
4. Saving hotkeys to file
```csharp
private void SaveHotkeys()
{
    string path = Path.Combine(Application.StartupPath, "hotkeys.cfg");
    List<string> lines = new List<string>();
    foreach (var kv in hotkeys)
        lines.Add($"{kv.Key}={kv.Value}");
    File.WriteAllLines(path, lines);
}
```
5. "Set Hotkeys" button and form
```csharp
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
btnSetHotkeys.Click += BtnSetHotkeys_Click;
```
6. Dynamic hotkey registration
```csharp
private void RegisterHotkeys()
{
    NativeWinMethods.RegisterHotKey(this.Handle, HOTKEY_START, 0, (uint)hotkeys["Start"]);
    NativeWinMethods.RegisterHotKey(this.Handle, HOTKEY_PAUSE, 0, (uint)hotkeys["Pause"]);
    NativeWinMethods.RegisterHotKey(this.Handle, HOTKEY_RESUME, 0, (uint)hotkeys["Resume"]);
    NativeWinMethods.RegisterHotKey(this.Handle, HOTKEY_STOP, 0, (uint)hotkeys["Stop"]);
}
```

🚀 How to Build
```bash
dotnet publish -c Release --self-contained true -p:PublishSingleFile=true
```
The EXE will be in the publish/ folder.

text

---

## Download EXE

[CE Auto Typer v1.5 on Archive.org](https://archive.org/details/ce-auto-typer-v1.5)

---

## Next Version

[v1.6](https://github.com/CreatorsEye/HumanTyper/tree/main/gui/src/C%23/v1.6) – Adds Mistakes simulation (typos)

---

## Related Documentation

- [Developer Guide](https://github.com/CreatorsEye/HumanTyper/blob/main/docs/developer-guide.md) – Complete learning path
- [User Guide](https://github.com/CreatorsEye/HumanTyper/blob/main/docs/user-guide.md) – How to use the tool

<p align="center">
  <b>Start from scratch. Reach advanced.</b><br>
  <i>Free. Open. For learning.</i>
</p>
