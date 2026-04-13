using System;
using System.Threading;
using System.Windows.Forms;

namespace CeAutoTyper
{
    static class Program
    {
        private static Mutex? mutex = null;

        [STAThread]
        static void Main()
        {
            const string appName = "CEAutoTyper";
            bool createdNew;

            mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                IntPtr hWnd = NativeMethods.FindWindow(null, "CE Auto Typer v1.9");
                if (hWnd != IntPtr.Zero)
                {
                    NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE);
                    NativeMethods.SetForegroundWindow(hWnd);
                }
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());

            mutex.ReleaseMutex();
        }
    }

    internal static class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public const int SW_RESTORE = 9;
    }
}