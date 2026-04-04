#include "imgui.h"
#include "imgui_impl_glfw.h"
#include "imgui_impl_opengl3.h"
#include <GLFW/glfw3.h>
#include <string>
#include <thread>
#include <atomic>
#include <mutex>
#include <fstream>
#include <vector>

#ifdef _WIN32
#define GLFW_EXPOSE_NATIVE_WIN32
#include <GLFW/glfw3native.h> 
#include <windows.h>
#endif

// Now we only need the TypingEngine
#include "TypingEngine.h"

static int spdIdx = 2;
static int mstIdx = 0;
static int totalWords = 15;

std::atomic<bool> isTyping(false);
std::atomic<bool> isPaused(false);
std::atomic<bool> stopTyping(false);

std::string engineLog = "[12:00:00] CE Commander Engine V1.5 \n> Native Windows Input Control Active (Perfect Universal Text).\n> Sleek Dark Mode Scrollbar Active.\n> Clipboard & Notepad Ready.\n";
std::mutex logMutex;

#ifdef _WIN32
static int hkStart = VK_F7;
static int hkPause = VK_F8;
static int hkResume = VK_F9;
static int hkStop = VK_F10;
static int hkStatus = VK_F11;
static int tempStart = VK_F7, tempPause = VK_F8, tempResume = VK_F9, tempStop = VK_F10, tempStatus = VK_F11;

HWND hwndEdit = NULL;
WNDPROC originalWindowProc = nullptr;

LRESULT CALLBACK WindowSubclassProc(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam) {
    if (uMsg == WM_CTLCOLOREDIT || uMsg == WM_CTLCOLORSTATIC) {
        HWND hControl = (HWND)lParam;
        if (GetDlgCtrlID(hControl) == 1001) { 
            HDC hdc = (HDC)wParam;
            SetTextColor(hdc, RGB(230, 230, 235)); 
            SetBkColor(hdc, RGB(17, 24, 39));      
            static HBRUSH bgBrush = CreateSolidBrush(RGB(17, 24, 39));
            return (LRESULT)bgBrush;
        }
    }
    return CallWindowProc(originalWindowProc, hWnd, uMsg, wParam, lParam);
}
#endif

static bool openHotkeyModal = false;
static bool openCaptureModal = false;
static int activeCaptureId = -1;

static bool isDragging = false;
static double dragOffsetX, dragOffsetY;

std::atomic<bool> notepadTextReady(false);
std::string notepadLoadedText = "";
std::mutex notepadMutex;

void AddLog(const std::string& msg) {
    std::lock_guard<std::mutex> lock(logMutex);
    engineLog += msg + "\n";
}

int CountWordsW(const std::wstring& str) {
    int count = 0; bool inWord = false;
    for (wchar_t c : str) {
        if (c == L' ' || c == L'\n' || c == L'\r' || c == L'\t') inWord = false;
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

std::wstring GetNativeText() {
    if (!hwndEdit) return L"";
    int len = GetWindowTextLengthW(hwndEdit);
    if (len == 0) return L"";
    std::wstring wtext(len + 1, L'\0');
    GetWindowTextW(hwndEdit, &wtext[0], len + 1);
    wtext.resize(len);
    return wtext;
}

int main() {
#ifdef _WIN32
    HANDLE hMutex = CreateMutexA(NULL, FALSE, "CE_AutoTyper_SingleInstance_Mutex_v1_5");
    if (GetLastError() == ERROR_ALREADY_EXISTS) {
        if (hMutex) { CloseHandle(hMutex); }
        return 0; 
    }
#endif

    if (!glfwInit()) return 1;
    
    glfwWindowHint(GLFW_DECORATED, GLFW_FALSE);
    glfwWindowHint(GLFW_VISIBLE, GLFW_FALSE);
    
    GLFWwindow* window = glfwCreateWindow(800, 640, "CE Commander Engine v1.5", NULL, NULL);
    if (!window) return 1;

#ifdef _WIN32
    HWND hwnd = glfwGetWin32Window(window);
    HICON hIcon = LoadIconA(GetModuleHandleA(NULL), "IDI_ICON1");
    if (hIcon) {
        SendMessageA(hwnd, WM_SETICON, ICON_SMALL, (LPARAM)hIcon);
        SendMessageA(hwnd, WM_SETICON, ICON_BIG,   (LPARAM)hIcon);
    }

    LONG_PTR styleFlags = GetWindowLongPtr(hwnd, GWL_STYLE);
    SetWindowLongPtr(hwnd, GWL_STYLE, styleFlags | WS_CLIPCHILDREN);

    originalWindowProc = (WNDPROC)SetWindowLongPtr(hwnd, GWLP_WNDPROC, (LONG_PTR)WindowSubclassProc);

    hwndEdit = CreateWindowExW(
        0, L"EDIT", L"This is simple auto writer tool used for good propose.\r\nFree to use.\r\nGreat to achieve your goals.",
        WS_CHILD | WS_VISIBLE | WS_VSCROLL | ES_MULTILINE | ES_WANTRETURN | ES_AUTOVSCROLL,
        20, 70, 760, 120, 
        hwnd, (HMENU)1001, GetModuleHandle(NULL), NULL);

    HFONT hFont = CreateFontW(18, 0, 0, 0, FW_NORMAL, FALSE, FALSE, FALSE, DEFAULT_CHARSET, OUT_DEFAULT_PRECIS, CLIP_DEFAULT_PRECIS, DEFAULT_QUALITY, DEFAULT_PITCH | FF_SWISS, L"Segoe UI");
    SendMessageW(hwndEdit, WM_SETFONT, (WPARAM)hFont, TRUE);

    HMODULE hUxTheme = LoadLibraryA("uxtheme.dll");
    if (hUxTheme) {
        typedef HRESULT(WINAPI *SetWindowTheme_Fn)(HWND, LPCWSTR, LPCWSTR);
        SetWindowTheme_Fn pSetWindowTheme = (SetWindowTheme_Fn)GetProcAddress(hUxTheme, "SetWindowTheme");
        if (pSetWindowTheme) {
            pSetWindowTheme(hwndEdit, L"DarkMode_Explorer", NULL);
        }
        FreeLibrary(hUxTheme);
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
    
    ImFontConfig fontCfg;
    fontCfg.OversampleH = 1; fontCfg.OversampleV = 1;
    io.Fonts->AddFontDefault(&fontCfg);

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
    colors[ImGuiCol_Button]                 = ImVec4(0.16f, 0.40f, 0.75f, 1.00f); 
    colors[ImGuiCol_ButtonHovered]          = ImVec4(0.20f, 0.48f, 0.88f, 1.00f);
    colors[ImGuiCol_ButtonActive]           = ImVec4(0.12f, 0.32f, 0.62f, 1.00f);
    colors[ImGuiCol_Header]                 = ImVec4(0.16f, 0.40f, 0.75f, 0.80f);
    colors[ImGuiCol_HeaderHovered]          = ImVec4(0.20f, 0.48f, 0.88f, 0.80f);
    colors[ImGuiCol_HeaderActive]           = ImVec4(0.12f, 0.32f, 0.62f, 1.00f);
    
    style.WindowRounding = style.ChildRounding = style.FrameRounding = style.PopupRounding = style.ScrollbarRounding = style.GrabRounding = style.TabRounding = 0.0f;
    style.FrameBorderSize = 1.0f;

    ImGui_ImplGlfw_InitForOpenGL(window, true);
    ImGui_ImplOpenGL3_Init("#version 130");
    static char logBufRender[4096 * 4] = "";

    while (!glfwWindowShouldClose(window)) {
        glfwPollEvents();

        if (IsKeyJustPressed(hkStart) && !isTyping && !openCaptureModal && !openHotkeyModal) {
            std::wstring textToType = GetNativeText();
            if (!textToType.empty()) {
                stopTyping = false; isPaused = false; isTyping = true;
                std::thread(TypingEngine::Worker, spdIdx, mstIdx, textToType).detach();
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

        if (notepadTextReady) {
            std::lock_guard<std::mutex> lock(notepadMutex);
            std::wstring wNotepad = TypingEngine::Utf8ToUtf16(notepadLoadedText);
            SetWindowTextW(hwndEdit, wNotepad.c_str());
            totalWords = CountWordsW(wNotepad);
            AddLog("> Loaded " + std::to_string(totalWords) + " words from Notepad Editor.");
            notepadTextReady = false;
        }

        ImGui_ImplOpenGL3_NewFrame();
        ImGui_ImplGlfw_NewFrame();
        ImGui::NewFrame();
        ImGui::SetNextWindowPos(ImVec2(0,0));
        ImGui::SetNextWindowSize(io.DisplaySize);
        ImGui::Begin("MainLayout", nullptr, ImGuiWindowFlags_NoTitleBar | ImGuiWindowFlags_NoResize | ImGuiWindowFlags_NoMove | ImGuiWindowFlags_NoCollapse);

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
        ImGui::TextColored(ImVec4(0.6f, 0.7f, 0.9f, 1.0f), "CE Commander Engine v1.5");

        ImGui::SameLine(ImGui::GetWindowWidth() - 40);
        ImGui::SetCursorPosY(0);
        ImGui::PushStyleColor(ImGuiCol_Button, ImVec4(0.8f, 0.2f, 0.2f, 1.0f));
        ImGui::PushStyleColor(ImGuiCol_ButtonHovered, ImVec4(1.0f, 0.3f, 0.3f, 1.0f));
        if (ImGui::Button("X", ImVec2(40, 30))) {
            glfwSetWindowShouldClose(window, true);
        }
        ImGui::PopStyleColor(2);

        ImGui::EndChild();
        ImGui::PopStyleColor();

        totalWords = CountWordsW(GetNativeText());

        const char* speeds[] = { "Very Slow", "Slow", "Medium", "Fast", "Very Fast" };
        const char* mistRanges[] = { "None", "Few", "Some", "Many", "Lots", "Random" };
        const char* currentStatus = isTyping ? (isPaused ? "pause" : "start") : "idle";
        
        ImGui::SetCursorPos(ImVec2(20, 45));
        ImGui::Text("Status : (%s) : speed (%s) : mistake (%s) : Words (%d)", currentStatus, speeds[spdIdx], mistRanges[mstIdx], totalWords);
        
        ImGui::SetCursorPos(ImVec2(20, 70));
        ImGui::Dummy(ImVec2(760, 120)); 
        
        char hkBuf[256];
        snprintf(hkBuf, sizeof(hkBuf), "Hotkeys -> Start: %s | Pause: %s | Resume: %s | Stop: %s | Status: %s", 
            GetKeyName(hkStart).c_str(), GetKeyName(hkPause).c_str(), GetKeyName(hkResume).c_str(), GetKeyName(hkStop).c_str(), GetKeyName(hkStatus).c_str());
        float textWidth = ImGui::CalcTextSize(hkBuf).x;
        ImGui::SetCursorPos(ImVec2(800 - textWidth - 20, 195));
        ImGui::TextColored(ImVec4(0.4f, 0.8f, 0.4f, 1.0f), "%s", hkBuf);

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
                std::wstring currentText = GetNativeText();
                ofs << TypingEngine::Utf16ToUtf8(currentText); 
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
        if (ImGui::Button("Clipboard", ImVec2(100, 30))) {
            const char* clipboardText = glfwGetClipboardString(window);
            if (clipboardText && strlen(clipboardText) > 0) {
                std::wstring wClip = TypingEngine::Utf8ToUtf16(clipboardText);
                SetWindowTextW(hwndEdit, wClip.c_str());
                AddLog("> Loaded text from system Clipboard.");
            } else {
                AddLog("> ERROR: System Clipboard is empty.");
            }
        }

        ImGui::SetCursorPos(ImVec2(350, 225));
        if (ImGui::Button("Default String", ImVec2(120, 30))) {
            std::wstring defStr = L"This is simple auto writer tool used for good propose.\r\nFree to use.\r\nGreat to achieve your goals.";
            SetWindowTextW(hwndEdit, defStr.c_str());
            AddLog("> Loaded default string.");
        }
        
        ImGui::SetCursorPos(ImVec2(20, 270)); ImGui::Text("Speed:");
        ImGui::SetCursorPos(ImVec2(70, 268)); ImGui::SetNextItemWidth(120);
        ImGui::Combo("##Spd", &spdIdx, "Very Slow\0Slow\0Medium\0Fast\0Very Fast\0\0");
        
        ImGui::SetCursorPos(ImVec2(210, 270)); ImGui::Text("Mistakes:");
        ImGui::SetCursorPos(ImVec2(280, 268)); ImGui::SetNextItemWidth(120);
        ImGui::Combo("##Mst", &mstIdx, "None\0Few\0Some\0Many\0Lots\0Random\0\0");
        
        if (!isTyping) {
            ImGui::SetCursorPos(ImVec2(20, 315));
            ImGui::PushStyleColor(ImGuiCol_Button, ImVec4(0.2f, 0.6f, 0.3f, 1.0f));
            ImGui::PushStyleColor(ImGuiCol_ButtonHovered, ImVec4(0.3f, 0.7f, 0.4f, 1.0f));
            if (ImGui::Button("START", ImVec2(100, 35))) {
                std::wstring textToType = GetNativeText();
                if (textToType.empty()) AddLog("> ERROR: No text loaded to type!");
                else { stopTyping = false; isPaused = false; isTyping = true; std::thread(TypingEngine::Worker, spdIdx, mstIdx, textToType).detach(); }
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
    if (hMutex) { CloseHandle(hMutex); } 
#endif
    return 0;
}

#ifdef _WIN32
int APIENTRY WinMain(HINSTANCE hInst, HINSTANCE hInstPrev, PSTR cmdline, int cmdshow) { return main(); }
#endif