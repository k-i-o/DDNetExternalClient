using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static DDNetExternalClient.Overlay;

namespace DDNetExternalClient
{
    public class Overlay
    {

        public static IntPtr hand;

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(Keys vKey);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string IpClassName, string IpWindowName);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]

        public static extern bool GetWindowRect(IntPtr hwnd, out RECT IpRect);

        public static RECT rect;

        public bool threadwire = true;

        public struct RECT
        {
            public int left, top, right, bottom;
        }

        public void setHandle(string window_name)
        {
            hand = FindWindow(null, window_name);

        }

        public void SetInvi(Form form)
        {
            form.BackColor = Color.Wheat;
            form.TransparencyKey = Color.Wheat;
            form.TopMost = true;
            form.FormBorderStyle = FormBorderStyle.None;

            ClickThrough(form.Handle);
        }

        public void GetRekt()
        {
            GetWindowRect(hand, out rect);
        }

        public void ClickThrough(IntPtr formHandle)
        {
            int initialStyle = GetWindowLong(formHandle, -20);
            SetWindowLong(formHandle, -20, initialStyle | 0x8000 | 0x20);

        }

        public Size CalcSize()
        {
            Size size = new Size(rect.right - rect.left, rect.bottom - rect.top);
            return size;
        }

        public void DoStuff(string WindowName, Form form)
        {
            setHandle(WindowName);
            GetRekt();
            form.Size = CalcSize();
            form.Left = rect.left;
            form.Top = rect.top;

        }

        public void PauseLoop()
        {
            threadwire = false;
        }

        public void UnPauseLoop()
        {
            threadwire = true;
        }
        public void StartLoop(int frequency, string WindowName, Form form)
        {
            Thread lp = new Thread(() => LOOP(frequency, WindowName, form)) { IsBackground = true };
            lp.Start();

        }




        public void LOOP(int frequency, string WindowName, Form form)
        {
            while (true)
            {
                if (threadwire == true)
                {
                    DoStuff(WindowName, form);

                }
                Thread.Sleep(frequency);

            }

        }

        public bool ProcessIsFullScreen(string WindowName)
        {
            Process? process = Process.GetProcessesByName(WindowName).FirstOrDefault();

            bool isFullScreen = false;

            if (process is not null)
            {
                IntPtr hWnd = process.MainWindowHandle;
                RECT rect = new RECT();
                GetWindowRect(hWnd, out rect);
                int width = rect.right - rect.left;
                int height = rect.bottom - rect.top;
                isFullScreen = width == Screen.PrimaryScreen.Bounds.Width && height == Screen.PrimaryScreen.Bounds.Height;

            }

            return isFullScreen;
        }
    }
}