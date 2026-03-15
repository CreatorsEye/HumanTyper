# COMPLETE CUSTOMIZABLE HUMAN-LIKE TYPER - CLEAN VERSION
Clear-Host
Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║         Creators Eye HUMAN-LIKE TYPER v1.0                 ║" -ForegroundColor Cyan
Write-Host "╠════════════════════════════════════════════════════════════╣" -ForegroundColor Cyan
Write-Host "║  • Organized menu                                          ║" -ForegroundColor Yellow
Write-Host "║  • All settings in one place                               ║" -ForegroundColor Yellow
Write-Host "║  • Quick access to text input                              ║" -ForegroundColor Yellow
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

# Load Windows Forms
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

# Global variables
$script:CurrentText = ""
$script:Words = @()
$script:TotalWords = 0
$script:Paused = $false
$script:StopTyping = $false
$script:CurrentSpeed = "Medium"
$script:CurrentMistakeRange = @(0,0)  # CHANGED: Default to 0 mistakes

# Hotkey definitions
$script:Hotkeys = @{
    "Pause" = "F8"
    "Resume" = "F9"
    "Stop" = "F10"
    "Status" = "F11"
}

# Speed profiles
$speedProfiles = @{
    "Very Slow" = 200..300
    "Slow" = 120..200
    "Medium" = 60..120
    "Fast" = 30..70
    "Very Fast" = 15..40
}

# Function to reset all settings to default
function Reset-ToDefaults {
    Clear-Host
    Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║                    RESET TO DEFAULTS                       ║" -ForegroundColor Cyan
    Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
    
    Write-Host "`nResetting all settings to default values..." -ForegroundColor Yellow
    
    # Reset variables
    $script:CurrentText = ""
    $script:Words = @()
    $script:TotalWords = 0
    $script:Paused = $false
    $script:StopTyping = $false
    $script:CurrentSpeed = "Medium"
    $script:CurrentMistakeRange = @(0,0)
    
    # Reset hotkeys to defaults
    $script:Hotkeys = @{
        "Pause" = "F8"
        "Resume" = "F9"
        "Stop" = "F10"
        "Status" = "F11"
    }
    
    Write-Host "`n✓ Settings reset complete!" -ForegroundColor Green
    Write-Host "  • Speed: Medium"
    Write-Host "  • Mistakes: 0"
    Write-Host "  • Text: Cleared"
    Write-Host "  • Hotkeys: Default (F8-F11)"
    
    Start-Sleep 3
}

# Function to show current settings
function Show-CurrentSettings {
    Write-Host "`n" + "═"*60 -ForegroundColor Cyan
    Write-Host "📊 CURRENT SETTINGS" -ForegroundColor White
    Write-Host "═"*60 -ForegroundColor Cyan
    if ($script:CurrentText -ne "") {
        Write-Host "📝 TEXT: $script:TotalWords words loaded" -ForegroundColor Green
    } else {
        Write-Host "📝 TEXT: No text loaded" -ForegroundColor Red
    }
    Write-Host "⚡ SPEED: $script:CurrentSpeed" -ForegroundColor Yellow
    Write-Host "❌ MISTAKES: $($script:CurrentMistakeRange[0])-$($script:CurrentMistakeRange[1])" -ForegroundColor Yellow
    Write-Host "🎮 HOTKEYS: Pause=$($script:Hotkeys['Pause']), Resume=$($script:Hotkeys['Resume']), Stop=$($script:Hotkeys['Stop']), Status=$($script:Hotkeys['Status'])" -ForegroundColor Gray
    Write-Host "═"*60 -ForegroundColor Cyan
}

# Function to configure hotkeys
function Set-Hotkeys {
    Clear-Host
    Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║                    HOTKEY CONFIGURATION                    ║" -ForegroundColor Cyan
    Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
    
    Write-Host "`nCurrent Hotkeys:" -ForegroundColor Yellow
    Write-Host "  Pause  : $($script:Hotkeys['Pause'])"
    Write-Host "  Resume : $($script:Hotkeys['Resume'])"
    Write-Host "  Stop   : $($script:Hotkeys['Stop'])"
    Write-Host "  Status : $($script:Hotkeys['Status'])"
    
    Write-Host "`nOptions:" -ForegroundColor Green
    Write-Host "  1. Change Pause key"
    Write-Host "  2. Change Resume key"
    Write-Host "  3. Change Stop key"
    Write-Host "  4. Change Status key"
    Write-Host "  5. Reset to defaults"
    Write-Host "  6. Back to main menu"
    
    $choice = Read-Host "`nSelect option"
    
    switch ($choice) {
        '1' { 
            $newKey = Read-Host "Enter new Pause key (F8, F9, etc.)"
            $script:Hotkeys['Pause'] = $newKey
            Write-Host "Pause key changed to $newKey" -ForegroundColor Green
            Start-Sleep 1
        }
        '2' { 
            $newKey = Read-Host "Enter new Resume key (F8, F9, etc.)"
            $script:Hotkeys['Resume'] = $newKey
            Write-Host "Resume key changed to $newKey" -ForegroundColor Green
            Start-Sleep 1
        }
        '3' { 
            $newKey = Read-Host "Enter new Stop key (F8, F9, etc.)"
            $script:Hotkeys['Stop'] = $newKey
            Write-Host "Stop key changed to $newKey" -ForegroundColor Green
            Start-Sleep 1
        }
        '4' { 
            $newKey = Read-Host "Enter new Status key (F8, F9, etc.)"
            $script:Hotkeys['Status'] = $newKey
            Write-Host "Status key changed to $newKey" -ForegroundColor Green
            Start-Sleep 1
        }
        '5' {
            $script:Hotkeys = @{
                "Pause" = "F8"
                "Resume" = "F9"
                "Stop" = "F10"
                "Status" = "F11"
            }
            Write-Host "Hotkeys reset to defaults" -ForegroundColor Green
            Start-Sleep 1
        }
    }
}

# Function to check hotkeys during typing
function Check-Hotkeys {
    if ([System.Console]::KeyAvailable) {
        $key = [System.Console]::ReadKey($true)
        
        if ($key.Key -eq $script:Hotkeys['Pause']) {
            $script:Paused = $true
            Write-Host "`n⏸️ PAUSED - Press $($script:Hotkeys['Resume']) to resume" -ForegroundColor Yellow
            [System.Console]::Beep(500,200)
            return $true
        }
        
        if ($key.Key -eq $script:Hotkeys['Resume'] -and $script:Paused) {
            $script:Paused = $false
            Write-Host "▶️ RESUMED" -ForegroundColor Green
            [System.Console]::Beep(800,200)
            return $true
        }
        
        if ($key.Key -eq $script:Hotkeys['Stop']) {
            $script:StopTyping = $true
            Write-Host "`n🛑 STOPPED" -ForegroundColor Red
            [System.Console]::Beep(300,500)
            return $true
        }
        
        if ($key.Key -eq $script:Hotkeys['Status']) {
            Write-Host "`n📊 Status: $(if($script:Paused){'PAUSED'}else{'RUNNING'})" -ForegroundColor Cyan
            return $true
        }
    }
    return $false
}

# Function to input text via Notepad
function Input-Text {
    Clear-Host
    Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║                    TEXT INPUT - NOTEPAD                    ║" -ForegroundColor Cyan
    Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
    
    $tempFile = [System.IO.Path]::GetTempFileName() + ".txt"
    
    Write-Host "`n1. Notepad will open"
    Write-Host "2. Paste or type your text"
    Write-Host "3. Save (Ctrl+S) and close notepad"
    
    Read-Host "`nPress Enter to open notepad"
    notepad.exe $tempFile
    
    Write-Host "Waiting for notepad to close..." -ForegroundColor Yellow
    do {
        Start-Sleep -Milliseconds 500
        $process = Get-Process notepad -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowTitle -like "*$tempFile*" }
    } while ($process)
    
    if (Test-Path $tempFile) {
        $newText = Get-Content $tempFile -Raw -Encoding utf8
        Remove-Item $tempFile
        
        if ($newText) {
            $script:CurrentText = $newText
            $script:Words = $script:CurrentText -split '\s+'
            $script:TotalWords = $script:Words.Count
            Write-Host "`nText loaded! Total words: $script:TotalWords" -ForegroundColor Green
        }
    }
    Read-Host "`nPress Enter to continue"
}

# Function to paste from clipboard
function Input-Text-Clipboard {
    Clear-Host
    Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║                 TEXT INPUT - CLIPBOARD                     ║" -ForegroundColor Cyan
    Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
    
    Write-Host "`n1. Copy your text to clipboard (Ctrl+C)"
    Write-Host "2. Press Enter to load it"
    
    Read-Host "`nPress Enter when ready"
    
    try {
        $newText = Get-Clipboard -Raw -ErrorAction Stop
        if ($newText) {
            $script:CurrentText = $newText
            $script:Words = $script:CurrentText -split '\s+'
            $script:TotalWords = $script:Words.Count
            Write-Host "`nText loaded! Total words: $script:TotalWords" -ForegroundColor Green
        } else {
            Write-Host "`nClipboard is empty" -ForegroundColor Red
        }
    }
    catch {
        Write-Host "`nFailed to get clipboard" -ForegroundColor Red
    }
    Read-Host "`nPress Enter to continue"
}

# Function to set default text - UPDATED WITH YOUR TEXT
function Set-DefaultText {
    $script:CurrentText = @"
This is simple auto writer tool used for good propose. Free to use. Great to achieve your goals.
"@
    $script:Words = $script:CurrentText -split '\s+'
    $script:TotalWords = $script:Words.Count
    Write-Host "Default text loaded! Total words: $script:TotalWords" -ForegroundColor Green
    Start-Sleep 2
}

# Function to set speed
function Set-Speed {
    Clear-Host
    Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║                    SPEED SETTINGS                          ║" -ForegroundColor Cyan
    Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
    
    Write-Host "`nCurrent speed: $script:CurrentSpeed" -ForegroundColor Yellow
    Write-Host "`n1. Very Slow"
    Write-Host "2. Slow"
    Write-Host "3. Medium"
    Write-Host "4. Fast"
    Write-Host "5. Very Fast"
    
    $choice = Read-Host "`nSelect speed (1-5)"
    
    switch ($choice) {
        '1' { $script:CurrentSpeed = "Very Slow" }
        '2' { $script:CurrentSpeed = "Slow" }
        '3' { $script:CurrentSpeed = "Medium" }
        '4' { $script:CurrentSpeed = "Fast" }
        '5' { $script:CurrentSpeed = "Very Fast" }
    }
    Write-Host "Speed set to: $script:CurrentSpeed" -ForegroundColor Green
    Start-Sleep 1
}

# Function to set mistakes
function Set-Mistakes {
    Clear-Host
    Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║                   MISTAKE SETTINGS                         ║" -ForegroundColor Cyan
    Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
    
    Write-Host "`nCurrent mistakes: $($script:CurrentMistakeRange[0])-$($script:CurrentMistakeRange[1])" -ForegroundColor Yellow
    Write-Host "`n0. No mistakes"
    Write-Host "1. 1-2 mistakes"
    Write-Host "2. 3-4 mistakes"
    Write-Host "3. 5-7 mistakes"
    Write-Host "4. 8-10 mistakes"
    Write-Host "5. Random"
    
    $choice = Read-Host "`nSelect (0-5)"
    
    switch ($choice) {
        '0' { $script:CurrentMistakeRange = @(0,0) }
        '1' { $script:CurrentMistakeRange = @(1,2) }
        '2' { $script:CurrentMistakeRange = @(3,4) }
        '3' { $script:CurrentMistakeRange = @(5,7) }
        '4' { $script:CurrentMistakeRange = @(8,10) }
        '5' { 
            $min = Get-Random -Minimum 0 -Maximum 5
            $max = $min + (Get-Random -Minimum 1 -Maximum 4)
            $script:CurrentMistakeRange = @($min, $max)
        }
    }
    Write-Host "Mistakes set to: $($script:CurrentMistakeRange[0])-$($script:CurrentMistakeRange[1])" -ForegroundColor Green
    Start-Sleep 1
}

# Function to preview text
function Show-TextPreview {
    Clear-Host
    Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║                     TEXT PREVIEW                           ║" -ForegroundColor Cyan
    Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
    
    if ($script:CurrentText) {
        Write-Host "`n$script:CurrentText" -ForegroundColor White
    } else {
        Write-Host "`nNo text loaded" -ForegroundColor Red
    }
    Read-Host "`nPress Enter to continue"
}

# Function to make typos
function Make-Typo {
    param([string]$word)
    
    if ($word.Length -le 1) { return $word }
    
    $typo = $word
    $typoType = Get-Random -Minimum 1 -Maximum 4
    
    switch ($typoType) {
        1 { # Double letter
            if ($word.Length -gt 2) {
                $pos = Get-Random -Minimum 1 -Maximum ($word.Length)
                $typo = $word.Substring(0, $pos) + $word[$pos-1] + $word.Substring($pos-1)
            }
        }
        2 { # Skip letter
            if ($word.Length -gt 3) {
                $pos = Get-Random -Minimum 1 -Maximum ($word.Length - 1)
                $typo = $word.Remove($pos, 1)
            }
        }
        3 { # Swap letters
            if ($word.Length -gt 3) {
                $pos = Get-Random -Minimum 0 -Maximum ($word.Length - 2)
                $char1 = $word[$pos]
                $char2 = $word[$pos + 1]
                $typo = $word.Remove($pos, 2).Insert($pos, "$char2$char1")
            }
        }
    }
    
    return $typo
}

# Main typing function
function Start-Typing {
    if ($script:CurrentText -eq "") {
        Write-Host "No text loaded!" -ForegroundColor Red
        Start-Sleep 2
        return
    }
    
    $targetMistakes = Get-Random -Minimum $script:CurrentMistakeRange[0] -Maximum ($script:CurrentMistakeRange[1] + 1)
    $mistakesMade = 0
    $script:Paused = $false
    $script:StopTyping = $false
    
    Clear-Host
    Write-Host "READY TO TYPE" -ForegroundColor Cyan
    Write-Host "Words: $script:TotalWords"
    Write-Host "Speed: $script:CurrentSpeed"
    Write-Host "Target mistakes: $targetMistakes"
    Write-Host "`nHotkeys: Pause=$($script:Hotkeys['Pause']), Resume=$($script:Hotkeys['Resume']), Stop=$($script:Hotkeys['Stop'])"
    Write-Host "`nClick target window. Starting in 5 seconds..."
    
    5..1 | ForEach-Object {
        Write-Host "$_..." -ForegroundColor Cyan
        Start-Sleep 1
    }
    
    Write-Host "`nTYPING STARTED" -ForegroundColor Green
    Write-Host "="*50
    
    $wordCount = 0
    $parts = $script:CurrentText -split '(\s+)'
    
    foreach ($part in $parts) {
        # Check stop flag
        if ($script:StopTyping) { break }
        
        # Handle pause
        while ($script:Paused) {
            Start-Sleep -Milliseconds 100
            if ($script:StopTyping) { break }
        }
        
        # Check hotkeys
        Check-Hotkeys
        
        if ($part -match '\S') {
            $wordCount++
            $word = $part
            
            # Decide if mistake
            $makeMistake = $false
            if ($mistakesMade -lt $targetMistakes -and $wordCount -gt 2) {
                if ((Get-Random) -lt 0.3) {
                    $makeMistake = $true
                    $mistakesMade++
                }
            }
            
            if ($makeMistake) {
                $typo = Make-Typo -word $word
                Write-Host "⚠️ TYPO: $typo" -ForegroundColor Red
                
                # Type typo
                foreach ($char in $typo.ToCharArray()) {
                    if ($script:StopTyping) { break }
                    while ($script:Paused) { Start-Sleep 100 }
                    [System.Windows.Forms.SendKeys]::SendWait($char)
                    Start-Sleep -Milliseconds (Get-Random -Minimum $speedProfiles[$script:CurrentSpeed][0] -Maximum $speedProfiles[$script:CurrentSpeed][1])
                }
                
                if (-not $script:StopTyping) {
                    Start-Sleep -Milliseconds 300
                    Write-Host "   Fixing..." -ForegroundColor Yellow
                    
                    # Backspace
                    for ($i = 0; $i -lt $typo.Length; $i++) {
                        if ($script:StopTyping) { break }
                        [System.Windows.Forms.SendKeys]::SendWait("{BACKSPACE}")
                        Start-Sleep -Milliseconds 50
                    }
                    
                    Start-Sleep -Milliseconds 400
                    Write-Host "   Correct: $word" -ForegroundColor Green
                    
                    # Type correctly
                    foreach ($char in $word.ToCharArray()) {
                        if ($script:StopTyping) { break }
                        [System.Windows.Forms.SendKeys]::SendWait($char)
                        Start-Sleep -Milliseconds (Get-Random -Minimum $speedProfiles[$script:CurrentSpeed][0] -Maximum $speedProfiles[$script:CurrentSpeed][1])
                    }
                }
            } else {
                Write-Host "✓ $word" -ForegroundColor Green
                foreach ($char in $word.ToCharArray()) {
                    if ($script:StopTyping) { break }
                    while ($script:Paused) { Start-Sleep 100 }
                    [System.Windows.Forms.SendKeys]::SendWait($char)
                    Start-Sleep -Milliseconds (Get-Random -Minimum $speedProfiles[$script:CurrentSpeed][0] -Maximum $speedProfiles[$script:CurrentSpeed][1])
                }
            }
            
            Start-Sleep -Milliseconds 100
        }
        else {
            if ($part -match " ") {
                [System.Windows.Forms.SendKeys]::SendWait(" ")
            }
            elseif ($part -match "[:;,.]") {
                [System.Windows.Forms.SendKeys]::SendWait($part)
                Start-Sleep -Milliseconds 300
            }
            elseif ($part -match "\r\n|\n") {
                [System.Windows.Forms.SendKeys]::SendWait("{ENTER}")
                Start-Sleep -Milliseconds 400
            }
        }
    }
    
    Write-Host "="*50
    if (-not $script:StopTyping) {
        Write-Host "COMPLETE! Made $mistakesMade mistakes" -ForegroundColor Green
    }
    [System.Console]::Beep(800,300)
}

# Main menu loop
do {
    Clear-Host
    Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║              HUMAN-LIKE TYPER - MAIN MENU                  ║" -ForegroundColor Cyan
    Write-Host "║                Mad by Creators Eye v1.0                    ║" -ForegroundColor Cyan
    Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
    
    Show-CurrentSettings
    
    Write-Host "`nTEXT MANAGEMENT:" -ForegroundColor Green
    Write-Host "  1. Input text via Notepad"
    Write-Host "  2. Paste from Clipboard"
    Write-Host "  3. Load default text"
    Write-Host "  4. Preview text"
    
    Write-Host "`nSETTINGS:" -ForegroundColor Yellow
    Write-Host "  5. Change speed"
    Write-Host "  6. Change mistakes"
    Write-Host "  7. Configure hotkeys"
    
    Write-Host "`nSYSTEM:" -ForegroundColor Magenta
    Write-Host "  8. Reset to defaults (NEW)" -ForegroundColor Cyan
    
    Write-Host "`nACTIONS:" -ForegroundColor Magenta
    Write-Host "  9. START TYPING"
    Write-Host "  10. Exit"
    
    $choice = Read-Host "`nSelect option"
    
    switch ($choice) {
        '1' { Input-Text }
        '2' { Input-Text-Clipboard }
        '3' { Set-DefaultText }
        '4' { Show-TextPreview }
        '5' { Set-Speed }
        '6' { Set-Mistakes }
        '7' { Set-Hotkeys }
        '8' { Reset-ToDefaults }  # NEW: Reset option
        '9' { 
            Start-Typing
            $again = Read-Host "Type again? (Y/N)"
            while ($again -eq 'Y') {
                Start-Typing
                $again = Read-Host "Type again? (Y/N)"
            }
        }
    }
} while ($choice -ne '10')  # Changed from '9' to '10'

Write-Host "Goodbye!" -ForegroundColor Cyan
