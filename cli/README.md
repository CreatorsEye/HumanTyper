# 🖥️ Creators Eye Auto Typer – CLI Version

A PowerShell script that simulates human typing with configurable speed, mistakes, and line break preservation.

## 🚀 Quick Start

1. Open PowerShell in the `cli` folder
2. Run: `.\Human-Like-Typer.ps1`
3. Follow the menu

## 📋 Menu Options

| Option | Description |
|--------|-------------|
| **1. Input text via Notepad** | Opens Notepad to enter text |
| **2. Paste from Clipboard** | Loads text from clipboard |
| **3. Load default text** | Loads sample text |
| **4. Preview text** | Shows current text |
| **5. Change speed** | Very Slow to Very Fast |
| **6. Change mistakes** | None to Random |
| **7. Configure hotkeys** | Change F8-F11 |
| **8. Reset to defaults** | Clear all settings |
| **9. START TYPING** | Begin typing |
| **10. Exit** | Close script |

## 🎮 Default Hotkeys

| Key | Action |
|-----|--------|
| `F8` | Pause |
| `F9` | Resume |
| `F10` | Stop |
| `F11` | Status |

## 📝 Notes

- Line breaks send ENTER key
- Punctuation has slight delay
- Mistakes are corrected automatically
- Works in any application

## 💡 Troubleshooting

- Click target window during 5s countdown
- Hotkeys work only when script is active
- Antivirus may flag SendKeys (false positive)
