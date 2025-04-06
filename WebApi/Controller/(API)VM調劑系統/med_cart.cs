using Microsoft.AspNetCore.Mvc;
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
using Microsoft.VisualBasic.FileIO;
using System.Reflection;






// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DB2VM_API.Controller._API_VM調劑系統
{
    [Route("api/[controller]")]
    [ApiController]
    public class med_cart : ControllerBase
    {
        static private string API01 = "http://127.0.0.1:4433";
        static private MySqlSslMode SSLMode = MySqlSslMode.None;
        public static string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static string logDirectory = $"{currentDirectory}/UD/";
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
                //List<patientInfoClass> patientInfoClasses = new List<patientInfoClass>();
                //List<medCpoeClass> medCpoe = new List<medCpoeClass>();
                //foreach (string 病歷號 in medCpoeDict.Keys)
                //{
                //    List<medCpoeClass> medCpoes = SortDictByPatID(medCpoeDict, 病歷號);
                //    patientInfoClass patientInfoClass = new patientInfoClass
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
                //    patientInfoClasses.Add(patientInfoClass);
                //    foreach (var item in medCpoes)
                //    {
                //        item.Master_GUID = patientInfoClass.GUID;
                //    }
                //    medCpoe.AddRange(medCpoes);
                //}
                //List<patientInfoClass> out_patientInfoClass = patientInfoClass.update_med_carinfo(API01, patientInfoClasses);
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

                foreach ( string cart in dicMedCpoe_cart.Keys)
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
                    if(returnData_patientInfoClass.Code != 200)
                    {
                        return returnData_patientInfoClass.JsonSerializationt(true);
                    }
                    patientInfoClasses = returnData_patientInfoClass.Data.ObjToClass<List<patientInfoClass>>();

                    List<medCpoeClass> add_medCpoeClass = new List<medCpoeClass>();

                    ///更新 med_cpoe 開始
                    for (int j = 0; j < patientInfoClasses.Count; j++)
                    {
                        string PRI_KEY = patientInfoClasses[j].PRI_KEY;
                        string GUID = patientInfoClasses[j].GUID;
                        List<medCpoeClass> medCpoe = GetDictByPatID(medCpoeDict, PRI_KEY);
                        for(int k = 0; k < medCpoe.Count; k++)
                        {
                            medCpoe[k].Master_GUID = GUID;
                        }
                        add_medCpoeClass.AddRange(medCpoe);
                    }
                    returnData returnData_med_cpoe = medCpoeClass.update_med_cpoe(API01, add_medCpoeClass);
                    if(returnData_med_cpoe.Code != 200)
                    {
                        return returnData_med_cpoe.JsonSerializationt(true);
                    }
                    //medCpoeClass.update_med_cpoe(API01, add_medCpoeClass);
                    
                }

                returnData.Code = 200;
                //returnData.Data = add_medCpoeClass;
                returnData.TimeTaken = $"{myTimerBasic}";
                returnData.Result = $"取得病床資訊結束";
                return returnData.JsonSerializationt(true);
            }
            catch (Exception ex)
            {
                returnData.Code = -200;
                returnData.Result = $"Exception:{ex.Message}";
                //Logger.Log("get_all", $"{returnData.JsonSerializationt(true)}");
                return returnData.JsonSerializationt(true);
            }
        }

        //[HttpGet("get_all_db")]
        //public string get_all_db()
        //{
        //    returnData returnData = new returnData();
        //    MyTimerBasic myTimerBasic = new MyTimerBasic();
        //    //try
        //    //{
        //    List<medCpoeClass> medCpoeClasses = ExcuteCSV();
        //    Dictionary<string, List<medCpoeClass>> dicMedCpoe_cart = ToDictByCart(medCpoeClasses);
        //    List<medCpoeClass> update_medCpoeClass = new List<medCpoeClass>();
        //    //string 護理站 = "";
        //    List<medCpoeClass> add_medCpoeClass = new List<medCpoeClass>();

        //    foreach (string cart in dicMedCpoe_cart.Keys)
        //    {
        //        List<patientInfoClass> patientInfoClasses = new List<patientInfoClass>();
        //        List<medCpoeClass> medCpoes = GetDictByCart(dicMedCpoe_cart, cart);
        //        Dictionary<string, List<medCpoeClass>> medCpoeDict = ToDictByPatID(medCpoes);
        //        foreach (string 病歷號 in medCpoeDict.Keys)
        //        {
        //            List<medCpoeClass> medCpoe = GetDictByPatID(medCpoeDict, 病歷號);
        //            patientInfoClass patientInfoClass = new patientInfoClass
        //            {
        //                GUID = Guid.NewGuid().ToString(),
        //                藥局 = medCpoe[0].藥局,
        //                PRI_KEY = medCpoe[0].病歷號,
        //                更新時間 = DateTime.Now.ToDateTimeString(),
        //                護理站 = medCpoe[0].護理站,
        //                床號 = medCpoe[0].床號,
        //                //病歷號 = medCpoe[0].病歷號,
        //                姓名 = medCpoe[0].姓名,
        //                入院日期 = DateTime.MinValue.ToDateTimeString_6(),
        //                調劑時間 = DateTime.MinValue.ToDateTimeString_6(),
        //                占床狀態 = "已佔床"

        //            };

        //            patientInfoClasses.Add(patientInfoClass);

        //        }               

        //        //update_med_carinfo
        //        List<patientInfoClass> medCart_sql_add = new List<patientInfoClass>();
        //        List<patientInfoClass> medCart_sql_replace = new List<patientInfoClass>();
        //        List<patientInfoClass> medCart_sql_delete = new List<patientInfoClass>();

        //        SQLControl sQLControl_med_carInfo = new SQLControl("127.0.0.1", "dbvm", "med_carInfo", "user", "66437068", 3306, SSLMode);
        //        SQLControl sQLControl_med_cpoe = new SQLControl("127.0.0.1", "dbvm", "med_cpoe", "user", "66437068", 3306, SSLMode);

        //        DateTime lestweek = DateTime.Now.AddDays(-30);
        //        DateTime yesterday = DateTime.Now.AddDays(-0);
        //        string starttime = lestweek.GetStartDate().ToDateString();
        //        string endtime = yesterday.GetEndDate().ToDateString();
        //        sQLControl_med_carInfo.DeleteByBetween(null, (int)enum_med_carInfo.更新時間, starttime, endtime);

        //        List<patientInfoClass> input_medCarInfo = patientInfoClasses;

        //        if (input_medCarInfo == null)
        //        {
        //            returnData.Code = -200;
        //            returnData.Result = $"傳入Data資料異常";
        //            return returnData.JsonSerializationt();
        //        }
        //        string 藥局 = input_medCarInfo[0].藥局;
        //        string 護理站 = input_medCarInfo[0].護理站;

        //        List<object[]> list_med_carInfo = sQLControl_med_carInfo.GetRowsByDefult(null, (int)enum_med_carInfo.藥局, 藥局);
        //        List<patientInfoClass> sql_medCar = list_med_carInfo.SQLToClass<patientInfoClass, enum_med_carInfo>();
        //        List<patientInfoClass> medCarInfo = sql_medCar.Where(temp => temp.護理站 == 護理站).ToList();
        //        Dictionary<string, List<patientInfoClass>> medCarInfoDictBedNum = patientInfoClass.ToDictByBedNum(medCarInfo);



        //        List<Task> tasks = new List<Task>();

        //        foreach (patientInfoClass patientInfoClass in input_medCarInfo)
        //        {
        //            tasks.Add(Task.Run(new Action(delegate
        //            {
        //                patientInfoClass targetPatient = new patientInfoClass();

        //                string 床號 = patientInfoClass.床號;
        //                if (patientInfoClass.GetDictByBedNum(medCarInfoDictBedNum, 床號).Count != 0)
        //                {
        //                    targetPatient = patientInfoClass.GetDictByBedNum(medCarInfoDictBedNum, 床號)[0];
        //                }
        //                if (targetPatient.GUID.StringIsEmpty() == true)
        //                {
        //                    patientInfoClass.GUID = Guid.NewGuid().ToString();
        //                    medCart_sql_add.LockAdd(patientInfoClass);
        //                }
        //                else
        //                {
        //                    if (patientInfoClass.PRI_KEY != targetPatient.PRI_KEY)
        //                    {
        //                        patientInfoClass.GUID = Guid.NewGuid().ToString();
        //                        patientInfoClass.異動 = "Y";
        //                        medCart_sql_add.LockAdd(patientInfoClass);
        //                        medCart_sql_delete.LockAdd(targetPatient);
        //                    }
        //                    else
        //                    {
        //                        patientInfoClass.GUID = targetPatient.GUID;
        //                        patientInfoClass.調劑狀態 = targetPatient.調劑狀態;
        //                        medCart_sql_replace.LockAdd(patientInfoClass);
        //                    }
        //                }
        //            })));
        //        }
        //        Task.WhenAll(tasks).Wait();




        //        List<object[]> list_medCart_add = new List<object[]>();
        //        List<object[]> list_medCart_replace = new List<object[]>();
        //        List<object[]> list_medCart_delete = new List<object[]>();
        //        list_medCart_add = medCart_sql_add.ClassToSQL<patientInfoClass, enum_med_carInfo>();
        //        list_medCart_replace = medCart_sql_replace.ClassToSQL<patientInfoClass, enum_med_carInfo>();
        //        list_medCart_delete = medCart_sql_delete.ClassToSQL<patientInfoClass, enum_med_carInfo>();

        //        if (list_medCart_add.Count > 0) sQLControl_med_carInfo.AddRows(null, list_medCart_add);
        //        if (list_medCart_replace.Count > 0) sQLControl_med_carInfo.UpdateByDefulteExtra(null, list_medCart_replace);
        //        if (list_medCart_delete.Count > 0)
        //        {
        //            sQLControl_med_carInfo.DeleteExtra(null, list_medCart_delete);
        //            List<object[]> list_med_cpoe_1 = sQLControl_med_cpoe.GetRowsByDefult(null, (int)enum_med_cpoe.藥局, 藥局);
        //            List<medCpoeClass> sql_medCpoe_1 = list_med_cpoe_1.SQLToClass<medCpoeClass, enum_med_cpoe>();
        //            List<medCpoeClass> filterCpoe = sql_medCpoe_1
        //                .Where(cpoe => medCart_sql_delete.Any(medCart => medCart.GUID == cpoe.Master_GUID)).ToList();
        //            List<object[]> list_medCpoe_delete_1 = filterCpoe.ClassToSQL<medCpoeClass, enum_med_cpoe>();
        //            if (list_medCpoe_delete_1.Count > 0) sQLControl_med_cpoe.DeleteExtra(null, list_medCpoe_delete_1);
        //        }

        //        List<object[]> list_bedList = sQLControl_med_carInfo.GetRowsByDefult(null, (int)enum_med_carInfo.藥局, 藥局);
        //        List<patientInfoClass>  bedList = list_bedList.SQLToClass<patientInfoClass, enum_med_carInfo>();
        //        patientInfoClasses = bedList.Where(temp => temp.護理站 == 護理站).ToList();
        //        patientInfoClasses.Sort(new patientInfoClass.ICP_By_bedNum());


        //        ///更新 med_cpoe 開始
        //        for (int j = 0; j < patientInfoClasses.Count; j++)
        //        {
        //            string PRI_KEY = patientInfoClasses[j].PRI_KEY;
        //            string GUID = patientInfoClasses[j].GUID;
        //            List<medCpoeClass> medCpoe = GetDictByPatID(medCpoeDict, PRI_KEY);
        //            for (int k = 0; k < medCpoe.Count; k++)
        //            {
        //                medCpoe[k].Master_GUID = GUID;
        //            }
        //            add_medCpoeClass.AddRange(medCpoe);
        //        }

        //        //update_med_cpoe
        //        sQLControl_med_cpoe.DeleteByBetween(null, (int)enum_med_cpoe.更新時間, starttime, endtime);

        //        List<medCpoeClass> input_medCpoe = add_medCpoeClass;
        //        if (input_medCpoe == null)
        //        {
        //            returnData.Code = -200;
        //            returnData.Result = $"傳入Data資料異常";
        //            return returnData.JsonSerializationt();
        //        }

        //        //string 藥局 = input_medCpoe[0].藥局;
        //        //string 護理站 = input_medCpoe[0].護理站;

        //        list_med_carInfo = sQLControl_med_carInfo.GetRowsByDefult(null, (int)enum_med_carInfo.護理站, 護理站);
        //        List<object[]> list_med_cpoe = sQLControl_med_cpoe.GetRowsByDefult(null, (int)enum_med_cpoe.護理站, 護理站);

        //        List<patientInfoClass> sql_medCarInfo = list_med_carInfo.SQLToClass<patientInfoClass, enum_med_carInfo>();
        //        List<medCpoeClass> sql_medCpoe = list_med_cpoe.SQLToClass<medCpoeClass, enum_med_cpoe>();

        //        List<medCpoeClass> medCpoe_sql_add = new List<medCpoeClass>();
        //        List<medCpoeClass> medCpoe_sql_replace = new List<medCpoeClass>();
        //        List<medCpoeClass> medCpoe_sql_delete_buf = new List<medCpoeClass>();
        //        List<medCpoeClass> medCpoe_sql_delete = new List<medCpoeClass>();
        //        List<patientInfoClass> update_medCarInfo = new List<patientInfoClass>();

        //        Dictionary<string, List<patientInfoClass>> medCarInfoDict = patientInfoClass.ToDictByGUID(sql_medCarInfo);
        //        Dictionary<string, List<medCpoeClass>> sqlMedCpoeDict = medCpoeClass.ToDictByMasterGUID(sql_medCpoe);
        //        Dictionary<string, List<medCpoeClass>> inputMedCpoeDict = medCpoeClass.ToDictByMasterGUID(input_medCpoe);

        //        foreach (string GUID in medCarInfoDict.Keys)
        //        {
        //            List<medCpoeClass> medCpoeClasses_old = medCpoeClass.GetByMasterGUID(sqlMedCpoeDict, GUID);
        //            List<medCpoeClass> medCpoeClasses_new = medCpoeClass.GetByMasterGUID(inputMedCpoeDict, GUID);
        //            patientInfoClasses = patientInfoClass.GetDictByGUID(medCarInfoDict, GUID);

        //            if (medCpoeClasses_old.Count == 0 && medCpoeClasses_new.Count == 0)
        //            {
        //                patientInfoClasses[0].調劑狀態 = "已調劑";
        //                continue;
        //            }
        //            List<medCpoeClass> onlyInOld = medCpoeClasses_old.Where(oldItem => !medCpoeClasses_new.Any(newItem => newItem.PRI_KEY == oldItem.PRI_KEY)).ToList(); //DC
        //            List<medCpoeClass> onlyInNew = medCpoeClasses_new.Where(newItem => !medCpoeClasses_old.Any(oldItem => oldItem.PRI_KEY == newItem.PRI_KEY)).ToList(); //NEW
        //            for (int k = 0; k < onlyInOld.Count; k++)
        //            {
        //                if (onlyInOld[k].調劑狀態.StringIsEmpty() && onlyInOld[k].狀態.StringIsEmpty())
        //                {
        //                    onlyInOld[k].調劑異動 = "Y";
        //                    medCpoe_sql_delete.Add(onlyInOld[k]);
        //                }
        //                else
        //                {
        //                    //找出onlyInOld有沒有和onlyInNew一樣的
        //                    for (int j = 0; j < onlyInNew.Count; j++)
        //                    {
        //                        if (onlyInOld[k].藥碼 == onlyInNew[j].藥碼 &&
        //                            onlyInOld[k].途徑 == onlyInNew[j].途徑 &&
        //                            onlyInOld[k].頻次代碼 == onlyInNew[j].頻次代碼)
        //                        {
        //                            medCpoe_sql_delete.Add(onlyInOld[k]);
        //                            onlyInNew[j].調劑狀態 = onlyInOld[k].調劑狀態;
        //                            onlyInOld[k].調劑異動 = "Y";
        //                            break;
        //                        }
        //                    }
        //                }
        //            }
        //            foreach (var oldItem in onlyInOld.Where(o => o.調劑異動.StringIsEmpty()))
        //            {
        //                double 數量 = oldItem.數量.StringToInt32() * -1;
        //                oldItem.數量 = 數量.ToString();
        //                oldItem.劑量 = "--";
        //                oldItem.頻次代碼 = "--";
        //                oldItem.途徑 = "--";
        //                oldItem.單位 = "--";
        //                oldItem.調劑狀態 = "";
        //                oldItem.狀態 = "DC";
        //                oldItem.調劑異動 = "Y";
        //                medCpoe_sql_replace.Add(oldItem);
        //            }
        //            DateTime 調劑時間 = patientInfoClasses[0].調劑時間.StringToDateTime();
        //            DateTime 現在時間 = DateTime.Now;
        //            if (調劑時間 != DateTime.MaxValue && 現在時間 > 調劑時間)
        //            {
        //                foreach (var item in onlyInNew)
        //                {
        //                    item.狀態 = "New";
        //                    item.調劑異動 = "Y";
        //                }
        //            }
        //            medCpoe_sql_add.AddRange(onlyInNew);
        //        }

        //        List<object[]> list_medCpoe_add = medCpoe_sql_add.ClassToSQL<medCpoeClass, enum_med_cpoe>();
        //        List<object[]> list_medCpoe_replace = medCpoe_sql_replace.ClassToSQL<medCpoeClass, enum_med_cpoe>();
        //        List<object[]> list_medCpoe_delete = medCpoe_sql_delete.ClassToSQL<medCpoeClass, enum_med_cpoe>();
        //        list_medCart_add = update_medCarInfo.ClassToSQL<patientInfoClass, enum_patient_info>();


        //        if (list_medCpoe_add.Count > 0) sQLControl_med_cpoe.AddRows(null, list_medCpoe_add);
        //        if (list_medCpoe_replace.Count > 0) sQLControl_med_cpoe.UpdateByDefulteExtra(null, list_medCpoe_replace);
        //        if (list_medCpoe_delete.Count > 0) sQLControl_med_cpoe.DeleteExtra(null, list_medCpoe_delete);
        //        if (list_medCart_add.Count > 0) sQLControl_med_carInfo.UpdateByDefulteExtra(null, list_medCart_add);
        //    }

        //    returnData.Code = 200;
        //    returnData.Data = add_medCpoeClass;
        //    returnData.TimeTaken = $"{myTimerBasic}";
        //    returnData.Result = $"取得病床資訊";
        //    return returnData.JsonSerializationt(true);
            
        //}

        private List<medCpoeClass> ExcuteCSV()
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
            //using (StreamReader sr = new StreamReader(filePath, Encoding.GetEncoding("Big5")))
            //{
            //    sr.ReadLine();
            //    while (!sr.EndOfStream)
            //    {
            //        string row = sr.ReadLine();
            //        string[] values = row.Split(",");
            //        for (int i = 0; i < values.Length; i++)
            //        {
            //            values[i] = values[i].Trim('"');
            //        }
            //        list_UD.Add(values);
            //    }
            //}
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
        private List<medCpoeClass> GetDictByPatID(Dictionary<string, List<medCpoeClass>> dict, string 序號)
        {
            if(dict.TryGetValue(序號, out List<medCpoeClass> medCpoeClasses))
            {
                return medCpoeClasses;
            }
            else
            {
                return new List<medCpoeClass>();
            }
        }
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
