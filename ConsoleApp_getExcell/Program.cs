using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Automation;

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

        // 定義按鈕的訊息
        const int BM_CLICK = 0x00F5;

        static void Main(string[] args)
        {
            //Process process = new Process();
            //process.StartInfo.FileName = "C:\\Program Files\\DeskIn\\DeskIn.exe";
            //process.StartInfo.CreateNoWindow = false;
            //process.StartInfo.UseShellExecute = true;
            //process.Start();
            //process.WaitForInputIdle();
            IntPtr mainWindowHandle = FindWindow(null, "程式");

            if (mainWindowHandle == IntPtr.Zero)
            {
                Console.WriteLine("找不到目標視窗。");
                return;
            }
            else
            {
                Console.WriteLine("找到目標視窗。");
            }
            AutomationElement mainWindow = AutomationElement.FromHandle(mainWindowHandle);
            if (mainWindow == null)
            {
                Console.WriteLine("找不到主視窗的 AutomationElement。");
                return;
            }

            // 根據 AutomationId 查找按鈕
            AutomationElement button = mainWindow.FindFirst(
                TreeScope.Descendants,
                new PropertyCondition(AutomationElement.AutomationIdProperty, "010000806E091400FCFFFFFF01000000"));

            if (button == null)
            {
                Console.WriteLine("找不到指定 AutomationId 的按鈕。");
                return;
            }
            //IntPtr buttonHandle = FindWindowEx(mainWindowHandle, IntPtr.Zero, null, "查詢資料");

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

            //IntPtr childHandle = IntPtr.Zero;
            //StringBuilder buttonText = new StringBuilder(256);
            //while ((childHandle = FindWindowEx(mainWindowHandle, childHandle, null, null)) != IntPtr.Zero)
            //{
            //    // 獲取子視窗的文字
            //    GetWindowText(childHandle, buttonText, buttonText.Capacity);
            //    Console.WriteLine($"{buttonText.ToString()}");
            //    // 檢查文字是否符合按鈕的文字
            //    if (buttonText.ToString() == "Button Text Here")
            //    {
            //        Console.WriteLine("找到按鈕，句柄：" + childHandle);
            //        // 在這裡你可以對按鈕進行操作，例如模擬點擊
            //        break;
            //    }
            //}

            //// 模擬按鈕點擊
            //SendMessage(buttonHandle, BM_CLICK, IntPtr.Zero, IntPtr.Zero);

            //Console.WriteLine("按鈕已點擊！");

        }
    }
}
