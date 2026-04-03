#include "imgui.h"
#include "imgui_impl_glfw.h"
#include "imgui_impl_opengl3.h"
#include <GLFW/glfw3.h>
#include <string>
#include <thread>
#include <atomic>
#include <mutex>
#include <random>
#include <chrono>
#include <cstring>
#include <cctype>

#ifdef _WIN32
#include <windows.h>
#endif

// ------------------------------------------------------------------
// Global state & Automation Logic (V1.0 Barebones)
// ------------------------------------------------------------------
static int spdIdx = 2;
static int mstIdx = 0;
static int totalWords = 15;
static char inputBuf[4096] = "This is simple auto writer tool used for good propose.\nFree to use.\nGreat to achieve your goals.";

// Hardcoded Global Hotkeys (Win32 Virtual Keys)
#ifdef _WIN32
static int hkStart = VK_F5;
static int hkPause = VK_F8;
static int hkResume = VK_F9;
static int hkStop = VK_F10;
#else
static int hkStart = 0, hkPause = 0, hkResume = 0, hkStop = 0;
#endif

std::atomic<bool> isTyping(false);
std::atomic<bool> isPaused(false);
std::atomic<bool> stopTyping(false);
std::string engineLog = "[12:00:00] CE Auto Typer V1.0 Online\n> GUI stripped down to core functionality.\n> Sharp edges, standard OS window.\n> Hardcoded Hotkeys: F5 (Launch), F8 (Pause), F9 (Resume), F10 (Stop)\n";
std::mutex logMutex;

// ------------------------------------------------------------------
// Helper Functions (Win32 Globals & Hooks)
// ------------------------------------------------------------------
void AddLog(const std::string& msg) {
    std::lock_guard<std::mutex> lock(logMutex);
    engineLog += msg + "\n";
}

int CountWords(const std::string& str) {
    int count = 0; bool inWord = false;
    for (char c : str) {
        if (std::isspace(c)) inWord = false;
        else if (!inWord) { inWord = true; count++; }
    }
    return count;
}

bool IsKeyJustPressed(int vk) {
#ifdef _WIN32
    static bool keyStates[256] = { false };
    if (vk < 0 || vk > 255) return false;
    bool isDown = (GetAsyncKeyState(vk) & 0x8000) != 0;
    if (isDown && !keyStates[vk]) {
        keyStates[vk] = true;
        return true;
    } else if (!isDown) {
        keyStates[vk] = false;
    }
#endif
    return false;
}

void TypeChar(char c) {
#ifdef _WIN32
    SHORT vk = VkKeyScanA(c);
    BYTE key = vk & 0xFF;
    BYTE shift = (vk >> 8) & 0xFF;

    if (shift & 1) keybd_event(VK_SHIFT, 0, 0, 0);
    keybd_event(key, 0, 0, 0);
    keybd_event(key, 0, KEYEVENTF_KEYUP, 0);
    if (shift & 1) keybd_event(VK_SHIFT, 0, KEYEVENTF_KEYUP, 0);
#endif
}

void TypingWorker(int speedSetting, int mistakeSetting, std::string textToType) {
#ifndef _WIN32
    AddLog("> ERROR: Automation is strictly supported on Windows.");
    isTyping = false;
    return;
#endif

    AddLog("> Ready. Please click your target window. Starting in 5 seconds...");
    for (int i = 5; i > 0; i--) {
        if (stopTyping) { isTyping = false; return; }
        std::this_thread::sleep_for(std::chrono::seconds(1));
    }
    if (stopTyping) { isTyping = false; return; }

    AddLog("> Typing started...");
    
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

    std::random_device rd;
    std::mt19937 gen(rd());
    std::uniform_int_distribution<> speedDist(speedMin, speedMax);
    std::uniform_int_distribution<> mstDist(mstMin, mstMax);
    
    int targetMistakes = mstDist(gen);
    int mistakesMade = 0;

    for (size_t i = 0; i < textToType.length(); i++) {
        if (stopTyping) break;

        while (isPaused) {
            std::this_thread::sleep_for(std::chrono::milliseconds(100));
            if (stopTyping) break;
        }
        if (stopTyping) break;

        char c = textToType[i];
        if (c == '\n' || c == '\r') {
            if (c == '\r' && i + 1 < textToType.length() && textToType[i + 1] == '\n') i++;
#ifdef _WIN32
            keybd_event(VK_RETURN, 0, 0, 0);
            keybd_event(VK_RETURN, 0, KEYEVENTF_KEYUP, 0);
#endif
            std::this_thread::sleep_for(std::chrono::milliseconds(400));
            continue;
        }

        if (mistakesMade < targetMistakes && textToType.length() > 5 && (gen() % 100 < 5)) {
            TypeChar('x');
            std::this_thread::sleep_for(std::chrono::milliseconds(speedDist(gen)));
#ifdef _WIN32
            keybd_event(VK_BACK, 0, 0, 0);
            keybd_event(VK_BACK, 0, KEYEVENTF_KEYUP, 0);
#endif
            std::this_thread::sleep_for(std::chrono::milliseconds(speedDist(gen)));
            mistakesMade++;
        }

        TypeChar(c);
        std::this_thread::sleep_for(std::chrono::milliseconds(speedDist(gen)));
    }

    if (stopTyping) AddLog("> Stopped by user.");
    else AddLog("> Complete! Made " + std::to_string(mistakesMade) + " human-like mistakes.");

    isTyping = false;
    stopTyping = false;
    isPaused = false;
}

// ------------------------------------------------------------------
int main() {
    if (!glfwInit()) return 1;

    // Standard OS window decoration (No custom title bar resizing stuff!)
    glfwWindowHint(GLFW_DECORATED, GLFW_TRUE);
    glfwWindowHint(GLFW_VISIBLE, GLFW_FALSE);

    int winW = 800, winH = 600;
    GLFWwindow* window = glfwCreateWindow(winW, winH, "CE AUTO TYPER v1.0", NULL, NULL);
    if (!window) return 1;

    // Keep it TopMost for functionality
    glfwSetWindowAttrib(window, GLFW_FLOATING, GLFW_TRUE);
    glfwSetWindowSizeLimits(window, 800, 600, GLFW_DONT_CARE, GLFW_DONT_CARE);

    const GLFWvidmode* mode = glfwGetVideoMode(glfwGetPrimaryMonitor());
    glfwSetWindowPos(window, (mode->width - winW) / 2, (mode->height - winH) / 2);
    glfwShowWindow(window);
    glfwMakeContextCurrent(window);
    glfwSwapInterval(1);

    IMGUI_CHECKVERSION();
    ImGui::CreateContext();
    ImGuiIO& io = ImGui::GetIO();

    // Standard ImGui Dark Theme with completely sharp edges (0.0f rounding)
    ImGui::StyleColorsDark();
    ImGuiStyle& style = ImGui::GetStyle();
    style.WindowRounding = 0.0f;
    style.ChildRounding = 0.0f;
    style.FrameRounding = 0.0f;
    style.PopupRounding = 0.0f;
    style.ScrollbarRounding = 0.0f;
    style.GrabRounding = 0.0f;
    style.TabRounding = 0.0f;

    ImGui_ImplGlfw_InitForOpenGL(window, true);
    ImGui_ImplOpenGL3_Init("#version 130");

    static char logBufRender[4096 * 4] = "";

    // ------------------------------------------------------------------
    // Main loop
    // ------------------------------------------------------------------
    while (!glfwWindowShouldClose(window)) {
        glfwPollEvents();

        // --------------------------------------------------------------
        // GLOBAL HOTKEY POLLING
        // --------------------------------------------------------------
        if (IsKeyJustPressed(hkStart) && !isTyping) {
            std::string textToType(inputBuf);
            if (!textToType.empty()) {
                stopTyping = false; isPaused = false; isTyping = true;
                std::thread(TypingWorker, spdIdx, mstIdx, textToType).detach();
                AddLog("> Global Hotkey: START Triggered.");
            }
        }
        if (IsKeyJustPressed(hkPause) && isTyping && !isPaused) {
            isPaused = true; AddLog("> Global Hotkey: PAUSED Triggered.");
        }
        if (IsKeyJustPressed(hkResume) && isTyping && isPaused) {
            isPaused = false; AddLog("> Global Hotkey: RESUMED Triggered.");
        }
        if (IsKeyJustPressed(hkStop) && isTyping) {
            stopTyping = true; AddLog("> Global Hotkey: STOPPED Triggered.");
        }

        ImGui_ImplOpenGL3_NewFrame();
        ImGui_ImplGlfw_NewFrame();
        ImGui::NewFrame();

        ImGui::SetNextWindowPos(ImVec2(0,0));
        ImGui::SetNextWindowSize(io.DisplaySize);
        
        // Single main fullscreen child window mapped to the OS Window
        ImGui::Begin("MainLayout", nullptr, ImGuiWindowFlags_NoTitleBar | ImGuiWindowFlags_NoResize | ImGuiWindowFlags_NoMove | ImGuiWindowFlags_NoCollapse);

        const char* speeds[] = { "Very Slow", "Slow", "Medium", "Fast", "Very Fast" };
        const char* mistRanges[] = { "None", "Few", "Some", "Many", "Lots", "Random" };
        
        // Setup dynamic status string exactly as requested
        const char* currentStatus = "idle";
        if (isTyping) currentStatus = isPaused ? "pause" : "start";

        // Status : (idle,start, pause) : speed (what chousen) : mistake (what chousen)
        ImGui::SetCursorPos(ImVec2(20, 15));
        ImGui::Text("Status : (%s) : speed (%s) : mistake (%s)", currentStatus, speeds[spdIdx], mistRanges[mstIdx]);

        ImGui::SetCursorPos(ImVec2(20, 40));
        if (ImGui::InputTextMultiline("##In", inputBuf, sizeof(inputBuf), ImVec2(760, 120))) {
            totalWords = CountWords(inputBuf);
        }

        // Replaced Notepad location with Default button
        ImGui::SetCursorPos(ImVec2(20, 175));
        if (ImGui::Button("Default String", ImVec2(120, 30))) {
            std::string defStr = "This is simple auto writer tool used for good propose.\nFree to use.\nGreat to achieve your goals.";
            strncpy(inputBuf, defStr.c_str(), sizeof(inputBuf) - 1);
            totalWords = CountWords(defStr);
            AddLog("> Loaded default string.");
        }

        ImGui::SetCursorPos(ImVec2(20, 220));
        ImGui::Text("Speed:");
        ImGui::SetCursorPos(ImVec2(70, 218));
        ImGui::SetNextItemWidth(120);
        ImGui::Combo("##Spd", &spdIdx, "Very Slow\0Slow\0Medium\0Fast\0Very Fast\0\0");

        ImGui::SetCursorPos(ImVec2(210, 220));
        ImGui::Text("Mistakes:");
        ImGui::SetCursorPos(ImVec2(280, 218));
        ImGui::SetNextItemWidth(120);
        ImGui::Combo("##Mst", &mstIdx, "None\0Few\0Some\0Many\0Lots\0Random\0\0");

        if (!isTyping) {
            ImGui::SetCursorPos(ImVec2(20, 270));
            if (ImGui::Button("START", ImVec2(100, 30))) {
                std::string textToType(inputBuf);
                if (textToType.empty()) {
                    AddLog("> ERROR: No text loaded to type!");
                } else {
                    stopTyping = false; isPaused = false; isTyping = true;
                    std::thread(TypingWorker, spdIdx, mstIdx, textToType).detach();
                }
            }
        } else {
            ImGui::SetCursorPos(ImVec2(20, 270));
            if (ImGui::Button("STOP", ImVec2(100, 30))) {
                stopTyping = true; isPaused = false;
                AddLog("> Sent stop signal to engine...");
            }
            ImGui::SetCursorPos(ImVec2(130, 270));
            if (ImGui::Button(isPaused ? "RESUME" : "PAUSE", ImVec2(100, 30))) {
                isPaused = !isPaused;
                AddLog(isPaused ? "> Engine Paused." : "> Engine Resumed.");
            }
        }

        {
            std::lock_guard<std::mutex> lock(logMutex);
            strncpy(logBufRender, engineLog.c_str(), sizeof(logBufRender) - 1);
        }

        ImGui::SetCursorPos(ImVec2(20, 320));
        ImGui::InputTextMultiline("##Log", logBufRender, sizeof(logBufRender), ImVec2(760, ImGui::GetWindowHeight() - 340), ImGuiInputTextFlags_ReadOnly);

        ImGui::End(); // MainLayout

        ImGui::Render();
        int dw, dh;
        glfwGetFramebufferSize(window, &dw, &dh);
        glViewport(0, 0, dw, dh);
        glClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        glClear(GL_COLOR_BUFFER_BIT);
        ImGui_ImplOpenGL3_RenderDrawData(ImGui::GetDrawData());
        glfwSwapBuffers(window);
    }

    ImGui_ImplOpenGL3_Shutdown();
    ImGui_ImplGlfw_Shutdown();
    ImGui::DestroyContext();
    glfwDestroyWindow(window);
    glfwTerminate();
    return 0;
}
