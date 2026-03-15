# 👁️ Creators Eye Human-Like Typer v1.0

![PowerShell](https://img.shields.io/badge/PowerShell-5.1+-blue.svg)
![Platform](https://img.shields.io/badge/Platform-Windows-lightgrey.svg)
![Version](https://img.shields.io/badge/Version-1.0-green.svg)

A sophisticated PowerShell tool that simulates human typing patterns with realistic mistakes, pauses, and customizable settings. Perfect for demonstrations, testing, or automating text input with a human touch.

## ✨ Features

### 🎯 Core Features
- **Human-like Typing**: Simulates natural typing speeds and patterns
- **Mistake Simulation**: Automatically creates and corrects typos
- **Real-time Control**: Pause, resume, stop, and check status with hotkeys

### ⚙️ Customizable Settings
- **Speed Profiles**: Very Slow, Slow, Medium, Fast, Very Fast
- **Mistake Levels**: 0 to 10+ mistakes with random option
- **Hotkey Configuration**: Customize all control keys
- **Text Input Options**: Notepad, Clipboard, or Default text

### 🎮 Hotkeys (Default)
| Function | Default Key |
|----------|------------|
| Pause | F8 |
| Resume | F9 |
| Stop | F10 |
| Status | F11 |

## 📥 Installation

### Prerequisites
- Windows Operating System
- PowerShell 5.1 or higher

### Method 1: Direct Download
```powershell
# Download and run
Invoke-WebRequest -Uri "https://raw.githubusercontent.com/YOUR_USERNAME/Creators-Eye-Human-Like-Typer/main/Human-Like-Typer.ps1" -OutFile "Human-Like-Typer.ps1"
.\Human-Like-Typer.ps1
