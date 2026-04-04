#pragma once
#include <string>
#include <atomic>

// Shared states
extern std::atomic<bool> isTyping;
extern std::atomic<bool> isPaused;
extern std::atomic<bool> stopTyping;

namespace TypingEngine {
    void Worker(int speedSetting, int mistakeSetting, std::string textToTypeUtf8);
}