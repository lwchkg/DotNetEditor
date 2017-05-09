using System;
using System.Runtime.InteropServices;

namespace DotNetEditor
{
    static class ConsoleUtil
    {
        [DllImport("Kernel32.dll", EntryPoint = "AllocConsole", ExactSpelling = false,
            CharSet = CharSet.Unicode, SetLastError = true)]
        public extern static bool AllocConsole();

        [DllImport("Kernel32.dll", EntryPoint = "GetConsoleWindow", ExactSpelling = false,
            CharSet = CharSet.Unicode)]
        public extern static IntPtr GetConsoleWindow();

        [DllImport("user32.dll", EntryPoint = "ShowWindow", ExactSpelling = false,
            CharSet = CharSet.Unicode)]
        public extern static bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static bool HasConsoleWindow()
        {
            IntPtr handle = ConsoleUtil.GetConsoleWindow();
            return handle != IntPtr.Zero;
        }

        public static void CreateHiddenConsoleWindowIfNotExists()
        {
            if (!HasConsoleWindow())
            {
                AllocConsole();
                IntPtr handle = GetConsoleWindow();
                ShowWindow(handle, 0);
            }
        }
    }
}
