# 🖥️ Creators Eye Auto Typer – CLI Version

A PowerShell script that simulates human typing with configurable speed, mistakes, and line break preservation.

---

## 📥 Two Ways to Get It

### Option A: Download
* [`Human-Like-Typer.ps1`](Human-Like-Typer.ps1) – Right-click and save

### Option B: Copy/Paste
1. **Open** Notepad.
2. **Copy** the script from [Human-Like-Typer.ps1](Human-Like-Typer.ps1).
3. **Paste** and save as `.ps1`.

---

## 🚀 Quick Start
👉 **For detailed instructions, see the [CLI Quick Guide](../docs/cli-quick-guide.md)**

```powershell
.\Human-Like-Typer.ps1
```

---

## 📋 Menu Options

| Option | Description |
|:---|:---|
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

---

## 🎮 Default Hotkeys

| Key | Action |
|:---|:---|
| `F8` | Pause |
| `F9` | Resume |
| `F10` | Stop |
| `F11` | Status |

---

## 📝 Notes
* Line breaks send **ENTER** key.
* Punctuation has a slight delay.
* Mistakes are corrected automatically.
* Works in any application.

---

## 📚 Documentation
* [**CLI Quick Guide**](../docs/cli-quick-guide.md) – Step-by-step instructions.
* [**Main README**](../README.md) – Project overview.

---

## 💡 Troubleshooting
* **Click target window** during 5s countdown.
* **Hotkeys work** only when script is active.
* **Antivirus** may flag `SendKeys` (false positive).

---

# 🎨 Creators Eye Auto Typer – GUI Version

A standalone Windows application with a modern interface, global hotkeys, and full typing automation.

---

## 📥 Download
* **Direct Download:** [`CEautoTyper.exe`](https://archive.org/download/creators-eye-human-typer/CEautoTyper.exe) (~60 MB)
* **Alternative:** [Archive.org Page](https://archive.org/details/creators-eye-human-typer)

---

## 🚀 Quick Start
👉 **For detailed instructions, see the [GUI Quick Guide](../docs/gui-quick-guide.md)**

1. **Download** and double-click `CEautoTyper.exe`.
2. **Paste or type** your text.
3. Click **START** or press `F5`.
4. **Click target window** within 5 seconds.
5. Watch it type!

---

## 🎮 Global Hotkeys

| Key | Action |
|:---|:---|
| `F5` | Start |
| `F8` | Pause |
| `F9` | Resume |
| `F10` | Stop |
| `F11` | Status |

*Click **Set Hotkeys** to customize.*

---

## ⚙️ Settings

**Speed Levels:**
* Very Slow (200-300ms)
* Slow (120-200ms)
* Medium (60-120ms)
* Fast (30-70ms)
* Very Fast (15-40ms)

**Mistake Levels:**
* None (0)
* Few (1-2)
* Some (3-4)
* Many (5-7)
* Lots (8-10)
* Random

---

## 📝 Input Methods
* Type directly.
* Paste from clipboard.
* Import from Notepad.

---

## 📚 Documentation
* [**GUI Quick Guide**](../docs/gui-quick-guide.md) – Step-by-step instructions.
* [**Main README**](../README.md) – Project overview.

---

## 🔧 Build from Source
```bash
cd src
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

---

## ❓ Troubleshooting
* **Click target** during countdown.
* **Change hotkeys** if conflicts occur.
* **Add antivirus exclusion** if needed.
