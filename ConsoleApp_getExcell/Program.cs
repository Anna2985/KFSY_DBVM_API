using System;
using System.Diagnostics;

namespace ConsoleApp_getExcell
{
    class Program
    {
        static void Main(string[] args)
        {
            Process process = new Process();
            process.StartInfo.FileName = "C:\\Program Files\\DeskIn\\DeskIn.exe";
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.UseShellExecute = true;
            process.Start();
        }
    }
}
