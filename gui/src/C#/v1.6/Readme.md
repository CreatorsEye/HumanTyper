# v1.6 – Added Mistakes Simulation

## 🎯 What This Version Teaches

Building on v1.5, this version adds:

- ✅ Mistakes simulation – adds realistic typos while typing
- ✅ Mistake levels: None, Few, Some, Many, Lots, Random
- ✅ Each mistake: types wrong word → backspace → correct word
- ✅ Typo generation (double letter, missing letter, swap adjacent letters)
- ✅ Makes typing look more human and natural
- ✅ Random number generation for mistake count

---

## 📁 Files in This Version

| File | Purpose |
|------|---------|
| `Program.cs` | Application entry point |
| `MainForm.cs` | Main UI and typing logic |
| `CeAutoTyper.csproj` | Project configuration |
| `ce.ico` | Application icon |

---

## 🔑 Key Code Concepts

### 1. Mistakes dropdown UI

```csharp
cmbMistakes = new ComboBox
{
    Location = new Point(240, 143),
    Size = new Size(100, 25),
    DropDownStyle = ComboBoxStyle.DropDownList,
    FlatStyle = FlatStyle.Flat,
    BackColor = Color.FromArgb(45, 45, 50),
    ForeColor = Color.White
};
cmbMistakes.Items.AddRange(new string[] { "None", "Few", "Some", "Many", "Lots", "Random" });
cmbMistakes.SelectedIndex = 0;
```
2. Getting mistake count from dropdown
```csharp
private int GetMistakeCount()
{
    switch (cmbMistakes.SelectedIndex)
    {
        case 0: return 0;                    // None
        case 1: return rand.Next(1, 3);      // Few (1-2)
        case 2: return rand.Next(3, 5);      // Some (3-4)
        case 3: return rand.Next(5, 8);      // Many (5-7)
        case 4: return rand.Next(8, 11);     // Lots (8-10)
        case 5: return rand.Next(1, 11);     // Random (1-10)
        default: return 0;
    }
}
```
3. Typo generation (double letter, missing, swap)
```csharp
private string MakeTypo(string word)
{
    if (word.Length <= 2) return word;
    int type = rand.Next(1, 4);
    string typo = word;
    switch (type)
    {
        case 1: // double letter
            int pos1 = rand.Next(0, word.Length);
            typo = word.Insert(pos1, word[pos1].ToString());
            break;
        case 2: // missing letter
            int pos2 = rand.Next(0, word.Length - 1);
            typo = word.Remove(pos2, 1);
            break;
        case 3: // swap adjacent letters
            int pos3 = rand.Next(0, word.Length - 2);
            char c1 = word[pos3];
            char c2 = word[pos3 + 1];
            typo = word.Remove(pos3, 2).Insert(pos3, $"{c2}{c1}");
            break;
    }
    return typo;
}
```
4. Mistake logic during typing
```csharp
bool makeMistake = targetMistakes > 0 && mistakesMade < targetMistakes && rand.Next(100) < 40 && char.IsLetter(c);

if (makeMistake && i < text.Length - 1)
{
    int start = i, end = i;
    while (start > 0 && char.IsLetter(text[start - 1])) start--;
    while (end < text.Length - 1 && char.IsLetter(text[end + 1])) end++;
    string word = text.Substring(start, end - start + 1);
    
    if (word.Length > 2)
    {
        string typo = MakeTypo(word);
        
        // Type the typo
        foreach (char tc in typo)
        {
            SendUnicodeChar(tc);
            await Task.Delay(GetDelay());
        }
        
        // Backspace and correct
        await Task.Delay(200);
        for (int b = 0; b < typo.Length; b++)
        {
            SendBackspace();
            await Task.Delay(30);
        }
        await Task.Delay(150);
        
        foreach (char wc in word)
        {
            SendUnicodeChar(wc);
            await Task.Delay(GetDelay());
        }
        
        mistakesMade++;
        i = end;
        continue;
    }
}
```
5. Sending backspace key
```csharp
private void SendBackspace()
{
    var inputs = new INPUT[2];
    inputs[0] = CreateKeyInput(0x08, 0x0000); // Backspace down
    inputs[1] = CreateKeyInput(0x08, 0x0002); // Backspace up
    SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT)));
}
```
🚀 How to Build
```bash
dotnet publish -c Release --self-contained true -p:PublishSingleFile=true
```
The EXE will be in the publish/ folder.


## Download EXE

[CE Auto Typer v1.6 on Archive.org](https://archive.org/details/ce-auto-typer)

---

## Next Version

[v1.7](https://github.com/CreatorsEye/HumanTyper/tree/main/gui/src/C%23/v1.7) – Adds Fix Typo control (Yes/No)

---

## Related Documentation

- [Developer Guide](https://github.com/CreatorsEye/HumanTyper/blob/main/docs/developer-guide.md) – Complete learning path
- [User Guide](https://github.com/CreatorsEye/HumanTyper/blob/main/docs/user-guide.md) – How to use the tool

<p align="center">
  <b>Start from scratch. Reach advanced.</b><br>
  <i>Free. Open. For learning.</i>
</p>
