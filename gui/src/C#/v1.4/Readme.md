# v1.4 – Added Multi-Line Support

## 🎯 What This Version Teaches

Building on v1.3, this version adds:

- ✅ Multi-line support – preserves line breaks and paragraphs
- ✅ Handles `\n` (new line) correctly – sends Enter key automatically
- ✅ Skips `\r` (carriage return) characters
- ✅ Perfect for pasting multi-line text, code, or formatted content
- ✅ Enter key simulation via SendInput

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

### 1. Detecting new line characters during typing

```csharp
foreach (char c in text)
{
    if (c == '\n')
    {
        SendEnterKey();
    }
    else if (c == '\r')
    {
        continue; // Skip carriage return
    }
    else
    {
        SendUnicodeChar(c);
    }
}
```
2. Sending Enter key via SendInput
```csharp
private void SendEnterKey()
{
    var inputs = new INPUT[2];
    inputs[0] = CreateKeyInput(0x0D, 0x0000); // Key down
    inputs[1] = CreateKeyInput(0x0D, 0x0002); // Key up
    SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT)));
}
```
3. Creating key input structure
```csharp
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
```
4. Example multi-line text in input box
```csharp
txtInput.Text = "Line 1\nLine 2\nLine 3\nLine 4";
```
🚀 How to Build
```bash
dotnet publish -c Release --self-contained true -p:PublishSingleFile=true
```
The EXE will be in the publish/ folder.

text

---
## Download EXE

[CE Auto Typer v1.4 on Archive.org](https://archive.org/details/ce-auto-typer-v1.4)

---

## Next Version

[v1.5](https://github.com/CreatorsEye/HumanTyper/tree/main/gui/src/C%23/v1.5) – Adds Customizable hotkeys + Single instance

---

## Related Documentation

- [Developer Guide](https://github.com/CreatorsEye/HumanTyper/blob/main/docs/developer-guide.md) – Complete learning path
- [User Guide](https://github.com/CreatorsEye/HumanTyper/blob/main/docs/user-guide.md) – How to use the tool

<p align="center">
  <b>Start from scratch. Reach advanced.</b><br>
  <i>Free. Open. For learning.</i>
</p>
