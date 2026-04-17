# v1.2 – Added Speed Control Dropdown

## 🎯 What This Version Teaches

Building on v1.1, this version adds:

- ✅ Speed dropdown – choose from 5 typing speeds
- ✅ Very Slow (250-450ms per character)
- ✅ Slow (150-250ms)
- ✅ Medium (80-160ms)
- ✅ Fast (40-90ms)
- ✅ Very Fast (15-45ms)
- ✅ Dynamic delay adjustment during typing

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

### 1. Speed dropdown UI

```csharp
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
```
2. Speed profile mapping
```csharp
private Dictionary<string, int[]> SpeedProfiles = new Dictionary<string, int[]>
{
    { "Very Slow", new int[] { 250, 450 } },
    { "Slow", new int[] { 150, 250 } },
    { "Medium", new int[] { 80, 160 } },
    { "Fast", new int[] { 40, 90 } },
    { "Very Fast", new int[] { 15, 45 } }
};
```
3. Setting speed before typing
```csharp
int speedIndex = cmbSpeed.SelectedIndex;
engine.SetSpeed(speedIndex);
```
4. Engine speed adjustment
```csharp
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
```
🚀 How to Build
```bash
dotnet publish -c Release --self-contained true -p:PublishSingleFile=true
```
The EXE will be in the publish/ folder.

## Download EXE

[CE Auto Typer v1.2 on Archive.org](https://archive.org/details/ce-auto-typer-v1.2)

---

## Next Version

[v1.3](https://github.com/CreatorsEye/HumanTyper/tree/main/gui/src/C%23/v1.3) – Adds Global hotkeys (F5, F8, F9, F10)

---

## Related Documentation

- [Developer Guide](https://github.com/CreatorsEye/HumanTyper/blob/main/docs/developer-guide.md) – Complete learning path
- [User Guide](https://github.com/CreatorsEye/HumanTyper/blob/main/docs/user-guide.md) – How to use the tool

<p align="center">
  <b>Start from scratch. Reach advanced.</b><br>
  i>Free. Open. For learning.</i>
</p>
