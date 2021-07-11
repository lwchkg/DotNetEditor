// Copyright 2017 Leung Wing-chung. All rights reserved.
// Use of this source code is governed by a GPLv3 license that can be found in
// the LICENSE file.

using System;
using System.Runtime.InteropServices;

namespace DotNetEditor
{
    static class ConsoleUtil
    {
        [DllImport("Kernel32.dll", EntryPoint = "AllocConsole", ExactSpelling = false,
            CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool AllocConsole();

        [DllImport("Kernel32.dll", EntryPoint = "GetConsoleWindow", ExactSpelling = false,
            CharSet = CharSet.Unicode)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll", EntryPoint = "ShowWindow", ExactSpelling = false,
            CharSet = CharSet.Unicode)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static void HideConsoleWindow()
        {
            IntPtr handle = GetConsoleWindow();
            if (handle == IntPtr.Zero)
            {
                AllocConsole();
                handle = GetConsoleWindow();
            }
            ShowWindow(handle, 0);
        }
    }
}
