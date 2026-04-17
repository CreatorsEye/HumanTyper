# v1.9 – Added Auto Stay-on-Top During Typing

## What This Version Teaches

- Window stays on top ONLY during typing
- TopMost = true when typing starts
- TopMost = false when typing stops or completes

## Files

| File | Purpose |
|------|---------|
| Program.cs | Entry point |
| MainForm.cs | UI and typing logic |
| CeAutoTyper.csproj | Project config |
| ce.ico | App icon |

## Key Code

### Set TopMost when typing starts

```csharp
private async void BtnStart_Click(object? sender, EventArgs e)
{
    this.TopMost = true;
    // ... typing logic
}
```
Remove TopMost when typing ends
```csharp
private void ResetUI()
{
    this.TopMost = false;
    // ... reset logic
}
```
How to Build
```bash
dotnet publish -c Release --self-contained true -p:PublishSingleFile=true
```
The EXE will be in the publish/ folder.


## Download EXE

[CE Auto Typer v1.9 on Archive.org](https://archive.org/details/ce-auto-typer-v1.9)

## Next Version

[v2.0](https://github.com/CreatorsEye/HumanTyper/tree/main/gui/src/C%23/v2.0) – Professional borderless UI (FINAL)

## Related Documentation

- [Developer Guide](https://github.com/CreatorsEye/HumanTyper/blob/main/docs/developer-guide.md)
- [User Guide](https://github.com/CreatorsEye/HumanTyper/blob/main/docs/user-guide.md)

---

Start from scratch. Reach advanced.

Free. Open. For learning.
