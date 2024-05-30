using Playnite.SDK;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Linq;
using Playnite.Native;      // CopyPasted form Playnite sources

namespace BackToGame.Locator {
    public static class ProcessTreeLocator
    {
        private static readonly ILogger logger = LogManager.GetLogger();


        public static void BringProcessToFront(int processId)
        {
            var hwnds = GetProcessWindows(processId);

            foreach (IntPtr hwnd in hwnds)
            {
                if (!User32.IsWindowVisible(hwnd)) continue;
                if (!User32.IsIconic(hwnd))
                {
                    User32.ShowWindow(hwnd, Winuser.SW_MINIMIZE);
                }
                User32.ShowWindow(hwnd, Winuser.SW_RESTORE);
            }
        }

        static private List<int> GetProcessTreeIds(int rootProcess)
        {
            List<int> relatedIds = new List<int>{rootProcess};

            var runningIds = new List<int>();
            foreach (Process proc in Process.GetProcesses().Where(a => a.SessionId != 0))
            {
                if (proc.TryGetParentId(out var parent))
                {
                    if (relatedIds.Contains(parent) && !relatedIds.Contains(proc.Id))
                    {
                        relatedIds.Add(proc.Id);
                    }
                }

                if (relatedIds.Contains(proc.Id))
                {
                    runningIds.Add(proc.Id);
                }
            }

            return runningIds;
        }
        static private List<IntPtr> GetProcessWindows(int processId)
        {
            var processIds = GetProcessTreeIds(processId);
            List<IntPtr> hWnds = new List<IntPtr>();

            IntPtr hLastWnd = IntPtr.Zero;
            do {
                hLastWnd = User32.FindWindowEx(IntPtr.Zero, hLastWnd, null, null);
                User32.GetWindowThreadProcessId(hLastWnd, out IntPtr hProcess);

                if (processIds.Contains(hProcess.ToInt32()))
                    hWnds.Add(hLastWnd);
            } while(hLastWnd != IntPtr.Zero);

            return hWnds;
        }
    }

}