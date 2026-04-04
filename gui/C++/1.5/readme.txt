CE Auto Typer v1.5 marks a major leap forward in usability and stability by embracing native Windows components instead of fighting against custom UI limitations. This version delivers seamless multilingual typing, a sleek dark‑themed interface, and a cleaner codebase.

New in v1.5

Native OS Hijack (The Big Fix): Replaced ImGui’s text box with a native Windows EDIT Control, ensuring perfect Arabic shaping, vowels (Harakat), Chinese characters, and Right‑to‑Left cursor movements.

UI “Hole” Trick: Used ImGui::Dummy() to carve out an invisible space, embedding the Windows text box seamlessly into the custom app with dark styling.

Dark Mode Scrollbar Hack: Hooked into Windows’ theming engine (uxtheme.dll) to apply the “DarkMode_Explorer” skin, giving scrollbars a modern dark look.

Massive Code Deletion (Cleanup): Removed TypingTranslation.cpp/.h and folded UTF‑8/UTF‑16 converters directly into TypingEngine.cpp, simplifying the codebase.

Direct UTF‑16 Injection: TypingEngine now grabs perfectly formatted UTF‑16 wide characters from the native text box and injects them directly into target OS windows.

Result: A sleek, dark‑themed C++ application with the bulletproof, universal typing power of a native Windows component, rivaling the robustness of C#’s RichTextBox.

Keywords: CE Auto Typer, CE Commander, Windows utility, Portable software, Freeware, Automation tool, Typing simulation, Productivity, Keyboard automation, Text input automation, Unicode typing, UTF‑16 injection, Arabic typing, Persian typing, Chinese typing, Japanese typing, Korean typing, Right‑to‑Left support, Dark mode scrollbar, ImGui integration, Windows EDIT Control, Clipboard sync, Notepad sync, Windows 7 8 10 11
