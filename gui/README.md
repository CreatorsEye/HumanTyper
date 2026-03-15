# 🎨 Creators Eye Auto Typer – GUI Version

A standalone Windows application with modern interface, global hotkeys, and full typing automation.

## 📥 Download

Download `CEautoTyper.exe` from [Archive.org](https://archive.org/details/creators-eye-human-typer)

## 🚀 Quick Start

1. Double-click `CEautoTyper.exe`
2. Paste or type your text
3. Click **START** or press `F5`
4. Click target window within 5 seconds
5. Watch it type!

## 🎮 Global Hotkeys

| Key | Action |
|-----|--------|
| `F5` | Start |
| `F8` | Pause |
| `F9` | Resume |
| `F10` | Stop |
| `F11` | Status |

*Click **Set Hotkeys** to customize*

## ⚙️ Settings

**Speed Levels:**
- Very Slow (200-300ms)
- Slow (120-200ms)
- Medium (60-120ms)
- Fast (30-70ms)
- Very Fast (15-40ms)

**Mistake Levels:**
- None (0)
- Few (1-2)
- Some (3-4)
- Many (5-7)
- Lots (8-10)
- Random

## 📝 Input Methods

- Type directly
- Paste from clipboard
- Import from Notepad

## 🖼️ Interface

- Borderless dark theme
- Drag by title bar
- Live action log
- Minimize/Close buttons

## 🔧 Build from Source

```bash
cd gui/src
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## ❓ Troubleshooting
Click target during countdown

Change hotkeys if conflicts

Add antivirus exclusion if needed
