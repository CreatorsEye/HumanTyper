#include "TypingEngine.h"
#include <thread>
#include <chrono>
#include <random>

#ifdef _WIN32
#include <windows.h>
#endif

// Link back to the UI log
extern void AddLog(const std::string& msg);

namespace TypingEngine {

#ifdef _WIN32
    // Converts ImGui's UTF-8 text into UTF-16 Wide Characters (Required for Arabic/Chinese)
    std::wstring Utf8ToUtf16(const std::string& utf8) {
        if (utf8.empty()) return std::wstring();
        int size_needed = MultiByteToWideChar(CP_UTF8, 0, &utf8[0], (int)utf8.size(), NULL, 0);
        std::wstring utf16(size_needed, 0);
        MultiByteToWideChar(CP_UTF8, 0, &utf8[0], (int)utf8.size(), &utf16[0], size_needed);
        return utf16;
    }

    // Identical to your C# NativeMethods KEYEVENTF_UNICODE logic
    void TypeUnicodeChar(wchar_t c) {
        INPUT input[2] = {0};
        input[0].type = INPUT_KEYBOARD;
        input[0].ki.wVk = 0;
        input[0].ki.wScan = c;
        input[0].ki.dwFlags = KEYEVENTF_UNICODE;
        
        input[1].type = INPUT_KEYBOARD;
        input[1].ki.wVk = 0;
        input[1].ki.wScan = c;
        input[1].ki.dwFlags = KEYEVENTF_UNICODE | KEYEVENTF_KEYUP;
        
        SendInput(2, input, sizeof(INPUT));
    }
#endif

    void Worker(int speedSetting, int mistakeSetting, std::string textToTypeUtf8) {
#ifdef _WIN32
        AddLog("> Engine: Ready. Please click your target window. Starting in 5 seconds...");
        for (int i = 5; i > 0; i--) {
            if (stopTyping) { isTyping = false; return; }
            std::this_thread::sleep_for(std::chrono::seconds(1));
        }
        if (stopTyping) { isTyping = false; return; }
        AddLog("> Engine: Multi-Language Unicode Typing started...");
        
        int speedMin = 60, speedMax = 120;
        if (speedSetting == 0) { speedMin = 200; speedMax = 300; }
        else if (speedSetting == 1) { speedMin = 120; speedMax = 200; }
        else if (speedSetting == 3) { speedMin = 30; speedMax = 70; }
        else if (speedSetting == 4) { speedMin = 15; speedMax = 40; }

        int mstMin = 0, mstMax = 0;
        if (mistakeSetting == 1) { mstMin = 1; mstMax = 2; }
        else if (mistakeSetting == 2) { mstMin = 3; mstMax = 4; }
        else if (mistakeSetting == 3) { mstMin = 5; mstMax = 7; }
        else if (mistakeSetting == 4) { mstMin = 8; mstMax = 10; }
        else if (mistakeSetting == 5) { mstMin = 1; mstMax = 5; } 

        std::random_device rd; std::mt19937 gen(rd());
        std::uniform_int_distribution<> speedDist(speedMin, speedMax);
        std::uniform_int_distribution<> mstDist(mstMin, mstMax);
        
        std::wstring textToType = Utf8ToUtf16(textToTypeUtf8);
        int targetMistakes = mstDist(gen), mistakesMade = 0;

        for (size_t i = 0; i < textToType.length(); i++) {
            if (stopTyping) break;
            while (isPaused) { std::this_thread::sleep_for(std::chrono::milliseconds(100)); if (stopTyping) break; }
            if (stopTyping) break;
            
            wchar_t c = textToType[i];
            
            if (c == L'\n' || c == L'\r') {
                if (c == L'\r' && i + 1 < textToType.length() && textToType[i + 1] == L'\n') i++;
                keybd_event(VK_RETURN, 0, 0, 0); keybd_event(VK_RETURN, 0, KEYEVENTF_KEYUP, 0);
                std::this_thread::sleep_for(std::chrono::milliseconds(400));
                continue;
            }
            
            // Prevent attempting to inject a mistake inside a multi-byte surrogate pair (Emojis/Rare CJK)
            bool isSurrogate = (c >= 0xD800 && c <= 0xDFFF);

            if (!isSurrogate && mistakesMade < targetMistakes && textToType.length() > 5 && (gen() % 100 < 5)) {
                TypeUnicodeChar(L'x'); 
                std::this_thread::sleep_for(std::chrono::milliseconds(speedDist(gen)));
                keybd_event(VK_BACK, 0, 0, 0); keybd_event(VK_BACK, 0, KEYEVENTF_KEYUP, 0);
                std::this_thread::sleep_for(std::chrono::milliseconds(speedDist(gen)));
                mistakesMade++;
            }
            
            TypeUnicodeChar(c);
            std::this_thread::sleep_for(std::chrono::milliseconds(speedDist(gen)));
        }
        
        if (stopTyping) AddLog("> Engine: Stopped by user.");
        else AddLog("> Engine: Complete! Made " + std::to_string(mistakesMade) + " human-like mistakes.");
        
        isTyping = false; stopTyping = false; isPaused = false;
#endif
    }
}