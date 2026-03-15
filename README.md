# 🚀 Creators Eye Auto Typer v1.0

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Windows](https://img.shields.io/badge/Platform-Windows-blue)](https://github.com/yourusername/CreatorsEye-AutoTyper)
[![Version](https://img.shields.io/badge/Version-1.0-green)](https://github.com/yourusername/CreatorsEye-AutoTyper/releases)
[![Download](https://img.shields.io/badge/Download-Archive.org-orange)](https://archive.org/details/creators-eye-human-typer)

![Creators Eye Auto Typer Screenshot](docs/screenshots/main.png)

A powerful, human‑like typing simulation tool with a modern GUI that automatically types text exactly as written – preserving line breaks, spacing, and formatting. Perfect for content creators, developers, and anyone needing automated typing with a natural, human touch.

---

## ✨ Features

| | |
|---|---|
| 🤖 **Human‑Like Typing** | Variable typing speeds with realistic delays |
| ✏️ **Mistake Simulation** | Automatically introduces and corrects typos (configurable) |
| 📝 **Line Break Preservation** | Types text exactly as formatted, with proper ENTER key presses |
| 🎮 **Global Hotkeys** | Control typing from anywhere: `F5` Start, `F8` Pause, `F9` Resume, `F10` Stop, `F11` Status |
| ⚙️ **Customizable Hotkeys** | Change all hotkeys to your preference |
| 📋 **Multiple Input Methods** | Direct input, clipboard paste, Notepad import |
| 🎚️ **Adjustable Settings** | 5 speed levels, 6 mistake levels (None to Lots + Random) |
| 🎨 **Modern Borderless GUI** | Clean, dark‑themed interface |
| 📌 **Always on Top** | Stays visible while you work |
| 📦 **Self‑Contained EXE** | No installation required, runs on any Windows PC |

---

## 📥 Download

### **▶️ [DOWNLOAD FROM ARCHIVE.ORG](https://archive.org/details/creators-eye-human-typer)**

**Direct link:** https://archive.org/details/creators-eye-human-typer

The download includes:
- `CEautoTyper.exe` – Main executable (single file, ~60 MB)
- `README.txt` – Quick start guide

---

## 📋 Quick Start

1. **Download** `CEautoTyper.exe` from the link above.
2. **Double‑click** to run (no installation needed).
3. **Paste or type** your text in the input box.
4. **Click START** or press `F5`.
5. **Click on your target window** (Notepad, browser, etc.) within 5 seconds.
6. **Watch it type** automatically!

---

## 🎮 Hotkeys

| Key  | Function | Description |
|------|----------|-------------|
| `F5` | Start    | Begin typing with current settings |
| `F8` | Pause    | Temporarily stop typing |
| `F9` | Resume   | Continue typing from where paused |
| `F10`| Stop     | End typing session |
| `F11`| Status   | Show current status in log |

*All hotkeys are customizable via the **Set Hotkeys** button.*

---

## ⚙️ Settings

### Speed Levels
| Level        | Delay per character |
|--------------|---------------------|
| 🐢 Very Slow | 200‑300 ms          |
| 🚶 Slow      | 120‑200 ms          |
| ⚡ Medium    | 60‑120 ms *(default)* |
| 🏃 Fast      | 30‑70 ms            |
| 🚀 Very Fast | 15‑40 ms            |

### Mistake Levels
| Level   | Mistakes |
|---------|----------|
| ✅ None | 0        |
| 🔹 Few  | 1‑2      |
| 🔸 Some | 3‑4      |
| ⚠️ Many | 5‑7      |
| ❗ Lots | 8‑10     |
| 🎲 Random | Variable range |

---

## 🖼️ Screenshots

| Main Interface | Hotkey Settings | Typing in Action |
|---------------|-----------------|------------------|
| ![Main](docs/screenshots/main.png) | ![Hotkeys](docs/screenshots/hotkeys.png) | ![Typing](docs/screenshots/typing.png) |

---

## 🛠️ Building from Source

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- Windows 7 or higher (64‑bit)
- Visual Studio 2022 / VS Code (optional)

### Build Instructions
```bash
# Clone the repository
git clone https://github.com/yourusername/CreatorsEye-AutoTyper.git
cd CreatorsEye-AutoTyper

# Restore dependencies
dotnet restore

# Build the project
dotnet build -c Release

# Publish as self‑contained single EXE
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
