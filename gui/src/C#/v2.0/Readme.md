# v2.0 – Professional Borderless UI (FINAL)

## What This Version Teaches

- Professional borderless window
- Custom title bar with minimize and close buttons
- Immersive dark mode support (Windows 11)
- Window shadow effects
- Resizable window with minimum size
- Modern rounded corners (sharp corners in this version)
- All previous features combined

## Files

| File | Purpose |
|------|---------|
| Program.cs | Entry point with Mutex |
| MainForm.cs | Main UI with custom title bar |
| CeAutoTyper.csproj | Project config |
| ce.ico | App icon |

## Key Code

### Remove default window border

```csharp
this.FormBorderStyle = FormBorderStyle.None;
SetWindowLong(this.Handle, GWL_STYLE, GetWindowLong(this.Handle, GWL_STYLE) & ~WS_CAPTION & ~WS_THICKFRAME);
```
Custom title bar
```csharp
titleBar = new Panel
{
    Dock = DockStyle.Top,
    Height = 45,
    BackColor = Color.FromArgb(28, 28, 35)
};

btnMinimize = new Button { Text = "─", Size = new Size(45, 45) };
btnClose = new Button { Text = "✕", Size = new Size(45, 45) };
```
Drag window by title bar
```csharp
private void TitleBar_MouseDown(object sender, MouseEventArgs e)
{
    if (e.Button == MouseButtons.Left)
    {
        ReleaseCapture();
        SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
    }
}
```
Dark mode for Windows 11
```csharp
if (Environment.OSVersion.Version.Major >= 10)
{
    int useDarkMode = 1;
    DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDarkMode, sizeof(int));
}
```
How to Build
```bash
dotnet publish -c Release --self-contained true -p:PublishSingleFile=true
```


## Download EXE

[CE Auto Typer v2.0 on Archive.org](https://archive.org/details/ce-auto-typer-v2.0)

## Final Version

This is the FINAL version. All features from v1.0 to v2.0 are included.

- Speed control
- Mistakes simulation
- Fix Typo (human-like timing)
- Global hotkeys (customizable)
- Multi-line support
- Single instance
- Auto stay-on-top during typing
- Professional borderless UI

## Related Documentation

- [Developer Guide](https://github.com/CreatorsEye/HumanTyper/blob/main/docs/developer-guide.md)
- [User Guide](https://github.com/CreatorsEye/HumanTyper/blob/main/docs/user-guide.md)

---

Start from scratch. Reach advanced.

Free. Open. For learning.
