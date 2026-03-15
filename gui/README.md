# 🎨 Creators Eye Auto Typer – GUI Version

[![Download](https://img.shields.io/badge/Download-EXE-brightgreen)](https://archive.org/download/creators-eye-human-typer/Creators%20Eye%20Human%20Typer.exe)
[![Version](https://img.shields.io/badge/Version-1.0-blue)]()
[![Windows](https://img.shields.io/badge/Platform-Windows%207%2B-success)]()
[![Archive.org](https://img.shields.io/badge/Archive.org-Project-orange)](https://archive.org/details/creators-eye-human-typer)

A standalone Windows application with a modern interface, global hotkeys, and full typing automation.

---

## 📥 Download

| File | Link | Size |
|:---|:---|:---|
| **Creators Eye Human Typer.exe** | [⬇️ Download from Archive.org](https://archive.org/download/creators-eye-human-typer/Creators%20Eye%20Human%20Typer.exe) | ~60 MB |
| **Project Page** | [archive.org/details/creators-eye-human-typer](https://archive.org/details/creators-eye-human-typer) | - |
| **Torrent** | [creators-eye-human-typer_archive.torrent](https://archive.org/download/creators-eye-human-typer/creators-eye-human-typer_archive.torrent) | - |

> **Note:** No installation required – just download and run!

---

## 🚀 Quick Start
👉 **For detailed instructions, see the [GUI Quick Guide](../docs/gui-quick-guide.md)**

### 30-Second Setup
1. **Download** and double-click `Creators Eye Human Typer.exe`.
2. **Paste** your text in the box.
3. Click **START** or press `F5`.
4. **Click** on your target window (Notepad, browser, etc.).
5. Watch it type automatically!

---

## 🎮 Features

### Global Hotkeys (Work Anywhere)
| Key | Action |
|:---|:---|
| `F5` | Start Typing |
| `F8` | Pause |
| `F9` | Resume |
| `F10` | Stop |
| `F11` | Status Check |

*Click **Set Hotkeys** to customize any key.*

### Speed Levels
* 🐢 **Very Slow** (200-300ms)
* 🚶 **Slow** (120-200ms)
* ⚡ **Medium** (60-120ms)
* 🏃 **Fast** (30-70ms)
* 🚀 **Very Fast** (15-40ms)

### Mistake Levels
* ✅ **None** (0)
* 🔹 **Few** (1-2)
* 🔸 **Some** (3-4)
* ⚠️ **Many** (5-7)
* ❗ **Lots** (8-10)
* 🎲 **Random**

---

## 📝 Input Methods

| Method | How to |
|:---|:---|
| **Type directly** | Just type in the large text box. |
| **Paste from clipboard** | Copy text (Ctrl+C), click **Clipboard** button. |
| **Import from Notepad** | Click **Notepad**, type there, save and close. |

---

## 📚 Documentation

| Guide | Description |
|:---|:---|
| [**GUI Quick Guide**](../docs/gui-quick-guide.md) | Step-by-step instructions with examples |
| [**Release Notes**](releases/v1.0/README.md) | Version 1.0 release information |
| [**Main README**](../README.md) | Project overview and CLI version |

---

## 🔧 Build from Source

### Prerequisites
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
* Windows 7+ (64-bit)

### Build Instructions
```bash
cd src
dotnet restore
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```
The compiled EXE will be in: `bin/Release/net8.0-windows/win-x64/publish/`

---

## ❓ Troubleshooting

| Problem | Solution |
|:---|:---|
| **Nothing happens at START** | You forgot to click the target window during the 5-second countdown. |
| **Hotkeys don't work** | Some apps block global hotkeys. Click **Set Hotkeys** and choose different keys. |
| **Antivirus blocks the EXE** | False positive—add an exclusion or use the CLI version. |
| **Line breaks aren't working** | Make sure your text has actual line breaks (Enter key). |

---

## 🔗 Related
* **CLI Version** – PowerShell script alternative.
* [GitHub Repository](https://github.com/yourusername/CreatorsEye-AutoTyper)
* [Archive.org Page](https://archive.org/details/creators-eye-human-typer)
