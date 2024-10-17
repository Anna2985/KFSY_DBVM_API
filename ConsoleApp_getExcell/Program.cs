using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace ConsoleApp_getExcell
{
    class Program
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        // 定義按鈕的訊息
        const int BM_CLICK = 0x00F5;

        static void Main(string[] args)
        {
            Process process = new Process();
            process.StartInfo.FileName = "C:\\Program Files\\DeskIn\\DeskIn.exe";
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.UseShellExecute = true;
            process.Start();
            process.WaitForInputIdle();
            IntPtr mainWindowHandle = FindWindow(null, "DeskIn");

            if (mainWindowHandle == IntPtr.Zero)
            {
                Console.WriteLine("找不到目標視窗。");
                return;
            }
            else
            {
                Console.WriteLine("找到目標視窗。");
            }

            IntPtr childHandle = IntPtr.Zero;
            StringBuilder buttonText = new StringBuilder(256);
            while ((childHandle = FindWindowEx(mainWindowHandle, childHandle, null, null)) != IntPtr.Zero)
            {
                // 獲取子視窗的文字
                GetWindowText(childHandle, buttonText, buttonText.Capacity);
                Console.WriteLine($"{buttonText.ToString()}");
                // 檢查文字是否符合按鈕的文字
                if (buttonText.ToString() == "Button Text Here")
                {
                    Console.WriteLine("找到按鈕，句柄：" + childHandle);
                    // 在這裡你可以對按鈕進行操作，例如模擬點擊
                    break;
                }
            }
            //IntPtr buttonHandle = FindWindowEx(mainWindowHandle, IntPtr.Zero, null, "設備列表");

            //if (buttonHandle == IntPtr.Zero)
            //{
            //    Console.WriteLine("找不到按鈕。");
            //    return;
            //}
            //else
            //{
            //    Console.WriteLine("找到按鈕。");
            //}

            //// 模擬按鈕點擊
            //SendMessage(buttonHandle, BM_CLICK, IntPtr.Zero, IntPtr.Zero);

            //Console.WriteLine("按鈕已點擊！");

        }
    }
}
