# v1.0 – Basic Auto-Typer

## 🎯 What This Version Teaches

This is the **foundation**. Everything starts here.

After studying v1.0, you will understand:

- ✅ Windows Forms basics (Form, Button, RichTextBox)
- ✅ SendInput API for Unicode character injection
- ✅ Async/await for countdown and typing delays
- ✅ System tray support (NotifyIcon)
- ✅ Embedded icon resources

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

### 1. SendInput API (Unicode character injection)

```csharp
private void SendUnicodeChar(char ch)
{
    var inputs = new INPUT[2];
    inputs[0] = CreateKeyboardInput(ch, 0x0004);
    inputs[1] = CreateKeyboardInput(ch, 0x0004 | 0x0002);
    SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT)));
}
```
This is the core of the auto-typer. It injects characters directly into the active window.

2. Async/await for delays
```csharp
for (int i = 5; i > 0; i--)
{
    btnStart.Text = $"STARTING IN {i}...";
    await Task.Delay(1000);
}
```
Async/await keeps the UI responsive during delays.

3. Embedded icon resource
```csharp
using (Stream stream = Assembly.GetExecutingAssembly()
    .GetManifestResourceStream("CeAutoTyper.ce.ico"))
{
    if (stream != null) this.Icon = new Icon(stream);
}
```
The icon is embedded inside the EXE – no external file needed.

🚀 How to Build
```bash
dotnet publish -c Release --self-contained true -p:PublishSingleFile=true
The EXE will be in the publish/ folder.
```

## Download EXE

[CE Auto Typer v1.0 on Archive.org](https://archive.org/details/ce-auto-typer-v-1.0)

---

## Next Version

[v1.1](https://github.com/CreatorsEye/HumanTyper/tree/main/gui/src/C%23/v1.1) – Adds Stop, Pause, Resume, and console output

---

## Related Documentation

- [Developer Guide](https://github.com/CreatorsEye/HumanTyper/blob/main/docs/developer-guide.md) – Complete learning path
- [User Guide](https://github.com/CreatorsEye/HumanTyper/blob/main/docs/user-guide.md) – How to use the tool

<p align="center"> <b>Start from scratch. Reach advanced.</b><br> <i>Free. Open. For learning.</i> </p> 
