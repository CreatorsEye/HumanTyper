# 🖥️ Creators Eye Auto Typer – CLI Version

A PowerShell script that simulates human typing with configurable speed, mistakes, and line-break preservation.

## 🚀 Quick Start

1. Open PowerShell and navigate to the `cli` folder.
2. Run the script:
   ```powershell
   .\Human-Like-Typer.ps1
   ```
3. Follow the menu.

---

## 📋 Menu Options

| Option | Description |
|:---|:---|
| **1. Input text via Notepad** | Opens Notepad; type/paste, save, and close to load text. |
| **2. Paste from Clipboard** | Loads text directly from clipboard. |
| **3. Load default text** | Loads a sample sentence. |
| **4. Preview text** | Displays currently loaded text. |
| **5. Change speed** | Select from Very Slow, Slow, Medium, Fast, Very Fast. |
| **6. Change mistakes** | Choose mistake level: None, Few, Some, Many, Lots, Random. |
| **7. Configure hotkeys** | Change keys for Pause, Resume, Stop, Status. |
| **8. Reset to defaults** | Clears all settings. |
| **9. START TYPING** | Begins the typing session. |
| **10. Exit** | Quits the script. |

---

## 🎮 Hotkeys During Typing

| Key (default) | Action |
|:---|:---|
| `F8` | Pause |
| `F9` | Resume |
| `F10` | Stop |
| `F11` | Show status |

*Hotkeys can be changed via option 7.*

---

## 📝 Notes

* **Line breaks** are preserved – the script sends an `ENTER` key when it encounters `\n` or `\r\n`.
* **Punctuation** (`:;,.[ ]`) is typed with a slight delay to mimic human behavior.
* **Mistakes** are randomly introduced based on the selected level; the script then corrects them.
* The script uses `SendKeys` to simulate typing, so it will work in any application that accepts keyboard input.

---

## 💡 Troubleshooting

**Nothing happens after selecting Start?**
Make sure you have loaded some text and that you click on the target window during the 5‑second countdown.

**Hotkeys not working?**
The script only listens for hotkeys while the PowerShell window is active. For global hotkeys, use the GUI version.

**Antivirus warnings?**
Some antivirus software may flag PowerShell scripts using `SendKeys`. This is a false positive; the script is safe.

---

## 📁 File: `cli/Human-Like-Typer.ps1`

This is the full PowerShell script. Due to its length, ensure you have the complete script file in this directory.

