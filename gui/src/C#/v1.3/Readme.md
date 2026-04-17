# v1.3 – Added Global Hotkeys

## 🎯 What This Version Teaches

Building on v1.2, this version adds:

- ✅ Global hotkeys – control typing from anywhere
- ✅ F5 = Start typing
- ✅ F8 = Pause
- ✅ F9 = Resume
- ✅ F10 = Stop
- ✅ Hotkeys work even when window is not focused
- ✅ WM_HOTKEY message handling via WndProc

---

## 📁 Files in This Version

| File | Purpose |
|------|---------|
| `Program.cs` | Application entry point |
| `MainForm.cs` | Main UI and typing logic |
| `CeAutoTyper.csproj` | Project configuration |
| `ce.ico` | Application icon |

---

## 🔑 Key Code Concepts

### 1. Registering global hotkeys

```csharp
private void RegisterHotkeys()
{
    NativeMethods.RegisterHotKey(this.Handle, HOTKEY_START, 0, 0x74); // F5
    NativeMethods.RegisterHotKey(this.Handle, HOTKEY_PAUSE, 0, 0x77); // F8
    NativeMethods.RegisterHotKey(this.Handle, HOTKEY_RESUME, 0, 0x78); // F9
    NativeMethods.RegisterHotKey(this.Handle, HOTKEY_STOP, 0, 0x79); // F10
}
```
2. Hotkey constants
```csharp
private const int HOTKEY_START = 1;
private const int HOTKEY_PAUSE = 2;
private const int HOTKEY_RESUME = 3;
private const int HOTKEY_STOP = 4;
```
3. Handling hotkey messages (WndProc)
```csharp
protected override void WndProc(ref Message m)
{
    if (m.Msg == 0x0312) // WM_HOTKEY
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
                    this.Invoke(new Action(() => { engine.Paused = true; btnPause.Text = "RESUME"; }));
                break;
            case HOTKEY_RESUME:
                if (isTyping && engine.Paused && btnPause.Visible)
                    this.Invoke(new Action(() => { engine.Paused = false; btnPause.Text = "PAUSE"; }));
                break;
            case HOTKEY_STOP:
                if (isTyping)
                    this.Invoke(new Action(() => { engine.Stop(); ResetUI(); }));
                break;
        }
    }
    base.WndProc(ref m);
}
```
4. Hotkey label display
```csharp
lblHotkeys = new Label
{
    Text = "Hotkeys: F5=Start | F8=Pause | F9=Resume | F10=Stop",
    Location = new Point(200, 145),
    Size = new Size(370, 25),
    ForeColor = Color.FromArgb(200, 200, 200),
    Font = new Font("Segoe UI", 9)
};
```
🚀 How to Build
```bash
dotnet publish -c Release --self-contained true -p:PublishSingleFile=true
```
The EXE will be in the publish/ folder.


## Download EXE

[CE Auto Typer v1.3 on Archive.org](https://archive.org/details/ce-auto-typer-v1.3)

---

## Next Version

[v1.4](https://github.com/CreatorsEye/HumanTyper/tree/main/gui/src/C%23/v1.4) – Adds Multi-line support (line breaks preserved)

---

## Related Documentation

- [Developer Guide](https://github.com/CreatorsEye/HumanTyper/blob/main/docs/developer-guide.md) – Complete learning path
- [User Guide](https://github.com/CreatorsEye/HumanTyper/blob/main/docs/user-guide.md) – How to use the tool

<p align="center">
  <b>Start from scratch. Reach advanced.</b><br>
  <i>Free. Open. For learning.</i>
</p>
