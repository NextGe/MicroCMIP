using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EFWCoreLib.CoreFrame.Common
{
    public class WindowsAPI
    {
        public const int WM_ASYN_INPUT = 0x0400 + 7201;      //异步加载消息

        #region 窗口消息函数
        [DllImport("user32.dll", EntryPoint = "PostMessageA", CharSet = CharSet.Ansi)]
        public static extern int PostMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        [DllImport("user32.dll", EntryPoint = "SendMessageA", CharSet = CharSet.Ansi)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        [DllImport("user32.dll", EntryPoint = "GetForegroundWindow", CharSet = CharSet.Ansi)]
        public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", EntryPoint = "IsWindowVisible", CharSet = CharSet.Ansi)]
        public static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll", EntryPoint = "GetParent", CharSet = CharSet.Ansi)]
        public static extern IntPtr GetParent(IntPtr hWnd);
        [DllImport("user32.dll", EntryPoint = "GetTopWindow", CharSet = CharSet.Ansi)]
        public static extern IntPtr GetTopWindow(IntPtr hWnd);
        #endregion

        [DllImport("kernel32.dll")]
        public static extern int CreateWaitableTimer(int lpTimerAttributes, bool bManualReset, int lpTimerName);


        [DllImport("kernel32.dll")]
        public static extern bool SetWaitableTimer(int hTimer, ref long pDueTime,
            int lPeriod, int pfnCompletionRoutine, // TimerCompleteDelegate  
            int lpArgToCompletionRoutine, bool fResume);


        [DllImport("user32.dll")]
        public static extern bool MsgWaitForMultipleObjects(uint nCount, ref int pHandles,
            bool bWaitAll, int dwMilliseconds, uint dwWakeMask);


        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(int hObject);


        public const int NULL = 0;
        public const int QS_TIMER = 0x10;
    }
}
