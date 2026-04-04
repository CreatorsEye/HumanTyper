// ============================================================================
// CE COMMANDER ENGINE v1.0 - COMPLETE SOURCE CODE WITH DEVELOPER NOTES
// Project: Auto Typing Tool with Human Simulation
// Team: Creators Eye Team
// Purpose: Simulate human-like typing with adjustable speed, mistakes, and pauses
// ============================================================================

// ----------------------------------------------------------------------------
// SECTION 1: HEADER INCLUDES
// Each include serves a specific purpose for the application
// ----------------------------------------------------------------------------

#include "imgui.h"              // Core Dear ImGui library for UI rendering
#include "imgui_impl_glfw.h"    // GLFW backend for ImGui (window/input handling)
#include "imgui_impl_opengl3.h" // OpenGL3 backend for ImGui (graphics rendering)
#include <GLFW/glfw3.h>         // Window creation, input, OpenGL context
#include <string>               // std::string for text manipulation
#include <thread>               // std::thread for background typing without freezing UI
#include <atomic>               // Thread-safe boolean flags (isTyping, isPaused, stopTyping)
#include <mutex>                // std::mutex to protect shared resources (engineLog)
#include <random>               // Random number generation for human-like delays/mistakes
#include <chrono>               // Time utilities for sleep durations

// ----------------------------------------------------------------------------
// SECTION 2: WINDOWS-SPECIFIC ICON FIX
// This ensures the application has a proper icon in taskbar and window title
// ----------------------------------------------------------------------------

#ifdef _WIN32
#define GLFW_EXPOSE_NATIVE_WIN32  // Allow GLFW to expose Win32 window handle
#include <GLFW/glfw3native.h>      // Native window handle functions
#include <windows.h>               // Win32 API for setting icons (LoadIcon, SendMessage)
#endif

// ----------------------------------------------------------------------------
// SECTION 3: GLOBAL APPLICATION STATE
// These variables control the entire typing engine behavior
// All atomic variables are thread-safe for background worker thread
// ----------------------------------------------------------------------------

static int spdIdx = 2;           // Speed preset index: 0=VerySlow,1=Slow,2=Medium,3=Fast,4=VeryFast
static int mstIdx = 0;           // Mistake preset index: 0=None,1=Few,2=Some,3=Many,4=Lots,5=Random
static int totalWords = 15;      // Live word count of input buffer (updates on every keystroke)
static char inputBuf[4096] = "This is simple auto writer tool used for good propose.\nFree to use.\nGreat to achieve your goals."; // Main text to type

std::atomic<bool> isTyping(false);   // TRUE = engine is actively typing (prevents multiple starts)
std::atomic<bool> isPaused(false);   // TRUE = temporarily halt typing (resume later)
std::atomic<bool> stopTyping(false); // TRUE = permanent stop (user clicked STOP button)

std::string engineLog = "[12:00:00] CE Commander Engine V1.0 \n> GUI stripped to core.\n> human typer.\n"; // Console log display buffer
std::mutex logMutex;                 // Prevents UI and worker thread from corrupting engineLog simultaneously

// ----------------------------------------------------------------------------
// FUNCTION: AddLog
// PURPOSE: Thread-safe logging to the console display
// WHY: Worker thread and main UI thread both write to engineLog
// ----------------------------------------------------------------------------

void AddLog(const std::string& msg) {
    std::lock_guard<std::mutex> lock(logMutex);  // Auto-lock, auto-release when out of scope
    engineLog += msg + "\n";                     // Append message with newline
}

// ----------------------------------------------------------------------------
// FUNCTION: CountWords
// PURPOSE: Count number of words in a string
// ALGORITHM: Tracks transitions from whitespace to non-whitespace
// ----------------------------------------------------------------------------

int CountWords(const std::string& str) {
    int count = 0; 
    bool inWord = false;
    for (char c : str) {
        if (c == ' ' || c == '\n' || c == '\r' || c == '\t') {
            inWord = false;  // Whitespace = end of current word
        }
        else if (!inWord) {
            inWord = true;   // First character of a new word
            count++;
        }
    }
    return count;
}

// ----------------------------------------------------------------------------
// FUNCTION: TypeChar
// PURPOSE: Simulate a single keyboard key press/release on Windows
// DETAILS: Handles uppercase letters by automatically pressing SHIFT
// ----------------------------------------------------------------------------

void TypeChar(char c) {
#ifdef _WIN32
    SHORT vk = VkKeyScanA(c);              // Convert character to virtual-key code
    BYTE key = vk & 0xFF;                  // Lower byte = key code (e.g., 'A' = 0x41)
    BYTE shift = (vk >> 8) & 0xFF;         // Upper byte = shift state (1 = shift required)
    
    if (shift & 1) keybd_event(VK_SHIFT, 0, 0, 0);  // Press SHIFT if needed
    keybd_event(key, 0, 0, 0);                      // Press the actual key
    keybd_event(key, 0, KEYEVENTF_KEYUP, 0);        // Release the actual key
    if (shift & 1) keybd_event(VK_SHIFT, 0, KEYEVENTF_KEYUP, 0); // Release SHIFT
#endif
}

// ----------------------------------------------------------------------------
// FUNCTION: TypingWorker
// PURPOSE: Background thread that performs the actual typing simulation
// RUNS ON: Separate thread (detached from main UI)
// ----------------------------------------------------------------------------

void TypingWorker(int speedSetting, int mistakeSetting, std::string textToType) {
#ifdef _WIN32
    // Step 1: Give user time to focus target window
    AddLog("> Ready. Please click your target window. Starting in 5 seconds...");
    for (int i = 5; i > 0; i--) {
        if (stopTyping) { isTyping = false; return; }  // User cancelled during countdown
        std::this_thread::sleep_for(std::chrono::seconds(1));
    }
    if (stopTyping) { isTyping = false; return; }
    AddLog("> Typing started...");
    
    // Step 2: Convert UI settings to actual milliseconds per keystroke
    int speedMin = 60, speedMax = 120;   // Default "Medium": 60-120ms between keys
    if (speedSetting == 0) { speedMin = 200; speedMax = 300; }  // Very Slow
    else if (speedSetting == 1) { speedMin = 120; speedMax = 200; } // Slow
    else if (speedSetting == 3) { speedMin = 30; speedMax = 70; }    // Fast
    else if (speedSetting == 4) { speedMin = 15; speedMax = 40; }    // Very Fast
    
    // Step 3: Configure mistake simulation ranges
    int mstMin = 0, mstMax = 0;
    if (mistakeSetting == 1) { mstMin = 1; mstMax = 2; }      // Few: 1-2 mistakes total
    else if (mistakeSetting == 2) { mstMin = 3; mstMax = 4; } // Some: 3-4 mistakes
    else if (mistakeSetting == 3) { mstMin = 5; mstMax = 7; } // Many: 5-7 mistakes
    else if (mistakeSetting == 4) { mstMin = 8; mstMax = 10; }// Lots: 8-10 mistakes
    else if (mistakeSetting == 5) { mstMin = 1; mstMax = 5; } // Random: 1-5 mistakes
    
    // Step 4: Setup random number generators
    std::random_device rd;                     // True random seed
    std::mt19937 gen(rd());                    // Mersenne Twister RNG
    std::uniform_int_distribution<> speedDist(speedMin, speedMax);  // Random delay
    std::uniform_int_distribution<> mstDist(mstMin, mstMax);        // Random mistake count
    
    int targetMistakes = mstDist(gen);   // How many mistakes this typing session will have
    int mistakesMade = 0;                // Counter for actual mistakes typed
    
    // Step 5: Main typing loop - process each character
    for (size_t i = 0; i < textToType.length(); i++) {
        if (stopTyping) break;           // STOP button was pressed
        while (isPaused) {               // Pause state: wait until resumed
            std::this_thread::sleep_for(std::chrono::milliseconds(100));
            if (stopTyping) break;
        }
        if (stopTyping) break;
        
        char c = textToType[i];
        
        // Handle newline characters (press Enter key)
        if (c == '\n' || c == '\r') {
            // Skip \r if followed by \n (Windows line ending compatibility)
            if (c == '\r' && i + 1 < textToType.length() && textToType[i + 1] == '\n') i++;
            keybd_event(VK_RETURN, 0, 0, 0);           // Press Enter
            keybd_event(VK_RETURN, 0, KEYEVENTF_KEYUP, 0); // Release Enter
            std::this_thread::sleep_for(std::chrono::milliseconds(400)); // Wait after newline
            continue;
        }
        
        // HUMAN-LIKE MISTAKE SIMULATION:
        // 5% chance per character to make a mistake (type 'x', backspace, retype correct char)
        if (mistakesMade < targetMistakes && textToType.length() > 5 && (gen() % 100 < 5)) {
            TypeChar('x');                                    // Type wrong character
            std::this_thread::sleep_for(std::chrono::milliseconds(speedDist(gen))); // Pause like a human
            keybd_event(VK_BACK, 0, 0, 0);                    // Press Backspace
            keybd_event(VK_BACK, 0, KEYEVENTF_KEYUP, 0);      // Release Backspace
            std::this_thread::sleep_for(std::chrono::milliseconds(speedDist(gen))); // Another pause
            mistakesMade++;
        }
        
        // Type the correct character
        TypeChar(c);
        std::this_thread::sleep_for(std::chrono::milliseconds(speedDist(gen))); // Human-like delay
    }
    
    // Step 6: Report completion status
    if (stopTyping) AddLog("> Stopped by user.");
    else AddLog("> Complete! Made " + std::to_string(mistakesMade) + " human-like mistakes.");
    
    // Reset all flags for next session
    isTyping = false;
    stopTyping = false;
    isPaused = false;
#endif
}

// ----------------------------------------------------------------------------
// FUNCTION: main
// PURPOSE: Entry point - sets up window, OpenGL, ImGui, and runs main loop
// ----------------------------------------------------------------------------

int main() {
    // ------------------------------
    // STEP 1: Initialize GLFW window
    // ------------------------------
    if (!glfwInit()) return 1;  // GLFW initialization failed
    
    glfwWindowHint(GLFW_DECORATED, GLFW_TRUE);  // Show window borders/title bar
    glfwWindowHint(GLFW_VISIBLE, GLFW_FALSE);   // Create hidden initially (we'll position first)
    GLFWwindow* window = glfwCreateWindow(800, 600, "CE Commander Engine v1.0", NULL, NULL);
    if (!window) return 1;

    // ------------------------------
    // STEP 2: Apply custom window icon (Windows only)
    // ------------------------------
#ifdef _WIN32
    HWND hwnd = glfwGetWin32Window(window);           // Get native Windows handle
    HICON hIcon = LoadIconA(GetModuleHandleA(NULL), "IDI_ICON1"); // Load from resources
    if (hIcon) {
        SendMessageA(hwnd, WM_SETICON, ICON_SMALL, (LPARAM)hIcon); // Small icon (title bar)
        SendMessageA(hwnd, WM_SETICON, ICON_BIG,   (LPARAM)hIcon); // Big icon (Alt+Tab)
    }
#endif

    // ------------------------------
    // STEP 3: Window positioning and visibility
    // ------------------------------
    glfwSetWindowAttrib(window, GLFW_FLOATING, GLFW_TRUE);  // Always on top
    glfwSetWindowSizeLimits(window, 800, 600, GLFW_DONT_CARE, GLFW_DONT_CARE); // Fixed minimum size
    const GLFWvidmode* mode = glfwGetVideoMode(glfwGetPrimaryMonitor());
    glfwSetWindowPos(window, (mode->width - 800) / 2, (mode->height - 600) / 2); // Center on screen
    glfwShowWindow(window);  // Now make it visible
    
    glfwMakeContextCurrent(window);
    glfwSwapInterval(1);     // Enable VSync

    // ------------------------------
    // STEP 4: Initialize Dear ImGui
    // ------------------------------
    IMGUI_CHECKVERSION();
    ImGui::CreateContext();
    ImGuiIO& io = ImGui::GetIO();
    ImGui::StyleColorsDark();  // Dark theme
    
    // Remove rounding from all UI elements (flat design)
    ImGuiStyle& style = ImGui::GetStyle();
    style.WindowRounding = style.ChildRounding = style.FrameRounding = style.PopupRounding = 
    style.ScrollbarRounding = style.GrabRounding = style.TabRounding = 0.0f;
    
    ImGui_ImplGlfw_InitForOpenGL(window, true);   // GLFW backend
    ImGui_ImplOpenGL3_Init("#version 130");       // OpenGL 3.0+ backend

    // ------------------------------
    // STEP 5: Log buffer for display
    // ------------------------------
    static char logBufRender[4096 * 4] = "";  // Big enough for all console output

    // ------------------------------
    // STEP 6: Main render loop
    // ------------------------------
    while (!glfwWindowShouldClose(window)) {
        glfwPollEvents();  // Process window events (keyboard, mouse, etc.)
        
        ImGui_ImplOpenGL3_NewFrame();
        ImGui_ImplGlfw_NewFrame();
        ImGui::NewFrame();
        
        // Create full-window layout (no title bar, no resize, no move)
        ImGui::SetNextWindowPos(ImVec2(0,0));
        ImGui::SetNextWindowSize(io.DisplaySize);
        ImGui::Begin("MainLayout", nullptr, ImGuiWindowFlags_NoTitleBar | ImGuiWindowFlags_NoResize | 
                     ImGuiWindowFlags_NoMove | ImGuiWindowFlags_NoCollapse);
        
        // ------------------------------
        // UI SECTION: Status bar
        // ------------------------------
        const char* speeds[] = { "Very Slow", "Slow", "Medium", "Fast", "Very Fast" };
        const char* mistRanges[] = { "None", "Few", "Some", "Many", "Lots", "Random" };
        const char* currentStatus = isTyping ? (isPaused ? "pause" : "start") : "idle";
        
        ImGui::SetCursorPos(ImVec2(20, 15));
        ImGui::Text("Status : (%s) : speed (%s) : mistake (%s)", 
                    currentStatus, speeds[spdIdx], mistRanges[mstIdx]);
        
        // ------------------------------
        // UI SECTION: Text input area (multiline)
        // ------------------------------
        ImGui::SetCursorPos(ImVec2(20, 40));
        if (ImGui::InputTextMultiline("##In", inputBuf, sizeof(inputBuf), ImVec2(760, 120))) {
            totalWords = CountWords(inputBuf);  // Update word count live
        }
        
        // ------------------------------
        // UI SECTION: Default string button
        // ------------------------------
        ImGui::SetCursorPos(ImVec2(20, 175));
        if (ImGui::Button("Default String", ImVec2(120, 30))) {
            std::string defStr = "This is simple auto writer tool used for good propose.\nFree to use.\nGreat to achieve your goals.";
            snprintf(inputBuf, sizeof(inputBuf), "%s", defStr.c_str());
            totalWords = CountWords(defStr);
            AddLog("> Loaded default string.");
        }
        
        // ------------------------------
        // UI SECTION: Speed combo box
        // ------------------------------
        ImGui::SetCursorPos(ImVec2(20, 220));
        ImGui::Text("Speed:");
        ImGui::SetCursorPos(ImVec2(70, 218));
        ImGui::SetNextItemWidth(120);
        ImGui::Combo("##Spd", &spdIdx, "Very Slow\0Slow\0Medium\0Fast\0Very Fast\0\0");
        
        // ------------------------------
        // UI SECTION: Mistakes combo box
        // ------------------------------
        ImGui::SetCursorPos(ImVec2(210, 220));
        ImGui::Text("Mistakes:");
        ImGui::SetCursorPos(ImVec2(280, 218));
        ImGui::SetNextItemWidth(120);
        ImGui::Combo("##Mst", &mstIdx, "None\0Few\0Some\0Many\0Lots\0Random\0\0");
        
        // ------------------------------
        // UI SECTION: Control buttons (START/STOP/PAUSE/RESUME)
        // ------------------------------
        if (!isTyping) {
            // SHOW START BUTTON (only when not typing)
            ImGui::SetCursorPos(ImVec2(20, 270));
            if (ImGui::Button("START", ImVec2(100, 30))) {
                std::string textToType(inputBuf);
                if (textToType.empty()) {
                    AddLog("> ERROR: No text loaded to type!");
                } else {
                    // Reset flags and launch worker thread
                    stopTyping = false;
                    isPaused = false;
                    isTyping = true;
                    std::thread(TypingWorker, spdIdx, mstIdx, textToType).detach(); // Detach = run independently
                }
            }
        } else {
            // SHOW STOP BUTTON (when typing active)
            ImGui::SetCursorPos(ImVec2(20, 270));
            if (ImGui::Button("STOP", ImVec2(100, 30))) {
                stopTyping = true;
                isPaused = false;
                AddLog("> Sent stop signal to engine...");
            }
            
            // SHOW PAUSE/RESUME BUTTON (when typing active)
            ImGui::SetCursorPos(ImVec2(130, 270));
            if (ImGui::Button(isPaused ? "RESUME" : "PAUSE", ImVec2(100, 30))) {
                isPaused = !isPaused;
                AddLog(isPaused ? "> Engine Paused." : "> Engine Resumed.");
            }
        }
        
        // ------------------------------
        // UI SECTION: Console log display (read-only)
        // ------------------------------
        {
            std::lock_guard<std::mutex> lock(logMutex);  // Thread-safe copy
            snprintf(logBufRender, sizeof(logBufRender), "%s", engineLog.c_str());
        }
        ImGui::SetCursorPos(ImVec2(20, 320));
        ImGui::InputTextMultiline("##Log", logBufRender, sizeof(logBufRender), 
                                  ImVec2(760, ImGui::GetWindowHeight() - 340), 
                                  ImGuiInputTextFlags_ReadOnly);  // User can't edit console
        
        ImGui::End();  // End MainLayout window
        
        // ------------------------------
        // STEP 7: Rendering (OpenGL)
        // ------------------------------
        ImGui::Render();
        int dw, dh;
        glfwGetFramebufferSize(window, &dw, &dh);
        glViewport(0, 0, dw, dh);
        glClearColor(0.1f, 0.1f, 0.1f, 1.0f);  // Dark gray background
        glClear(GL_COLOR_BUFFER_BIT);
        ImGui_ImplOpenGL3_RenderDrawData(ImGui::GetDrawData());
        glfwSwapBuffers(window);  // Double buffering
    }
    
    // ------------------------------
    // STEP 8: Cleanup
    // ------------------------------
    ImGui_ImplOpenGL3_Shutdown();
    ImGui_ImplGlfw_Shutdown();
    ImGui::DestroyContext();
    glfwDestroyWindow(window);
    glfwTerminate();
    return 0;
}

// ----------------------------------------------------------------------------
// SECTION 7: WINDOWS ENTRY POINT REDIRECT
// Purpose: Allow Windows to start the app without a console window
// ----------------------------------------------------------------------------

#ifdef _WIN32
int APIENTRY WinMain(HINSTANCE hInst, HINSTANCE hInstPrev, PSTR cmdline, int cmdshow) {
    return main();  // Just call standard main()
}
#endif
