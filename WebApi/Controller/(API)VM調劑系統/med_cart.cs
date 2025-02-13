﻿using Microsoft.AspNetCore.Mvc;
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
using MyOffice;
using System.Threading;
using MySql.Data.MySqlClient;






// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DB2VM_API.Controller._API_VM調劑系統
{
    [Route("api/[controller]")]
    [ApiController]
    public class med_cart : ControllerBase
    {
        static private string API01 = "http://127.0.0.1:4433";
        static private MySqlSslMode SSLMode = MySqlSslMode.None;

        //static string DB2_schema = $"{ConfigurationManager.AppSettings["DB2_schema"]}";
        static string Message = "------------------------------";

        /// <summary>
        ///以藥局、護理站確認是否可交車
        /// </summary>
        /// <remarks>
        /// 以下為JSON範例
        /// <code>
        ///     {
        ///         "ValueAry":[藥局, 護理站]
        ///     }
        /// </code>
        /// </remarks>
        /// <param name="returnData">共用傳遞資料結構</param>
        /// <returns></returns>
        [HttpPost("handover")]
        public string handover([FromBody] returnData returnData)
        {
            MyTimerBasic myTimerBasic = new MyTimerBasic();
            try
            {
                DateTime now = DateTime.Now;
                if (now.TimeOfDay < new TimeSpan(15, 0, 0))
                {
                    returnData.Code = -200;
                    returnData.Result = "執行失敗：目前時間尚未超過下午三點。";
                    return returnData.JsonSerializationt(true);
                }
                if (returnData.ValueAry == null || returnData.ValueAry.Count != 2)
                {
                    returnData.Code = -200;
                    returnData.Result = $"returnData.ValueAry 內容應為[藥局, 護理站]";
                    return returnData.JsonSerializationt(true);
                }
                string 藥局 = returnData.ValueAry[0];
                string 護理站 = returnData.ValueAry[1];
                //List<medCpoeClass> medCpoeClasses = ExcuteCSV(藥局, 護理站);
                //Dictionary<string, List<medCpoeClass>> medCpoeDict = CoverToDictByPatID(medCpoeClasses);
                //List<medCarInfoClass> medCarInfoClasses = new List<medCarInfoClass>();
                //List<medCpoeClass> medCpoe = new List<medCpoeClass>();
                //foreach (string 病歷號 in medCpoeDict.Keys)
                //{
                //    List<medCpoeClass> medCpoes = SortDictByPatID(medCpoeDict, 病歷號);
                //    medCarInfoClass medCarInfoClass = new medCarInfoClass
                //    {
                //        GUID = Guid.NewGuid().ToString(),
                //        藥局 = medCpoes[0].藥局,
                //        更新時間 = DateTime.Now.ToDateTimeString(),
                //        護理站 = medCpoes[0].護理站,
                //        床號 = medCpoes[0].床號,
                //        病歷號 = medCpoes[0].病歷號,
                //        姓名 = medCpoes[0].姓名,
                //        入院日期 = DateTime.MinValue.ToDateTimeString_6()
                //    };
                //    medCarInfoClasses.Add(medCarInfoClass);
                //    foreach (var item in medCpoes)
                //    {
                //        item.Master_GUID = medCarInfoClass.GUID;
                //    }
                //    medCpoe.AddRange(medCpoes);
                //}
                //List<medCarInfoClass> out_medCarInfoClass = medCarInfoClass.update_med_carinfo(API01, medCarInfoClasses);
                //List<medCpoeClass> out_medCpoe = medCpoeClass.update_med_cpoe(API01, medCpoe);

                HIS_DB_Lib.returnData returnData_handover = medCpoeClass.handover(API01, returnData.ValueAry);

                returnData_handover.Code = 200;
                returnData_handover.TimeTaken = $"{myTimerBasic}";
                return returnData_handover.JsonSerializationt(true);
            }
            catch (Exception ex)
            {
                returnData.Code = -200;
                returnData.Result = $"Exception:{ex.Message}";
                return returnData.JsonSerializationt(true);
            }
        }
        [HttpGet("get_all")]
        public string get_all()
        {
            returnData returnData = new returnData();
            MyTimerBasic myTimerBasic = new MyTimerBasic();
            try
            {
                List<medCpoeClass> medCpoeClasses = ExcuteCSV();
                Dictionary<string, List<medCpoeClass>> dicMedCpoe_cart= ToDictByCart(medCpoeClasses);
                List<medCpoeClass> update_medCpoeClass = new List<medCpoeClass>();
                //string 護理站 = "";
                int 病床清單 = 0;
                foreach( string cart in dicMedCpoe_cart.Keys)
                {
                    List<medCarInfoClass> medCarInfoClasses = new List<medCarInfoClass>();
                    List<medCpoeClass> medCpoes = GetDictByCart(dicMedCpoe_cart, cart);
                    Dictionary<string, List<medCpoeClass>> medCpoeDict = ToDictByPatID(medCpoes);
                    List<medCpoeClass> add_medCpoeClass = new List<medCpoeClass>();
                    foreach (string 病歷號 in medCpoeDict.Keys)
                    {
                        List<medCpoeClass> medCpoe = GetDictByPatID(medCpoeDict, 病歷號);
                        medCarInfoClass medCarInfoClass = new medCarInfoClass
                        {
                            藥局 = medCpoe[0].藥局,
                            更新時間 = DateTime.Now.ToDateTimeString(),
                            護理站 = medCpoe[0].護理站,
                            床號 = medCpoe[0].床號,
                            病歷號 = medCpoe[0].病歷號,
                            姓名 = medCpoe[0].姓名,
                            入院日期 = DateTime.MinValue.ToDateTimeString_6(),
                            占床狀態 = "已佔床"

                        };
                    
                        medCarInfoClasses.Add(medCarInfoClass);
                    }

                    //List<medCarInfoClass> out_medCarInfoClass = medCarInfoClass.update_med_carinfo(API01, medCarInfoClasses);

                    #region
                    List<medCarInfoClass> medCart_sql_add = new List<medCarInfoClass>();
                    List<medCarInfoClass> medCart_sql_replace = new List<medCarInfoClass>();
                    List<medCarInfoClass> medCart_sql_delete = new List<medCarInfoClass>();


                    SQLControl sQLControl_med_carInfo = new SQLControl("127.0.0.1", "dbvm", "med_carInfo", "user", "66437068", 3306, SSLMode);
                    SQLControl sQLControl_med_cpoe = new SQLControl("127.0.0.1", "dbvm", "med_cpoe", "user", "66437068", 3306, SSLMode);

                    DateTime lestweek = DateTime.Now.AddDays(-30);
                    DateTime yesterday = DateTime.Now.AddDays(-0);
                    string starttime = lestweek.GetStartDate().ToDateString();
                    string endtime = yesterday.GetEndDate().ToDateString();
                    sQLControl_med_carInfo.DeleteByBetween(null, (int)enum_med_carInfo.更新時間, starttime, endtime);

                    List<medCarInfoClass> input_medCarInfo = medCarInfoClasses;

                    if (input_medCarInfo == null)
                    {
                        returnData.Code = -200;
                        returnData.Result = $"傳入Data資料異常";
                        return returnData.JsonSerializationt();
                    }
                    string 藥局 = input_medCarInfo[0].藥局;
                    string 護理站 = input_medCarInfo[0].護理站;

                    List<object[]> list_med_carInfo = sQLControl_med_carInfo.GetRowsByDefult(null, (int)enum_med_carInfo.藥局, 藥局);
                    List<medCarInfoClass> sql_medCar = list_med_carInfo.SQLToClass<medCarInfoClass, enum_med_carInfo>();
                    List<medCarInfoClass> medCarInfo = sql_medCar.Where(temp => temp.護理站 == 護理站).ToList();
                    Dictionary<string, List<medCarInfoClass>> medCarInfoDictBedNum = medCarInfoClass.ToDictByBedNum(medCarInfo);

                   
                    List<Task> tasks = new List<Task>();

                    foreach (medCarInfoClass medCarInfoClass in input_medCarInfo)
                    {
                        tasks.Add(Task.Run(new Action(delegate
                        {
                            medCarInfoClass targetPatient = new medCarInfoClass();

                            string 床號 = medCarInfoClass.床號;
                            if (medCarInfoClass.GetDictByBedNum(medCarInfoDictBedNum, 床號).Count != 0)
                            {
                                targetPatient = medCarInfoClass.GetDictByBedNum(medCarInfoDictBedNum, 床號)[0];
                            }
                            if (targetPatient.GUID.StringIsEmpty() == true)
                            {
                                medCarInfoClass.GUID = Guid.NewGuid().ToString();
                                medCart_sql_add.LockAdd(medCarInfoClass);
                            }
                            else
                            {
                                if (medCarInfoClass.病歷號 != targetPatient.病歷號)
                                {
                                    medCarInfoClass.GUID = Guid.NewGuid().ToString();
                                    medCarInfoClass.異動 = "Y";
                                    medCart_sql_add.LockAdd(medCarInfoClass);
                                    medCart_sql_delete.LockAdd(targetPatient);
                                }
                                else
                                {
                                    medCarInfoClass.GUID = targetPatient.GUID;
                                    medCarInfoClass.調劑狀態 = targetPatient.調劑狀態;
                                    medCart_sql_replace.LockAdd(medCarInfoClass);
                                }
                            }
                        })));
                    }
                    Task.WhenAll(tasks).Wait();




                    List<object[]> list_medCart_add = new List<object[]>();
                    List<object[]> list_medCart_replace = new List<object[]>();
                    List<object[]> list_medCart_delete = new List<object[]>();
                    list_medCart_add = medCart_sql_add.ClassToSQL<medCarInfoClass, enum_med_carInfo>();
                    list_medCart_replace = medCart_sql_replace.ClassToSQL<medCarInfoClass, enum_med_carInfo>();
                    list_medCart_delete = medCart_sql_delete.ClassToSQL<medCarInfoClass, enum_med_carInfo>();

                    if (list_medCart_add.Count > 0) sQLControl_med_carInfo.AddRows(null, list_medCart_add);
                    if (list_medCart_replace.Count > 0) sQLControl_med_carInfo.UpdateByDefulteExtra(null, list_medCart_replace);
                    if (list_medCart_delete.Count > 0)
                    {
                        sQLControl_med_carInfo.DeleteExtra(null, list_medCart_delete);
                        List<object[]> med_cpoe = sQLControl_med_cpoe.GetRowsByDefult(null, (int)enum_med_cpoe.藥局, 藥局);
                        List<medCpoeClass> medCpoe = med_cpoe.SQLToClass<medCpoeClass, enum_med_cpoe>();
                        List<medCpoeClass> filterCpoe = medCpoe
                            .Where(cpoe => medCart_sql_delete.Any(medCart => medCart.GUID == cpoe.Master_GUID)).ToList();
                        List<object[]> medCpoe_delete_list = filterCpoe.ClassToSQL<medCpoeClass, enum_med_cpoe>();
                        if (medCpoe_delete_list.Count > 0) sQLControl_med_cpoe.DeleteExtra(null, medCpoe_delete_list);
                    }

                    List<object[]> list_bedList = sQLControl_med_carInfo.GetRowsByDefult(null, (int)enum_med_carInfo.藥局, 藥局);
                    List<medCarInfoClass> bedList = list_bedList.SQLToClass<medCarInfoClass, enum_med_carInfo>();
                    medCarInfoClasses = bedList.Where(temp => temp.護理站 == 護理站).ToList();
                    medCarInfoClasses.Sort(new medCarInfoClass.ICP_By_bedNum());
                    #endregion
                    ///更新 med_cpoe 開始
                    for (int j = 0; j < medCarInfoClasses.Count; j++)
                    {
                        string ptID = medCarInfoClasses[j].病歷號;
                        string GUID = medCarInfoClasses[j].GUID;
                        List<medCpoeClass> medCpoe = GetDictByPatID(medCpoeDict, ptID);
                        for(int k = 0; k < medCpoe.Count; k++)
                        {
                            medCpoe[k].Master_GUID = GUID;
                        }
                        add_medCpoeClass.AddRange(medCpoe);
                    }
                    //medCpoeClass.update_med_cpoe(API01, add_medCpoeClass);
                    #region
                   
                    sQLControl_med_cpoe.DeleteByBetween(null, (int)enum_med_cpoe.更新時間, starttime, endtime);

                    List<medCpoeClass> input_medCpoe = add_medCpoeClass;
                  

                    藥局 = input_medCpoe[0].藥局;
                    護理站 = input_medCpoe[0].護理站;

                    list_med_carInfo = sQLControl_med_carInfo.GetRowsByDefult(null, (int)enum_med_carInfo.藥局, 藥局);
                    List<object[]> list_med_cpoe = sQLControl_med_cpoe.GetRowsByDefult(null, (int)enum_med_cpoe.藥局, 藥局);

                    List<medCarInfoClass> sql_medCarInfo = list_med_carInfo.SQLToClass<medCarInfoClass, enum_med_carInfo>();
                    List<medCpoeClass> sql_medCpoe = list_med_cpoe.SQLToClass<medCpoeClass, enum_med_cpoe>();

                    List<medCpoeClass> medCpoe_sql_add = new List<medCpoeClass>();
                    List<medCpoeClass> medCpoe_sql_replace = new List<medCpoeClass>();
                    List<medCpoeClass> medCpoe_sql_delete_buf = new List<medCpoeClass>();
                    List<medCpoeClass> medCpoe_sql_delete = new List<medCpoeClass>();
                    List<medCarInfoClass> update_medCarInfo = new List<medCarInfoClass>();

                    Dictionary<string, List<medCarInfoClass>> medCarInfoDict = medCarInfoClass.CoverToDictByGUID(sql_medCarInfo);
                    Dictionary<string, List<medCpoeClass>> sqlMedCpoeDict = medCpoeClass.CoverToDictByMasterGUID(sql_medCpoe);
                    Dictionary<string, List<medCpoeClass>> inputMedCpoeDict = medCpoeClass.CoverToDictByMasterGUID(input_medCpoe);


                    foreach (string Master_GUID in inputMedCpoeDict.Keys)
                    {
                        if (!medCarInfoDict.ContainsKey(Master_GUID))
                        {
                            returnData.Code = -200;
                            returnData.Result = "處方資料錯誤，請更新病床資訊";
                            return returnData.JsonSerializationt(true);
                        }
                        else
                        {
                            List<medCpoeClass> medCpoeClasses_current = medCpoeClass.SortDictByMasterGUID(sqlMedCpoeDict, Master_GUID);
                            List<medCpoeClass> medCpoeClasses_new = medCpoeClass.SortDictByMasterGUID(inputMedCpoeDict, Master_GUID);
                            if (medCpoeClasses_current.Count == 0 && medCpoeClasses_new.Count == 0)
                            {
                                medCarInfoClasses = medCarInfoClass.SortDictByGUID(medCarInfoDict, Master_GUID);
                                if (medCarInfoClasses[0].占床狀態 == "已佔床")
                                {
                                    medCarInfoClasses[0].調劑狀態 = "Y";
                                    update_medCarInfo.Add(medCarInfoClasses[0]);
                                }
                            }
                            else
                            {
                                List<medCpoeClass> onlyInOld = medCpoeClasses_current.Where(o => !medCpoeClasses_new.Any(n => n.序號 == o.序號)).ToList();
                                List<medCpoeClass> onlyInNew = medCpoeClasses_new.Where(o => !medCpoeClasses_current.Any(n => n.序號 == o.序號)).ToList();
                                foreach (var oldItem in onlyInOld)
                                {

                                    if (oldItem.調劑狀態.StringIsEmpty() && oldItem.調劑異動.StringIsEmpty())
                                    {
                                        medCpoe_sql_delete.Add(oldItem);
                                    }
                                    else
                                    {
                                        foreach (var newItem in onlyInNew)
                                        {
                                            if (oldItem.藥碼 == newItem.藥碼 &&
                                                oldItem.途徑 == newItem.途徑 &&
                                                oldItem.頻次代碼 == newItem.頻次代碼)
                                            {
                                                medCpoe_sql_delete.Add(oldItem);
                                                oldItem.調劑異動 = "Y";
                                                newItem.調劑狀態 = "Y";
                                            }
                                        }
                                        if (oldItem.調劑異動.StringIsEmpty())
                                        {
                                            oldItem.數量 = "0";
                                            oldItem.劑量 = "0";
                                            oldItem.調劑狀態 = "";
                                            oldItem.頻次代碼 = "--";
                                            oldItem.狀態 = "DC";
                                            oldItem.調劑異動 = "Y";
                                            medCpoe_sql_replace.Add(oldItem);
                                        }
                                    }
                                }
                                medCpoe_sql_add.AddRange(onlyInNew);
                            }
                        }
                    }


                    List<object[]> list_medCpoe_add = medCpoe_sql_add.ClassToSQL<medCpoeClass, enum_med_cpoe>();
                    List<object[]> list_medCpoe_replace = medCpoe_sql_replace.ClassToSQL<medCpoeClass, enum_med_cpoe>();
                    List<object[]> list_medCpoe_delete = medCpoe_sql_delete.ClassToSQL<medCpoeClass, enum_med_cpoe>();
                    list_medCart_add = update_medCarInfo.ClassToSQL<medCarInfoClass, enum_med_carInfo>();


                    if (list_medCpoe_add.Count > 0) sQLControl_med_cpoe.AddRows(null, list_medCpoe_add);
                    if (list_medCpoe_replace.Count > 0) sQLControl_med_cpoe.UpdateByDefulteExtra(null, list_medCpoe_replace);
                    if (list_medCpoe_delete.Count > 0) sQLControl_med_cpoe.DeleteExtra(null, list_medCpoe_delete);
                    if (list_medCart_add.Count > 0) sQLControl_med_carInfo.UpdateByDefulteExtra(null, list_medCart_add);
                    #endregion
                }

                returnData.Code = 200;
                returnData.TimeTaken = $"{myTimerBasic}";
                returnData.Result = $"取得病床資訊共{病床清單}筆";
                return returnData.JsonSerializationt(true);
            }
            catch (Exception ex)
            {
                returnData.Code = -200;
                returnData.Result = $"Exception:{ex.Message}";
                Logger.Log("get_all", $"{returnData.JsonSerializationt(true)}");
                return returnData.JsonSerializationt(true);
            }
        }

        private List<medCpoeClass> ExcuteCSV()
        {
            //string folderPath = @"C:\Users\Administrator\Desktop\UD";
            string folderPath = @"C:\Users\user\Desktop\UD";
            //string folderPath = @"Z:\";
            string searchPattern = "*.csv";
            string NewFile = null;
            List<string> todayFiles = new List<string>();

            string[] files = Directory.GetFiles(folderPath, searchPattern, SearchOption.AllDirectories);
            if (files.Length > 0)
            {
                NewFile = files.OrderBy(f => Path.GetFileName(f)).Last();
            }
            string filePath = Path.Combine(folderPath, NewFile);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            List<object[]> list_UD = new List<object[]>();
            using (StreamReader sr = new StreamReader(filePath, Encoding.GetEncoding("Big5")))
            {
                sr.ReadLine();
                while (!sr.EndOfStream)
                {
                    string row = sr.ReadLine();
                    string[] values = row.Split(",");
                    for (int i = 0; i < values.Length; i++)
                    {
                        values[i] = values[i].Trim('"');
                    }
                    list_UD.Add(values);
                }
            }

            List<medCpoeClass> medCpoeClasses = new List<medCpoeClass>();
            for(int i = 0; i < list_UD.Count(); i++)
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
                    頻次代碼 = list_UD[i][(int)enum_UDCSV.服法].ObjectToString(),
                    藥品名 = list_UD[i][(int)enum_UDCSV.藥名].ObjectToString(),
                    途徑 = list_UD[i][(int)enum_UDCSV.途徑].ObjectToString(),
                    數量 = list_UD[i][(int)enum_UDCSV.每日量].ObjectToString(),
                    單位 = list_UD[i][(int)enum_UDCSV.單位].ObjectToString(),
                    藥碼 = list_UD[i][(int)enum_UDCSV.TSTR].ObjectToString(),
                    劑量 = list_UD[i][(int)enum_UDCSV.每次量].ObjectToString(),
                };
                medCpoeClass.病歷號 = $"{medCpoeClass.護理站}-{medCpoeClass.床號}-{medCpoeClass.姓名}";
                medCpoeClass.序號 = $"{medCpoeClass.藥碼}-{medCpoeClass.途徑}-{medCpoeClass.數量}";
                medCpoeClasses.Add(medCpoeClass);
            }
            return medCpoeClasses;

        }

        private string age(string birthday)
        {
            int birthYear = birthday.Substring(0, 4).StringToInt32();
            int birthMon = birthday.Substring(4, 2).StringToInt32();
            int birthDay = birthday.Substring(6, 2).StringToInt32();

            DateTime today = DateTime.Now;
            int todayYear = today.Year;
            int todayMon = today.Month;
            int todayDay = today.Day;

            int ageYears = todayYear - birthYear;
            int ageMonths = todayMon - birthMon;

            if (ageMonths < 0 || (ageMonths == 0 && todayDay < birthDay))
            {
                ageYears--;
                ageMonths += 12;
            }

            if (todayDay < birthDay)
            {
                ageMonths--;
                if (ageMonths < 0)
                {
                    ageYears--;
                    ageMonths += 12;
                }
            }
            string ages = $"{ageYears}歲{ageMonths}月";

            return ages;
        }
        private (string dieaseCode, string dieaseName) disease(diseaseClass diseaseClass)
        {
            string dieaseCode = "";
            string dieaseName = "";

            if (!string.IsNullOrWhiteSpace(diseaseClass.國際疾病分類代碼1)) dieaseCode += diseaseClass.國際疾病分類代碼1;
            if (!string.IsNullOrWhiteSpace(diseaseClass.國際疾病分類代碼2)) dieaseCode += $";{diseaseClass.國際疾病分類代碼2}";
            if (!string.IsNullOrWhiteSpace(diseaseClass.國際疾病分類代碼3)) dieaseCode += $";{diseaseClass.國際疾病分類代碼3}";
            if (!string.IsNullOrWhiteSpace(diseaseClass.國際疾病分類代碼4)) dieaseCode += $";{diseaseClass.國際疾病分類代碼4}";
            if (!string.IsNullOrWhiteSpace(diseaseClass.疾病說明1)) dieaseName += diseaseClass.疾病說明1;
            if (!string.IsNullOrWhiteSpace(diseaseClass.疾病說明2)) dieaseName += $";{diseaseClass.疾病說明2}";
            if (!string.IsNullOrWhiteSpace(diseaseClass.疾病說明3)) dieaseName += $";{diseaseClass.疾病說明3}";
            if (!string.IsNullOrWhiteSpace(diseaseClass.疾病說明4)) dieaseName += $";{diseaseClass.疾病說明4}";
            return (dieaseCode, dieaseName);
        }


        private Dictionary<string, List<medCpoeClass>> ToDictByCart(List<medCpoeClass> medCpoeClasses)
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
        private List<medCpoeClass> GetDictByCart(Dictionary<string, List<medCpoeClass>> dict, string 護理站)
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

        private Dictionary<string, List<medCpoeClass>> ToDictByPatID(List<medCpoeClass> medCpoeClasses)
        {
            Dictionary<string, List<medCpoeClass>> dictionary = new Dictionary<string, List<medCpoeClass>>();
            foreach(var item in medCpoeClasses)
            {
                if(dictionary.TryGetValue(item.病歷號, out List<medCpoeClass> list))
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
        private List<medCpoeClass> GetDictByPatID(Dictionary<string, List<medCpoeClass>> dict, string 病歷號)
        {
            if(dict.TryGetValue(病歷號, out List<medCpoeClass> medCpoeClasses))
            {
                return medCpoeClasses;
            }
            else
            {
                return new List<medCpoeClass>();
            }
        }
        public enum enum_UDCSV
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
