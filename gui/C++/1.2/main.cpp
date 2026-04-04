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
#include <fstream>
#include <vector>

#ifdef _WIN32
#define GLFW_EXPOSE_NATIVE_WIN32
#include <GLFW/glfw3native.h> 
#include <windows.h>
#endif

// ------------------------------------------------------------------
// Global State & Settings (v1.2)
// ------------------------------------------------------------------
static int spdIdx = 2;
static int mstIdx = 0;
static int totalWords = 15;
static char inputBuf[4096] = "This is simple auto writer tool used for good propose.\nFree to use.\nGreat to achieve your goals.";

std::atomic<bool> isTyping(false);
std::atomic<bool> isPaused(false);
std::atomic<bool> stopTyping(false);
std::string engineLog = "[12:00:00] CE Commander Engine V1.2 \n> Single Instance Lock Active.\n> GUI upgraded to Modern Dark.\n> Notepad Sync & Hotkey Mapping Restored.\n";
std::mutex logMutex;

// Hotkey Variables
#ifdef _WIN32
static int hkStart = VK_F7;
static int hkPause = VK_F8;
static int hkResume = VK_F9;
static int hkStop = VK_F10;
static int hkStatus = VK_F11;
static int tempStart = VK_F7, tempPause = VK_F8, tempResume = VK_F9, tempStop = VK_F10, tempStatus = VK_F11;
#else
static int hkStart = 0, hkPause = 0, hkResume = 0, hkStop = 0, hkStatus = 0;
static int tempStart = 0, tempPause = 0, tempResume = 0, tempStop = 0, tempStatus = 0;
#endif

static bool openHotkeyModal = false;
static bool openCaptureModal = false;
static int activeCaptureId = -1;

// Drag & Drop State
static bool isDragging = false;
static double dragOffsetX, dragOffsetY;

// Notepad Sync State
std::atomic<bool> notepadTextReady(false);
std::string notepadLoadedText = "";
std::mutex notepadMutex;

// ------------------------------------------------------------------
// Helper Functions
// ------------------------------------------------------------------
void AddLog(const std::string& msg) {
    std::lock_guard<std::mutex> lock(logMutex);
    engineLog += msg + "\n";
}

int CountWords(const std::string& str) {
    int count = 0; bool inWord = false;
    for (char c : str) {
        if (c == ' ' || c == '\n' || c == '\r' || c == '\t') inWord = false;
        else if (!inWord) { inWord = true; count++; }
    }
    return count;
}

std::string GetKeyName(int vk) {
#ifdef _WIN32
    char name[128];
    UINT scanCode = MapVirtualKeyA(vk, MAPVK_VK_TO_VSC);
    switch (vk) {
        case VK_LEFT: case VK_UP: case VK_RIGHT: case VK_DOWN:
        case VK_PRIOR: case VK_NEXT: case VK_END: case VK_HOME:
        case VK_INSERT: case VK_DELETE: case VK_DIVIDE: case VK_NUMLOCK:
            scanCode |= KF_EXTENDED; break;
    }
    if (GetKeyNameTextA(scanCode << 16, name, sizeof(name)) != 0) return std::string(name);
    return "Key " + std::to_string(vk);
#else
    return "Key";
#endif
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
    BYTE key = vk & 0xFF, shift = (vk >> 8) & 0xFF;
    if (shift & 1) keybd_event(VK_SHIFT, 0, 0, 0);
    keybd_event(key, 0, 0, 0);
    keybd_event(key, 0, KEYEVENTF_KEYUP, 0);
    if (shift & 1) keybd_event(VK_SHIFT, 0, KEYEVENTF_KEYUP, 0);
#endif
}

void TypingWorker(int speedSetting, int mistakeSetting, std::string textToType) {
#ifdef _WIN32
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

    std::random_device rd; std::mt19937 gen(rd());
    std::uniform_int_distribution<> speedDist(speedMin, speedMax);
    std::uniform_int_distribution<> mstDist(mstMin, mstMax);
    
    int targetMistakes = mstDist(gen), mistakesMade = 0;
    for (size_t i = 0; i < textToType.length(); i++) {
        if (stopTyping) break;
        while (isPaused) { std::this_thread::sleep_for(std::chrono::milliseconds(100)); if (stopTyping) break; }
        if (stopTyping) break;
        char c = textToType[i];
        if (c == '\n' || c == '\r') {
            if (c == '\r' && i + 1 < textToType.length() && textToType[i + 1] == '\n') i++;
            keybd_event(VK_RETURN, 0, 0, 0); keybd_event(VK_RETURN, 0, KEYEVENTF_KEYUP, 0);
            std::this_thread::sleep_for(std::chrono::milliseconds(400));
            continue;
        }
        if (mistakesMade < targetMistakes && textToType.length() > 5 && (gen() % 100 < 5)) {
            TypeChar('x'); std::this_thread::sleep_for(std::chrono::milliseconds(speedDist(gen)));
            keybd_event(VK_BACK, 0, 0, 0); keybd_event(VK_BACK, 0, KEYEVENTF_KEYUP, 0);
            std::this_thread::sleep_for(std::chrono::milliseconds(speedDist(gen)));
            mistakesMade++;
        }
        TypeChar(c); std::this_thread::sleep_for(std::chrono::milliseconds(speedDist(gen)));
    }
    if (stopTyping) AddLog("> Stopped by user.");
    else AddLog("> Complete! Made " + std::to_string(mistakesMade) + " human-like mistakes.");
    isTyping = false; stopTyping = false; isPaused = false;
#endif
}

// ------------------------------------------------------------------
// Main Entry Point
// ------------------------------------------------------------------
int main() {
#ifdef _WIN32
    // SINGLE INSTANCE LOCK (v1.2)
    HANDLE hMutex = CreateMutexA(NULL, FALSE, "CE_AutoTyper_SingleInstance_Mutex_v1_2");
    if (GetLastError() == ERROR_ALREADY_EXISTS) {
        if (hMutex) { CloseHandle(hMutex); }
        return 0; // Exit silently if already running
    }
#endif

    if (!glfwInit()) return 1;
    
    glfwWindowHint(GLFW_DECORATED, GLFW_FALSE);
    glfwWindowHint(GLFW_VISIBLE, GLFW_FALSE);
    
    GLFWwindow* window = glfwCreateWindow(800, 640, "CE Commander Engine v1.2", NULL, NULL);
    if (!window) return 1;

#ifdef _WIN32
    HWND hwnd = glfwGetWin32Window(window);
    HICON hIcon = LoadIconA(GetModuleHandleA(NULL), "IDI_ICON1");
    if (hIcon) {
        SendMessageA(hwnd, WM_SETICON, ICON_SMALL, (LPARAM)hIcon);
        SendMessageA(hwnd, WM_SETICON, ICON_BIG,   (LPARAM)hIcon);
    }
#endif

    glfwSetWindowAttrib(window, GLFW_FLOATING, GLFW_TRUE);
    glfwSetWindowSizeLimits(window, 800, 640, GLFW_DONT_CARE, GLFW_DONT_CARE);
    const GLFWvidmode* mode = glfwGetVideoMode(glfwGetPrimaryMonitor());
    glfwSetWindowPos(window, (mode->width - 800) / 2, (mode->height - 640) / 2);
    glfwShowWindow(window);
    glfwMakeContextCurrent(window);
    glfwSwapInterval(1);
    
    IMGUI_CHECKVERSION();
    ImGui::CreateContext();
    ImGuiIO& io = ImGui::GetIO();
    
    // Modern Dark Theme Setup (V1.2)
    ImGuiStyle& style = ImGui::GetStyle();
    ImVec4* colors = style.Colors;
    colors[ImGuiCol_Text]                   = ImVec4(0.90f, 0.90f, 0.93f, 1.00f);
    colors[ImGuiCol_WindowBg]               = ImVec4(0.08f, 0.08f, 0.09f, 1.00f);
    colors[ImGuiCol_ChildBg]                = ImVec4(0.12f, 0.12f, 0.13f, 1.00f);
    colors[ImGuiCol_PopupBg]                = ImVec4(0.10f, 0.10f, 0.11f, 1.00f);
    colors[ImGuiCol_Border]                 = ImVec4(0.20f, 0.20f, 0.23f, 0.50f);
    colors[ImGuiCol_FrameBg]                = ImVec4(0.15f, 0.15f, 0.17f, 1.00f);
    colors[ImGuiCol_FrameBgHovered]         = ImVec4(0.19f, 0.19f, 0.21f, 1.00f);
    colors[ImGuiCol_FrameBgActive]          = ImVec4(0.23f, 0.23f, 0.26f, 1.00f);
    colors[ImGuiCol_Button]                 = ImVec4(0.16f, 0.40f, 0.75f, 1.00f); // Modern Blue Accent
    colors[ImGuiCol_ButtonHovered]          = ImVec4(0.20f, 0.48f, 0.88f, 1.00f);
    colors[ImGuiCol_ButtonActive]           = ImVec4(0.12f, 0.32f, 0.62f, 1.00f);
    colors[ImGuiCol_Header]                 = ImVec4(0.16f, 0.40f, 0.75f, 0.80f);
    colors[ImGuiCol_HeaderHovered]          = ImVec4(0.20f, 0.48f, 0.88f, 0.80f);
    colors[ImGuiCol_HeaderActive]           = ImVec4(0.12f, 0.32f, 0.62f, 1.00f);
    
    // v1.x Sharp Edges
    style.WindowRounding = style.ChildRounding = style.FrameRounding = style.PopupRounding = style.ScrollbarRounding = style.GrabRounding = style.TabRounding = 0.0f;
    style.FrameBorderSize = 1.0f;

    ImGui_ImplGlfw_InitForOpenGL(window, true);
    ImGui_ImplOpenGL3_Init("#version 130");
    static char logBufRender[4096 * 4] = "";
    
    while (!glfwWindowShouldClose(window)) {
        glfwPollEvents();

        // --------------------------------------------------------------
        // GLOBAL HOTKEY LISTENER
        // --------------------------------------------------------------
        if (IsKeyJustPressed(hkStart) && !isTyping && !openCaptureModal && !openHotkeyModal) {
            std::string textToType(inputBuf);
            if (!textToType.empty()) {
                stopTyping = false; isPaused = false; isTyping = true;
                std::thread(TypingWorker, spdIdx, mstIdx, textToType).detach();
                AddLog("> Global Hotkey: START Triggered.");
            }
        }
        if (IsKeyJustPressed(hkPause) && isTyping && !isPaused && !openCaptureModal && !openHotkeyModal) {
            isPaused = true; AddLog("> Global Hotkey: PAUSED Triggered.");
        }
        if (IsKeyJustPressed(hkResume) && isTyping && isPaused && !openCaptureModal && !openHotkeyModal) {
            isPaused = false; AddLog("> Global Hotkey: RESUMED Triggered.");
        }
        if (IsKeyJustPressed(hkStop) && isTyping && !openCaptureModal && !openHotkeyModal) {
            stopTyping = true; AddLog("> Global Hotkey: STOPPED Triggered.");
        }
        if (IsKeyJustPressed(hkStatus) && !openCaptureModal && !openHotkeyModal) {
            AddLog(std::string("> Global Hotkey: Status -> ") + (isTyping ? (isPaused ? "PAUSED" : "TYPING") : "READY"));
        }

        // --------------------------------------------------------------
        // ASYNC NOTEPAD SYNC CHECK
        // --------------------------------------------------------------
        if (notepadTextReady) {
            std::lock_guard<std::mutex> lock(notepadMutex);
            snprintf(inputBuf, sizeof(inputBuf), "%s", notepadLoadedText.c_str());
            totalWords = CountWords(notepadLoadedText);
            AddLog("> Loaded " + std::to_string(totalWords) + " words from Notepad Editor.");
            notepadTextReady = false;
        }

        ImGui_ImplOpenGL3_NewFrame();
        ImGui_ImplGlfw_NewFrame();
        ImGui::NewFrame();
        ImGui::SetNextWindowPos(ImVec2(0,0));
        ImGui::SetNextWindowSize(io.DisplaySize);
        ImGui::Begin("MainLayout", nullptr, ImGuiWindowFlags_NoTitleBar | ImGuiWindowFlags_NoResize | ImGuiWindowFlags_NoMove | ImGuiWindowFlags_NoCollapse);

        // --------------------------------------------------------------
        // CUSTOM DRAG-AND-DROP TOP BAR
        // --------------------------------------------------------------
        ImGui::PushStyleColor(ImGuiCol_ChildBg, ImVec4(0.10f, 0.10f, 0.11f, 1.0f));
        ImGui::BeginChild("TitleBar", ImVec2(ImGui::GetContentRegionAvail().x, 30), false, ImGuiWindowFlags_NoScrollbar);
        
        if (ImGui::IsWindowHovered() && ImGui::IsMouseClicked(0)) {
            isDragging = true;
            double cx, cy;
            glfwGetCursorPos(window, &cx, &cy);
            dragOffsetX = cx;
            dragOffsetY = cy;
        }
        if (ImGui::IsMouseReleased(0)) isDragging = false;
        if (isDragging) {
            double cx, cy;
            glfwGetCursorPos(window, &cx, &cy);
            int wx, wy;
            glfwGetWindowPos(window, &wx, &wy);
            glfwSetWindowPos(window, wx + (int)(cx - dragOffsetX), wy + (int)(cy - dragOffsetY));
        }

        ImGui::SetCursorPos(ImVec2(10, 7));
        ImGui::TextColored(ImVec4(0.6f, 0.7f, 0.9f, 1.0f), "CE Commander Engine v1.2");

        ImGui::SameLine(ImGui::GetWindowWidth() - 40);
        ImGui::SetCursorPosY(0);
        ImGui::PushStyleColor(ImGuiCol_Button, ImVec4(0.8f, 0.2f, 0.2f, 1.0f));
        ImGui::PushStyleColor(ImGuiCol_ButtonHovered, ImVec4(1.0f, 0.3f, 0.3f, 1.0f));
        if (ImGui::Button("X", ImVec2(40, 30))) {
            glfwSetWindowShouldClose(window, true);
        }
        ImGui::PopStyleColor(2);

        ImGui::EndChild();
        ImGui::PopStyleColor(); // End TitleBar

        // --------------------------------------------------------------
        // MAIN UI BODY
        // --------------------------------------------------------------
        const char* speeds[] = { "Very Slow", "Slow", "Medium", "Fast", "Very Fast" };
        const char* mistRanges[] = { "None", "Few", "Some", "Many", "Lots", "Random" };
        const char* currentStatus = isTyping ? (isPaused ? "pause" : "start") : "idle";
        
        ImGui::SetCursorPos(ImVec2(20, 45));
        ImGui::Text("Status : (%s) : speed (%s) : mistake (%s)", currentStatus, speeds[spdIdx], mistRanges[mstIdx]);
        
        ImGui::SetCursorPos(ImVec2(20, 70));
        if (ImGui::InputTextMultiline("##In", inputBuf, sizeof(inputBuf), ImVec2(760, 120))) totalWords = CountWords(inputBuf);
        
        // Dynamic Hotkey Label
        ImGui::SetCursorPos(ImVec2(20, 195));
        ImGui::TextColored(ImVec4(0.4f, 0.8f, 0.4f, 1.0f), "Hotkeys -> Start: %s | Pause: %s | Resume: %s | Stop: %s | Status: %s", 
            GetKeyName(hkStart).c_str(), GetKeyName(hkPause).c_str(), GetKeyName(hkResume).c_str(), GetKeyName(hkStop).c_str(), GetKeyName(hkStatus).c_str());

        // Buttons Row
        ImGui::SetCursorPos(ImVec2(20, 225));
        if (ImGui::Button("Set Hotkeys", ImVec2(100, 30))) {
            tempStart = hkStart; tempPause = hkPause; tempResume = hkResume; tempStop = hkStop; tempStatus = hkStatus;
            openHotkeyModal = true;
        }

        ImGui::SetCursorPos(ImVec2(130, 225));
        if (ImGui::Button("Notepad", ImVec2(100, 30))) {
            std::thread([]() {
#ifdef _WIN32
                char tempPath[MAX_PATH];
                GetTempPathA(MAX_PATH, tempPath);
                std::string tempFile = std::string(tempPath) + "CE_temp.txt";
                std::ofstream ofs(tempFile);
                ofs << inputBuf; 
                ofs.close();

                std::string cmd = "notepad.exe \"" + tempFile + "\"";
                system(cmd.c_str()); 

                std::ifstream ifs(tempFile);
                if (ifs.is_open()) {
                    std::string content((std::istreambuf_iterator<char>(ifs)), std::istreambuf_iterator<char>());
                    ifs.close();
                    remove(tempFile.c_str());
                    if (!content.empty()) {
                        std::lock_guard<std::mutex> lock(notepadMutex);
                        notepadLoadedText = content;
                        notepadTextReady = true;
                    }
                }
#endif
            }).detach();
            AddLog("> Opened Notepad. Save & Close Notepad to automatically load text back here.");
        }

        ImGui::SetCursorPos(ImVec2(240, 225));
        if (ImGui::Button("Default String", ImVec2(120, 30))) {
            std::string defStr = "This is simple auto writer tool used for good propose.\nFree to use.\nGreat to achieve your goals.";
            snprintf(inputBuf, sizeof(inputBuf), "%s", defStr.c_str());
            totalWords = CountWords(defStr);
            AddLog("> Loaded default string.");
        }
        
        // Combos
        ImGui::SetCursorPos(ImVec2(20, 270)); ImGui::Text("Speed:");
        ImGui::SetCursorPos(ImVec2(70, 268)); ImGui::SetNextItemWidth(120);
        ImGui::Combo("##Spd", &spdIdx, "Very Slow\0Slow\0Medium\0Fast\0Very Fast\0\0");
        
        ImGui::SetCursorPos(ImVec2(210, 270)); ImGui::Text("Mistakes:");
        ImGui::SetCursorPos(ImVec2(280, 268)); ImGui::SetNextItemWidth(120);
        ImGui::Combo("##Mst", &mstIdx, "None\0Few\0Some\0Many\0Lots\0Random\0\0");
        
        // Engine Control Buttons
        if (!isTyping) {
            ImGui::SetCursorPos(ImVec2(20, 315));
            ImGui::PushStyleColor(ImGuiCol_Button, ImVec4(0.2f, 0.6f, 0.3f, 1.0f));
            ImGui::PushStyleColor(ImGuiCol_ButtonHovered, ImVec4(0.3f, 0.7f, 0.4f, 1.0f));
            if (ImGui::Button("START", ImVec2(100, 35))) {
                std::string textToType(inputBuf);
                if (textToType.empty()) AddLog("> ERROR: No text loaded to type!");
                else { stopTyping = false; isPaused = false; isTyping = true; std::thread(TypingWorker, spdIdx, mstIdx, textToType).detach(); }
            }
            ImGui::PopStyleColor(2);
        } else {
            ImGui::SetCursorPos(ImVec2(20, 315));
            ImGui::PushStyleColor(ImGuiCol_Button, ImVec4(0.8f, 0.2f, 0.2f, 1.0f));
            ImGui::PushStyleColor(ImGuiCol_ButtonHovered, ImVec4(0.9f, 0.3f, 0.3f, 1.0f));
            if (ImGui::Button("STOP", ImVec2(100, 35))) { stopTyping = true; isPaused = false; AddLog("> Sent stop signal to engine..."); }
            ImGui::PopStyleColor(2);

            ImGui::SetCursorPos(ImVec2(130, 315));
            ImGui::PushStyleColor(ImGuiCol_Button, ImVec4(0.8f, 0.6f, 0.1f, 1.0f));
            ImGui::PushStyleColor(ImGuiCol_ButtonHovered, ImVec4(0.9f, 0.7f, 0.2f, 1.0f));
            if (ImGui::Button(isPaused ? "RESUME" : "PAUSE", ImVec2(100, 35))) { isPaused = !isPaused; AddLog(isPaused ? "> Engine Paused." : "> Engine Resumed."); }
            ImGui::PopStyleColor(2);
        }
        
        { std::lock_guard<std::mutex> lock(logMutex); snprintf(logBufRender, sizeof(logBufRender), "%s", engineLog.c_str()); }
        ImGui::SetCursorPos(ImVec2(20, 365));
        ImGui::InputTextMultiline("##Log", logBufRender, sizeof(logBufRender), ImVec2(760, ImGui::GetWindowHeight() - 385), ImGuiInputTextFlags_ReadOnly);
        
        // --------------------------------------------------------------
        // HOTKEY MODALS
        // --------------------------------------------------------------
        if (openHotkeyModal) { ImGui::OpenPopup("Hotkey Settings"); openHotkeyModal = false; }
        if (ImGui::BeginPopupModal("Hotkey Settings", NULL, ImGuiWindowFlags_AlwaysAutoResize | ImGuiWindowFlags_NoMove | ImGuiWindowFlags_NoTitleBar)) {
            ImGui::TextColored(ImVec4(0.4f, 0.8f, 0.4f, 1.0f), "Hotkey Settings");
            ImGui::Dummy(ImVec2(0, 10));

            auto DrawHkRow = [&](const char* name, int& keyVar, int id) {
                ImGui::AlignTextToFramePadding();
                ImGui::Text("%s", name);
                ImGui::SameLine(80);
                if (ImGui::Button(GetKeyName(keyVar).c_str(), ImVec2(100, 25))) {
                    activeCaptureId = id;
                    openCaptureModal = true;
                }
            };

            DrawHkRow("Start:", tempStart, 1);
            DrawHkRow("Pause:", tempPause, 2);
            DrawHkRow("Resume:", tempResume, 3);
            DrawHkRow("Stop:", tempStop, 4);
            DrawHkRow("Status:", tempStatus, 5);
            ImGui::Dummy(ImVec2(0, 10));

            if (openCaptureModal) { ImGui::OpenPopup("Press any key"); openCaptureModal = false; }
            if (ImGui::BeginPopupModal("Press any key", NULL, ImGuiWindowFlags_AlwaysAutoResize | ImGuiWindowFlags_NoMove | ImGuiWindowFlags_NoTitleBar)) {
                ImGui::Text("Press new key...");
                ImGui::TextColored(ImVec4(0.5f, 0.5f, 0.5f, 1.0f), "Press ESC to cancel");
#ifdef _WIN32
                for (int k = 8; k <= 255; k++) {
                    if (k == VK_LBUTTON || k == VK_RBUTTON || k == VK_MBUTTON) continue;
                    if (GetAsyncKeyState(k) & 0x8000) {
                        if (k != VK_ESCAPE) {
                            if (activeCaptureId == 1) tempStart = k;
                            if (activeCaptureId == 2) tempPause = k;
                            if (activeCaptureId == 3) tempResume = k;
                            if (activeCaptureId == 4) tempStop = k;
                            if (activeCaptureId == 5) tempStatus = k;
                        }
                        activeCaptureId = -1;
                        IsKeyJustPressed(k); // Flush debounce
                        ImGui::CloseCurrentPopup();
                        break;
                    }
                }
#endif
                ImGui::EndPopup();
            }

            ImGui::PushStyleColor(ImGuiCol_Button, ImVec4(0.2f, 0.6f, 0.3f, 1.0f));
            if (ImGui::Button("Save", ImVec2(80, 30))) {
                hkStart = tempStart; hkPause = tempPause; hkResume = tempResume; hkStop = tempStop; hkStatus = tempStatus;
                AddLog("> Hotkeys updated successfully.");
                ImGui::CloseCurrentPopup();
            }
            ImGui::PopStyleColor();
            ImGui::SameLine();
            ImGui::PushStyleColor(ImGuiCol_Button, ImVec4(0.8f, 0.2f, 0.2f, 1.0f));
            if (ImGui::Button("Cancel", ImVec2(80, 30))) {
                activeCaptureId = -1;
                ImGui::CloseCurrentPopup();
            }
            ImGui::PopStyleColor();

            ImGui::EndPopup();
        }

        ImGui::End(); // MainLayout
        ImGui::Render();
        
        int dw, dh; glfwGetFramebufferSize(window, &dw, &dh); glViewport(0, 0, dw, dh);
        glClearColor(0.08f, 0.08f, 0.09f, 1.0f); glClear(GL_COLOR_BUFFER_BIT);
        ImGui_ImplOpenGL3_RenderDrawData(ImGui::GetDrawData());
        glfwSwapBuffers(window);
    }
    
    ImGui_ImplOpenGL3_Shutdown(); ImGui_ImplGlfw_Shutdown(); ImGui::DestroyContext();
    glfwDestroyWindow(window); glfwTerminate();
    
#ifdef _WIN32
    if (hMutex) { CloseHandle(hMutex); } // Clean up the lock
#endif
    return 0;
}

#ifdef _WIN32
int APIENTRY WinMain(HINSTANCE hInst, HINSTANCE hInstPrev, PSTR cmdline, int cmdshow) { return main(); }
#endif
