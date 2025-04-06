//using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;
using Basic;
using SQLUI;
using HIS_DB_Lib;
using System.Data;
using System.Text;
using System.IO;
//using MyOffice;
using System.Threading;
//using MySql.Data.MySqlClient;
using Microsoft.VisualBasic.FileIO;
using System.Reflection;

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
                    Console.WriteLine("程式已經在運行中...");
                    Console.ReadKey();
                    return;
                }
                while (true)
                {
                    List<medCpoeClass> medCpoeClasses = ExcuteCSV();
                    Dictionary<string, List<medCpoeClass>> dicMedCpoe_cart = ToDictByCart(medCpoeClasses);
                    List<medCpoeClass> update_medCpoeClass = new List<medCpoeClass>();
                    //string 護理站 = "";

                    foreach (string cart in dicMedCpoe_cart.Keys)
                    {
                        List<patientInfoClass> patientInfoClasses = new List<patientInfoClass>();
                        List<medCpoeClass> medCpoes = GetDictByCart(dicMedCpoe_cart, cart);
                        Dictionary<string, List<medCpoeClass>> medCpoeDict = ToDictByPatID(medCpoes);
                        foreach (string 病歷號 in medCpoeDict.Keys)
                        {
                            List<medCpoeClass> medCpoe = GetDictByPatID(medCpoeDict, 病歷號);
                            patientInfoClass patientInfoClass = new patientInfoClass
                            {
                                GUID = Guid.NewGuid().ToString(),
                                藥局 = medCpoe[0].藥局,
                                PRI_KEY = medCpoe[0].病歷號,
                                更新時間 = DateTime.Now.ToDateTimeString(),
                                護理站 = medCpoe[0].護理站,
                                床號 = medCpoe[0].床號,
                                //病歷號 = medCpoe[0].病歷號,
                                姓名 = medCpoe[0].姓名,
                                入院日期 = DateTime.MinValue.ToDateTimeString_6(),
                                調劑時間 = DateTime.MinValue.ToDateTimeString_6(),
                                占床狀態 = "已佔床"

                            };

                            patientInfoClasses.Add(patientInfoClass);

                        }

                        returnData returnData_patientInfoClass = patientInfoClass.update_patientInfo(API01, patientInfoClasses);
                        if (returnData_patientInfoClass.Code != 200)
                        {
                            Console.WriteLine( returnData_patientInfoClass.JsonSerializationt(true));
                        }
                        patientInfoClasses = returnData_patientInfoClass.Data.ObjToClass<List<patientInfoClass>>();

                        List<medCpoeClass> add_medCpoeClass = new List<medCpoeClass>();

                        ///更新 med_cpoe 開始
                        for (int j = 0; j < patientInfoClasses.Count; j++)
                        {
                            string PRI_KEY = patientInfoClasses[j].PRI_KEY;
                            string GUID = patientInfoClasses[j].GUID;
                            List<medCpoeClass> medCpoe = GetDictByPatID(medCpoeDict, PRI_KEY);
                            for (int k = 0; k < medCpoe.Count; k++)
                            {
                                medCpoe[k].Master_GUID = GUID;
                            }
                            add_medCpoeClass.AddRange(medCpoe);
                        }
                        returnData returnData_med_cpoe = medCpoeClass.update_med_cpoe(API01, add_medCpoeClass);
                        if (returnData_med_cpoe.Code != 200)
                        {
                            Console.WriteLine(returnData_med_cpoe.JsonSerializationt(true));
                        }
                        //medCpoeClass.update_med_cpoe(API01, add_medCpoeClass);

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
        private static List<medCpoeClass> ExcuteCSV()
        {
            //Logger.Log("filePath", $"ExcuteCSV");

            string folderPath = @"C:\UD";
            //string folderPath = logDirectory;

            string searchPattern = "*.csv";
            string NewFile = null;
            List<string> todayFiles = new List<string>();

            string[] files = Directory.GetFiles(folderPath, searchPattern, System.IO.SearchOption.AllDirectories);
            if (files.Length > 0)
            {
                NewFile = files.OrderBy(f => Path.GetFileName(f)).Last();
            }
            string filePath = Path.Combine(folderPath, NewFile);
            //Logger.Log("filePath", $"{filePath}");
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            List<object[]> list_UD = new List<object[]>();
            
            using (TextFieldParser parser = new TextFieldParser(filePath, System.Text.Encoding.GetEncoding("Big5")))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                parser.ReadLine(); // 跳過標題行

                while (!parser.EndOfData)
                {
                    string[] values = parser.ReadFields(); // 自動解析 CSV，處理雙引號
                    list_UD.Add(values);
                }
            }

            List<medCpoeClass> medCpoeClasses = new List<medCpoeClass>();
            for (int i = 0; i < list_UD.Count(); i++)
            {
                string 護理站 = list_UD[i][(int)enum_UDCSV.站].ObjectToString();
                medCpoeClass medCpoeClass = new medCpoeClass
                {
                    GUID = Guid.NewGuid().ToString(),
                    藥局 = "UD",
                    護理站 = list_UD[i][(int)enum_UDCSV.站].ObjectToString(),
                    床號 = list_UD[i][(int)enum_UDCSV.床號].ObjectToString(),
                    //病歷號 = $"{護理站}-{床號}-{姓名}",
                    姓名 = list_UD[i][(int)enum_UDCSV.姓名].ObjectToString(),
                    更新時間 = DateTime.Now.ToDateTimeString(),
                    //序號 = $"{藥品名}-{途徑}-{數量}",
                    開始時間 = DateTime.MinValue.ToDateTimeString_6(),
                    結束時間 = DateTime.MinValue.ToDateTimeString_6(),
                    頻次 = list_UD[i][(int)enum_UDCSV.服法].ObjectToString(),
                    藥品名 = list_UD[i][(int)enum_UDCSV.藥名].ObjectToString(),
                    途徑 = list_UD[i][(int)enum_UDCSV.途徑].ObjectToString(),
                    數量 = list_UD[i][(int)enum_UDCSV.每日量].ObjectToString(),
                    單位 = list_UD[i][(int)enum_UDCSV.單位].ObjectToString(),
                    藥碼 = list_UD[i][(int)enum_UDCSV.TSTR].ObjectToString(),
                    劑量 = list_UD[i][(int)enum_UDCSV.每次量].ObjectToString(),
                    儲位 = list_UD[i][(int)enum_UDCSV.儲位].ObjectToString(),

                };
                medCpoeClass.病歷號 = $"{medCpoeClass.護理站}-{medCpoeClass.床號}-{medCpoeClass.姓名}";
                medCpoeClass.PRI_KEY = $"{medCpoeClass.藥碼}-{medCpoeClass.途徑}-{medCpoeClass.數量}";
                if (medCpoeClass.藥碼.StartsWith("I"))
                {
                    medCpoeClass.針劑 = "Y";
                }
                if (medCpoeClass.藥碼.StartsWith("O"))
                {
                    medCpoeClass.口服 = "Y";
                }
                medCpoeClasses.Add(medCpoeClass);
            }
            return medCpoeClasses;

        }
        private static Dictionary<string, List<medCpoeClass>> ToDictByCart(List<medCpoeClass> medCpoeClasses)
        {
            Dictionary<string, List<medCpoeClass>> dictionary = new Dictionary<string, List<medCpoeClass>>();
            foreach (var item in medCpoeClasses)
            {
                if (dictionary.TryGetValue(item.護理站, out List<medCpoeClass> list))
                {
                    list.Add(item);
                }
                else
                {
                    dictionary[item.護理站] = new List<medCpoeClass> { item };
                }
            }
            return dictionary;
        }
        private static List<medCpoeClass> GetDictByCart(Dictionary<string, List<medCpoeClass>> dict, string 護理站)
        {
            if (dict.TryGetValue(護理站, out List<medCpoeClass> medCpoeClasses))
            {
                return medCpoeClasses;
            }
            else
            {
                return new List<medCpoeClass>();
            }
        }

        private static Dictionary<string, List<medCpoeClass>> ToDictByPatID(List<medCpoeClass> medCpoeClasses)
        {
            Dictionary<string, List<medCpoeClass>> dictionary = new Dictionary<string, List<medCpoeClass>>();
            foreach (var item in medCpoeClasses)
            {
                if (dictionary.TryGetValue(item.病歷號, out List<medCpoeClass> list))
                {
                    list.Add(item);
                }
                else
                {
                    dictionary[item.病歷號] = new List<medCpoeClass> { item };
                }
            }
            return dictionary;
        }
        private static List<medCpoeClass> GetDictByPatID(Dictionary<string, List<medCpoeClass>> dict, string 序號)
        {
            if (dict.TryGetValue(序號, out List<medCpoeClass> medCpoeClasses))
            {
                return medCpoeClasses;
            }
            else
            {
                return new List<medCpoeClass>();
            }
        }
        private static string API01 = "http://127.0.0.1:4433";

        private enum enum_UDCSV
        {
            站,
            床號,
            姓名,
            囑型,
            藥名,
            途徑,
            服法,
            每次量,
            每日量,
            單位,
            備註,
            儲位,
            TSTR,
            NOW_DOC_NO1,
            RANK,
            ANESTAETICE,
            INT_DAYS,
            INT_TIMES
        }


    }
}
