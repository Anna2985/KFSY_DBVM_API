using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;


namespace ConsoleApp_copyFile
{
    class Program
    {
        static void Main(string[] args)
        {
            bool isNewInstance;
            Mutex mutex = new Mutex(true, "ConsoleApp_copyFile", out isNewInstance);
            try
            {
                if (!isNewInstance)
                {
                    Console.ReadKey();
                    Console.WriteLine("程式已經在運行中...");
                    return;
                }
                while (true)
                {
                    string folderPath = @"Z:\";
                    string searchPattern = "*.csv";
                    string NewFile = null;
                    List<string> todayFiles = new List<string>();

                    string[] files = Directory.GetFiles(folderPath, searchPattern, SearchOption.AllDirectories);
                    if (files.Length > 0)
                    {
                        NewFile = files.OrderBy(f => Path.GetFileName(f)).Last();
                    }
                    string filePath = Path.Combine(folderPath, NewFile);

                    // 獲取桌面路徑
                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    // 建立目標檔案路徑
                    desktopPath = Path.Combine(desktopPath, "UD");

                    // 刪除15分鐘以前的檔案
                    string[] allFiles = Directory.GetFiles(desktopPath, searchPattern, SearchOption.AllDirectories);
                    foreach (string file in allFiles)
                    {
                        DateTime lastWriteTime = File.GetLastWriteTime(file);
                        if ((DateTime.Now - lastWriteTime).TotalMinutes > 15)
                        {
                            File.Delete(file);
                        }
                    }
                    string destinationFilePath = Path.Combine(desktopPath, Path.GetFileName(NewFile));
                    string path = @"C:\UD";
                    destinationFilePath = Path.Combine(path, Path.GetFileName(NewFile));
                    // 複製檔案到桌面
                    File.Copy(NewFile, destinationFilePath, overwrite: true);
                    Console.WriteLine($"檔案已成功複製到桌面: {destinationFilePath}");
                    System.Threading.Thread.Sleep(100000);

                }

            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception:{ex}");
                Console.ReadKey();
            }
            
        }
    }
}
