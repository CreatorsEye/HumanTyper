#pragma once
#include <string>
#include <atomic>

extern std::atomic<bool> isTyping;
extern std::atomic<bool> isPaused;
extern std::atomic<bool> stopTyping;

namespace TypingEngine {
    // Moved from the deleted Translation file
    std::wstring Utf8ToUtf16(const std::string& utf8);
    std::string Utf16ToUtf8(const std::wstring& utf16);

    void Worker(int speedSetting, int mistakeSetting, std::wstring textToType);
}