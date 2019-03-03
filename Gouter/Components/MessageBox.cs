using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Gouter.Components.NativeMethods;

namespace Gouter.Components
{
    internal class MessageBox : IDisposable
    {
        public MessageBox()
        {
        }

        public MessageBox(IntPtr hWnd)
        {
            this.hwnd = hWnd;
        }

        private IntPtr hwnd;
        private IntPtr hook;

        public void SetHandle(IntPtr hWnd)
        {
            this.hwnd = hWnd;
        }

        public bool CenterOwner { get; } = true;

        public MessageBoxResult Show(string text, string caption = null, MessageBoxButtons buttons = 0, MessageBoxIcon icon = 0, MessageBoxFlags flags = 0)
        {
            if (hwnd != IntPtr.Zero && this.CenterOwner)
            {
                IntPtr hInst = GetWindowLong(hwnd, GWL.HINSTANCE);
                IntPtr thrId = GetCurrentThreadId();
                hook = SetWindowsHookEx(WH.CBT, HookPrc, hInst, thrId);
            }

            return (MessageBoxResult)MessageBox(hwnd, text, caption, (MB)buttons | (MB)icon | (MB)flags);
        }

        public static MessageBoxResult Show(IntPtr hWnd, string text, string caption = null, MessageBoxButtons buttons = 0, MessageBoxIcon icon = 0, MessageBoxFlags flags = 0)
        {
            return (MessageBoxResult)MessageBox(hWnd, text, caption, (MB)buttons | (MB)icon | (MB)flags);
        }

        private IntPtr HookPrc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode == (int)HCBT.ACTIVATE)
            {
                IntPtr res;
                int x, y;

                GetWindowRect(hwnd, out var parentRect);
                GetWindowRect(wParam, out var msgBoxRect);

                x = (parentRect.left + ((parentRect.right - parentRect.left) / 2)) - ((msgBoxRect.right - msgBoxRect.left) / 2);
                y = (parentRect.top + ((parentRect.bottom - parentRect.top) / 2)) - ((msgBoxRect.bottom - msgBoxRect.top) / 2);

                SetWindowPos(wParam, IntPtr.Zero, x, y, 0, 0, SWP.NOSIZE | SWP.NOZORDER | SWP.NOACTIVATE);

                res = CallNextHookEx(hwnd, nCode, wParam, lParam);

                UnhookWindowsHookEx(hook);
                hook = IntPtr.Zero;

                return res;
            }
            else
            {
                return CallNextHookEx(hook, nCode, wParam, lParam);
            }
        }

        public void Dispose()
        {
            hook = IntPtr.Zero;
            hwnd = IntPtr.Zero;
        }
    }

    [Flags]
    internal enum MessageBoxButtons
    {
        Ok = MB.MB_OK,
        OkCancel = MB.MB_OKCANCEL,
        AbortRetryIgnore = MB.MB_ABORTRETRYIGNORE,
        YesNoCancel = MB.MB_YESNOCANCEL,
        YesNo = MB.MB_YESNO,
        RetryCancel = MB.MB_RETRYCANCEL,
        CancelTryContinue = MB.MB_CANCELTRYCONTINUE,
        Help = MB.MB_HELP
    }

    internal enum MessageBoxIcon : int
    {
        Stop = MB.MB_ICONSTOP,
        Error = MB.MB_ICONERROR,
        Hand = MB.MB_ICONHAND,
        Question = MB.MB_ICONQUESTION,
        Exclamation = MB.MB_ICONEXCLAMATION,
        Warning = MB.MB_ICONWARNING,
        Information = MB.MB_ICONINFORMATION,
        Asterisk = MB.MB_ICONASTERISK
    }

    [Flags]
    internal enum MessageBoxFlags
    {
        DefButton1 = MB.MB_DEFBUTTON1,
        AppModal = MB.MB_APPMODAL,
        DefButton2 = MB.MB_DEFBUTTON2,
        DefButton3 = MB.MB_DEFBUTTON3,
        DefButton4 = MB.MB_DEFBUTTON4,
        SystemModal = MB.MB_SYSTEMMODAL,
        TaskModal = MB.MB_TASKMODAL,
        SetForeground = MB.MB_SETFOREGROUND,
        DefaultDesktopOnly = MB.MB_DEFAULT_DESKTOP_ONLY,
        TopMost = MB.MB_TOPMOST,
        Right = MB.MB_RIGHT,
        RTLReading = MB.MB_RTLREADING,
        ServiceNotification = MB.MB_SERVICE_NOTIFICATION
    }

    internal enum MessageBoxResult
    {
        Ok = ID.OK,
        Cancel = ID.CANCEL,
        Abort = ID.ABORT,
        Retry = ID.RETRY,
        Ignore = ID.IGNORE,
        Yes = ID.YES,
        No = ID.NO,
        TryAgain = ID.TRYAGAIN,
        Continue = ID.CONTINUE
    }
}