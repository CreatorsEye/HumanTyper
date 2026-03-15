# <img src="https://raw.githubusercontent.com/yourusername/CreatorsEye-AutoTyper/main/docs/icon.png" width="32" height="32"> Creators Eye Auto Typer

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Windows](https://img.shields.io/badge/Platform-Windows%207%2B-blue)](https://github.com/yourusername/CreatorsEye-AutoTyper)
[![Downloads](https://img.shields.io/badge/Downloads-Archive.org-orange)](https://archive.org/details/creators-eye-human-typer)

A human-like typing automation tool that types text **exactly as written** – preserving line breaks, spacing, and formatting with realistic delays and mistakes.

---
## 📋 Quick Overview

| For | Choose | What you get |
|:---|:---|:---|
| **👤 End Users** | [**CLI Tool**](cli/) or [**GUI Tool**](gui/) | Ready-to-run tools, no coding needed |
| **👨‍💻 Developers** | [**Source Code**](gui/src/) | Build, modify, or contribute |
---

## 🎯 For End Users

### 🖥️ CLI Edition (PowerShell Script)
**Perfect for:** Users who want a lightweight, script-based tool with no downloads.

#### 📥 Get Started
| Step | Action |
|:---:|:---|
| **1** | Download [`Human-Like-Typer.ps1`](cli/Human-Like-Typer.ps1) |
| **2** | Right-click and select **"Run with PowerShell"** |
| **3** | Follow the on-screen menu |

#### 📚 Resources
* [CLI Quick Start Guide](docs/cli-quick-guide.md) – Step-by-step instructions
* No installation required – works on any Windows PC

---

### 🎨 GUI Edition (Standalone Application)
**Perfect for:** Users who want a modern interface with global hotkeys.

#### 📥 Get Started
| Step | Action |
|:---:|:---|
| **1** | Download [`Creators Eye Human Typer.exe`](https://archive.org/download/creators-eye-human-typer/Creators%20Eye%20Human%20Typer.exe) (~60 MB) |
| **2** | Double-click to run – **no installation needed** |
| **3** | Paste your text and press `F5` |

#### 📚 Resources
* [GUI Quick Start Guide](docs/gui-quick-guide.md) – Detailed instructions
* [Archive.org Mirror](https://archive.org/details/creators-eye-human-typer) – Project page
* [Torrent Download](https://archive.org/download/creators-eye-human-typer/creators-eye-human-typer_archive.torrent)

---

## 👨‍💻 For Developers

### Source Code Access
| Version | Location | Technology |
|:---|:---|:---|
| **CLI Source** | [`cli/Human-Like-Typer.ps1`](cli/Human-Like-Typer.ps1) | PowerShell 5.1+ |
| **GUI Source** | [`gui/src/`](gui/src/) | C# / .NET 8.0 (Windows Forms) |

### 🛠️ Build the GUI Yourself
```bash
# Clone the repository
git clone https://github.com/yourusername/CreatorsEye-AutoTyper.git
cd CreatorsEye-AutoTyper/gui/src

# Restore dependencies
dotnet restore

# Build and publish as single EXE
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```
*The compiled EXE will be in:* `bin/Release/net8.0-windows/win-x64/publish/`

### 🔧 Modify and Extend
* **CLI Version:** Edit the PowerShell script directly – it's just text!
* **GUI Version:** Open `gui/src/` in Visual Studio or VS Code.

### 📁 Project Structure for Developers
```text
CreatorsEye-AutoTyper/
├── cli/                      # PowerShell source
│   └── Human-Like-Typer.ps1
├── gui/                      # C# source
│   └── src/
│       ├── Program.cs        # Entry point
│       ├── MainForm.cs       # Main logic (800+ lines)
│       └── CEautoTyper.csproj
├── docs/                     # Documentation
│   ├── cli-quick-guide.md
│   └── gui-quick-guide.md
├── releases/                 # Pre-built releases
│   └── v1.0/
│       ├── README.md
│       └── Capture.PNG
└── README.md
```

---

## ✨ Features at a Glance

| Feature | CLI | GUI |
|:---|:---:|:---:|
| Ready to run – no setup | ✅ | ✅ |
| 5 typing speeds | ✅ | ✅ |
| 6 mistake levels + Random | ✅ | ✅ |
| Line break preservation | ✅ | ✅ |
| Clipboard import | ✅ | ✅ |
| Notepad import | ✅ | ✅ |
| Global hotkeys | ❌ | ✅ |
| Customizable hotkeys | ✅ | ✅ |
| Always on top | ❌ | ✅ |
| Modern graphical interface | ❌ | ✅ |
| Open source | ✅ | ✅ |

---

## 📝 License
**MIT** © 2026 Creators Eye – Free for personal and commercial use.

---

## ⭐ Support the Project
* ⭐ **Star** this repository
* 📥 **Download** from Archive.org
* 🐛 **Report issues** on GitHub
* 💡 **Suggest features** via Issues

<br>
<p align="center">
  <b>Built for users, crafted for developers</b><br>
  <i>Type naturally, automatically! 🚀</i>
</p>
