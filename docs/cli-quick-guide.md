# 🖥️ Creators Eye Auto Typer – CLI Quick Guide

## 📥 Two Ways to Get the CLI Version

### Option A: Download the Script (Easiest)
* Get `Human-Like-Typer.ps1` from the `cli` folder.
* Save it anywhere on your computer.

### Option B: Copy/Paste the Code (No Download Needed)
1. **Open Notepad**.
2. **Copy** the entire PowerShell script from:
   * GitHub (`cli/Human-Like-Typer.ps1`)
   * Or from the project's source code blocks.
3. **Paste** into Notepad.
4. **Save as** `Human-Like-Typer.ps1` (ensure the extension is `.ps1`, not `.txt`).

---

## 🚀 Quick Start Guide

### Step 1: Open PowerShell
* Press `Win + R`, type `powershell`, and press **Enter**.
* Navigate to your script folder:
  ```powershell
  cd C:\path\to\script\folder
  ```

### Step 2: Run the Script
```powershell
.\Human-Like-Typer.ps1
```
*If a security warning appears, type `R` to run once.*

### Step 3: Main Menu
```text
╔═══════════════════════════════════════════╗
║     CREATORS EYE HUMAN-LIKE TYPER v1.0    ║
╠═══════════════════════════════════════════╣
║ TEXT: 42 words loaded                     ║
║ SPEED: Medium                             ║
║ MISTAKES: 3-4                             ║
║ HOTKEYS: Pause=F8, Resume=F9, Stop=F10... ║
╠═══════════════════════════════════════════╣
║ 1. Input text via Notepad                 ║
║ 2. Paste from Clipboard                   ║
║ 3. Load default text                      ║
║ 4. Preview text                           ║
║ 5. Change speed                           ║
║ 6. Change mistakes                        ║
║ 7. Configure hotkeys                      ║
║ 8. Reset to defaults                      ║
║ 9. START TYPING                           ║
║ 10. Exit                                  ║
╚═══════════════════════════════════════════╝
```

### Step 4: Load Text
Choose one:
* **Option 1** – Opens Notepad; type/paste, save, and close.
* **Option 2** – Pastes directly from your clipboard.
* **Option 3** – Loads sample text.

### Step 5: Adjust Settings
* **Option 5** – Change speed (1=Very Slow to 5=Very Fast).
* **Option 6** – Change mistakes (0=None to 5=Random).

### Step 6: Start Typing
* Choose **Option 9**.
* You have **5 seconds** to click on your target window.
* Watch it type!

---

## 🎮 Control & Hotkeys

### Hotkeys During Typing
| Key | Action |
|:---|:---|
| `F8` | Pause |
| `F9` | Resume |
| `F10` | Stop |
| `F11` | Show status |

### Hotkey Configuration
Choose **Option 7** from the main menu:
```text
Current Hotkeys:
  Pause  : F8
  Resume : F9
  Stop   : F10
  Status : F11

Options:
  1. Change Pause key
  2. Change Resume key
  3. Change Stop key
  4. Change Status key
  5. Reset to defaults
  6. Back to main menu
```
*Enter the option number, then press the new key (e.g., F7).*

---

## ⚙️ Settings Reference

### Speed Options
| Choice | Speed Level | Delay per character |
|:---:|:---|:---|
| 1 | Very Slow | 200-300ms |
| 2 | Slow | 120-200ms |
| 3 | **Medium** | 60-120ms |
| 4 | Fast | 30-70ms |
| 5 | Very Fast | 15-40ms |

### Mistake Options
| Choice | Mistake Level | Mistakes |
|:---:|:---|:---|
| 0 | None | 0 |
| 1 | Few | 1-2 |
| 2 | Some | 3-4 |
| 3 | Many | 5-7 |
| 4 | Lots | 8-10 |
| 5 | Random | Variable |

---

## 📝 Example Session
```text
Select option: 3
Default text loaded! Total words: 12

Select option: 5
Select speed (1-5): 2
Speed set to: Slow

Select option: 9
READY TO TYPE
Words: 12
Speed: Slow
Target mistakes: 6
Click target window. Starting in 5 seconds...
5... 4... 3... 2... 1...
TYPING STARTED
==================================================
✓ This
✓ is
⚠️ TYPO: simpole
   Fixing...
   Correct: simple
✓ auto
✓ writer...
==================================================
COMPLETE! Made 6 mistakes
Type again? (Y/N): N
```

---

## ❓ Troubleshooting

| Problem | Solution |
|:---|:---|
| **"Scripts is disabled" error** | Run PowerShell as Admin and use: `Set-ExecutionPolicy RemoteSigned -Scope CurrentUser` |
| **Nothing happens at Start** | Ensure you clicked the target window during the 5-second countdown. |
| **Hotkeys don't work** | The PowerShell window must be active. Click it before using hotkeys. |
| **No line breaks** | Ensure your text has actual line breaks (Enter key) in the loaded source. |
| **Antivirus warning** | False positive—the script is safe and uses `SendKeys` to simulate typing. |

---

## 💡 Pro Tips
* **Keep the PowerShell window visible** to see live status and typo corrections.
* **Test in Notepad first** before using in important applications.
* **Use Clipboard (Option 2)** for the fastest loading.
* **Note:** Hotkeys only register when the PowerShell window has focus.
* **Copy/paste the code** if you can't download—it's the exact same script!

---

## 🔗 Links
* **GitHub Repository:** [CreatorsEye-AutoTyper](https://github.com/yourusername/CreatorsEye-AutoTyper)
* **GUI Version Guide:** [gui-quick-guide.md](gui-quick-guide.md)
* **Direct Script Link:** [Human-Like-Typer.ps1](Human-Like-Typer.ps1)

---

## 📧 Need Help?
* **Open an issue** on GitHub.
* Check the **troubleshooting section** above.

**Happy typing!** 🚀
