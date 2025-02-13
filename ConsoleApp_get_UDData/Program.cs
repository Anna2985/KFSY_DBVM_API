using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Basic;
using System.Threading;
using HIS_DB_Lib;

namespace ConsoleApp_get_UDData
{
    class Program
    {
        static void Main(string[] args)
        {
            bool isNewInstance;
            Mutex mutex = new Mutex(true, "ConsoleApp_get_UDData", out isNewInstance);
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
                    DateTime now = DateTime.Now;
                    //if (now.TimeOfDay > new TimeSpan(15, 0, 0)) break;
                    Console.WriteLine($"{DateTime.Now.ToString()}-取得病床資訊、處方開始");
                    string url = "http://172.20.126.83:443/api/med_cart/get_all";
                    string json = Basic.Net.WEBApiGet(url);
                    returnData returnData = json.JsonDeserializet<returnData>();
                    if(returnData.Code != 200)
                    {
                        Console.WriteLine($"{DateTime.Now.ToString()}-病床更新發生錯誤");
                        Console.WriteLine($"{returnData.Data.JsonSerializationt()}");
                        Console.WriteLine("----------------------------------------");
                    }
                    else
                    {
                        Console.WriteLine($"{DateTime.Now.ToString()}-取得病床資訊、處方結束");
                        Console.WriteLine("----------------------------------------");
                    }
                   
                    System.Threading.Thread.Sleep(100000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception:{ex}");
                Console.ReadKey();
            }
        }
    }
}
