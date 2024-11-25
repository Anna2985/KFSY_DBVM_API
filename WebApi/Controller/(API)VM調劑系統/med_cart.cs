using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;
using Basic;
using SQLUI;
using HIS_DB_Lib;
using IBM.Data.DB2.Core;
using System.Data;
using System.Text;
using System.IO;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DB2VM_API.Controller._API_VM調劑系統
{
    [Route("api/[controller]")]
    [ApiController]
    public class med_cart : ControllerBase
    {
        static private string API01 = "http://127.0.0.1:4433";
        static string DB2_schema = $"{ConfigurationManager.AppSettings["DB2_schema"]}";

        /// <summary>
        ///以藥局和護理站取得占床資料
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
        [HttpPost("get_bed_list_by_cart")]
        public string get_bed_list_by_cart([FromBody] returnData returnData)
        {
            MyTimerBasic myTimerBasic = new MyTimerBasic();
            try
            {
                if (returnData.ValueAry == null)
                {
                    returnData.Code = -200;
                    returnData.Result = $"returnData.ValueAry 無傳入資料";
                    return returnData.JsonSerializationt(true);
                }
                if (returnData.ValueAry.Count != 2)
                {
                    returnData.Code = -200;
                    returnData.Result = $"returnData.ValueAry 內容應為[藥局, 護理站]";
                    return returnData.JsonSerializationt(true);
                }

               
                string 藥局 = returnData.ValueAry[0];
                string 護理站 = returnData.ValueAry[1];
                List<medCpoeClass> medCpoeClasses = ExcuteCSV(藥局, 護理站);
                Dictionary<string, List<medCpoeClass>> medCpoeDict = CoverToDictByPatID(medCpoeClasses);
                List<medCarInfoClass> medCarInfoClasses = new List<medCarInfoClass>();
                List<medCpoeClass> medCpoe = new List<medCpoeClass>();
                foreach(string 病歷號 in medCpoeDict.Keys)
                {
                    List<medCpoeClass> medCpoes = SortDictByPatID(medCpoeDict, 病歷號);
                    medCarInfoClass medCarInfoClass = new medCarInfoClass
                    {
                        GUID = Guid.NewGuid().ToString(),
                        藥局 = medCpoes[0].藥局,
                        更新時間 = DateTime.Now.ToDateTimeString(),
                        護理站 = medCpoes[0].護理站,
                        床號 = medCpoes[0].床號,
                        病歷號 = medCpoes[0].病歷號,
                        姓名 = medCpoes[0].姓名,
                        入院日期 = DateTime.MinValue.ToDateTimeString_6()
                    };
                    medCarInfoClasses.Add(medCarInfoClass);
                    foreach(var item in medCpoes)
                    {
                        item.Master_GUID = medCarInfoClass.GUID;
                    }
                    medCpoe.AddRange(medCpoes);
                }
                List<medCarInfoClass> out_medCarInfoClass = medCarInfoClass.update_med_carinfo(API01, medCarInfoClasses);
                List<medCpoeClass> out_medCpoe = medCpoeClass.update_med_cpoe(API01, medCpoe);
                returnData.TimeTaken = $"{myTimerBasic}";
                returnData.Data = medCpoe;
                returnData.Result = $"取得住院{藥局} {護理站} 病床資訊共{out_medCarInfoClass.Count}筆";
                return returnData.JsonSerializationt(true);
            }
            catch (Exception ex)
            {
                returnData.Code = -200;
                returnData.Result = $"Exception:{ex.Message}";
                return returnData.JsonSerializationt(true);
            }
        }
        /// <summary>
        ///以GUID取得病人詳細資料
        /// </summary>
        /// <remarks>
        /// 以下為JSON範例
        /// <code>
        ///     {
        ///         "ValueAry":[GUID]
        ///     }
        /// </code>
        /// </remarks>
        /// <param name="returnData">共用傳遞資料結構</param>
        /// <returns></returns>
        [HttpPost("get_patient_by_GUID")]
        public string get_patient_by_GUID([FromBody] returnData returnData)
        {
            MyTimerBasic myTimerBasic = new MyTimerBasic();
            try
            {
                if (returnData.ValueAry == null || returnData.ValueAry.Count != 1)
                {
                    returnData.Code = -200;
                    returnData.Result = $"returnData.ValueAry 內容應為[\"GUID\"]";
                    return returnData.JsonSerializationt(true);
                }                                            
                medCarInfoClass out_medCarInfoClass = medCarInfoClass.get_patient_by_GUID(API01,returnData.Value, returnData.ValueAry);
                string 藥局 = out_medCarInfoClass.藥局;
                string 護理站 = out_medCarInfoClass.護理站;
                string 床號 = out_medCarInfoClass.床號;
                returnData.Code = 200;
                returnData.TimeTaken = $"{myTimerBasic}";
                returnData.Data = out_medCarInfoClass;
                returnData.Result = $"取得{藥局} {護理站} 第{床號}病床資訊";
                return returnData.JsonSerializationt(true);
            }
            catch (Exception ex)
            {
                returnData.Code = -200;
                returnData.Result = $"Exception:{ex.Message}";
                return returnData.JsonSerializationt(true);
            }
        }
        /// <summary>
        ///以護理站取得藥品總量
        /// </summary>
        /// <remarks>
        /// 以下為JSON範例
        /// <code>
        ///     {
        ///         "Value":"調劑台"
        ///         "ValueAry":[藥局, 護理站]
        ///     }
        /// </code>
        /// </remarks>
        /// <param name="returnData">共用傳遞資料結構</param>
        /// <returns></returns>
        [HttpPost("get_med_qty")]
        public string get_med_qty([FromBody] returnData returnData)
        {
            MyTimerBasic myTimerBasic = new MyTimerBasic();
            try
            {
                if (returnData.ValueAry == null || returnData.ValueAry.Count != 2)
                {
                    returnData.Code = -200;
                    returnData.Result = $"returnData.ValueAry 內容應為[藥局, 護理站]";
                    return returnData.JsonSerializationt(true);
                }
                if (returnData.Value == null)
                {
                    returnData.Code = -200;
                    returnData.Result = $"returnData.Value 無傳入資料";
                    return returnData.JsonSerializationt(true);
                }

                string 藥局 = returnData.ValueAry[0];
                string 護理站 = returnData.ValueAry[1];

                //List<medCarInfoClass> bedList = ExecuteUDPDPPF1(藥局, 護理站);
                //List<medCarInfoClass> bedListInfo = ExecuteUDPDPPF0(bedList);
                //medCarInfoClass.update_med_carinfo(API01, bedListInfo);
                //List<medCarInfoClass> out_medCarInfoClass = medCarInfoClass.get_bed_list_by_cart(API01, returnData.ValueAry);
                //List<medCpoeClass> bedListCpoe = ExecuteUDPDPDSP(out_medCarInfoClass);
                //medCpoeClass.update_med_cpoe(API01, bedListCpoe);

                //List<medCarInfoClass> update = new List<medCarInfoClass>();
                //foreach (var medCarInfoClass in out_medCarInfoClass)
                //{
                //    List<medCpoeClass> medCpoeClasses = bedListCpoe
                //        .Where(temp => temp.Master_GUID == medCarInfoClass.GUID)
                //        .ToList();
                //    if (medCpoeClasses.Count == 0 && medCarInfoClass.占床狀態 == "已佔床")
                //    {
                //        medCarInfoClass.調劑狀態 = "Y";
                //        update.Add(medCarInfoClass);
                //    }
                //}
                //if (update.Count != 0) medCarInfoClass.update_med_carinfo(API01, update);

                List<medQtyClass> get_med_qty = medCpoeClass.get_med_qty(API01, returnData.Value, returnData.ValueAry);
                if (get_med_qty == null)
                {
                    returnData.Code = 200;
                    returnData.Result = $"無藥品處方資料";
                    return returnData.JsonSerializationt(true);
                }
                returnData.Code = 200;
                returnData.TimeTaken = $"{myTimerBasic}";
                returnData.Data = get_med_qty;
                returnData.Result = $"{藥局} {護理站} 的藥品清單";
                return returnData.JsonSerializationt(true);
            }
            catch (Exception ex)
            {
                returnData.Code = -200;
                returnData.Result = ex.Message;
                return returnData.JsonSerializationt(true);
            }
        }
        /// <summary>
        ///以病床GUID取得處方異動資料
        /// </summary>
        /// <remarks>
        /// 以下為JSON範例
        /// <code>
        ///     {
        ///         "ValueAry":[GUID]
        ///     }
        /// </code>
        /// </remarks>
        /// <param name="returnData">共用傳遞資料結構</param>
        /// <returns></returns>
        [HttpPost("get_medChange_by_GUID")]
        public string get_medChange_by_GUID([FromBody] returnData returnData)
        {
            MyTimerBasic myTimerBasic = new MyTimerBasic();
            try
            {
                //if (returnData.ValueAry == null || returnData.ValueAry.Count != 1)
                //{
                //    returnData.Code = -200;
                //    returnData.Result = $"returnData.ValueAry 內容應為[GUID]";
                //    return returnData.JsonSerializationt(true);
                //}

                //List<medCarInfoClass> bedList = new List<medCarInfoClass> { medCarInfoClass.get_patient_by_GUID_brief(API01, returnData.ValueAry) };
                //List<medCpoeRecClass> medCpoe_change = ExecuteUDPDPORD(bedList);
                //medCpoeRecClass.update_med_CpoeRec(API01, medCpoe_change);
                //List<medCarInfoClass> get_patient = medCpoeRecClass.get_medChange_by_GUID(API01, returnData.ValueAry);

                //string 藥局 = bedList[0].藥局;
                //string 護理站 = bedList[0].護理站;
                //string 床號 = bedList[0].床號;
                List<medCarInfoClass> get_patient = new List<medCarInfoClass>();
                returnData.Code = 200;
                returnData.TimeTaken = $"{myTimerBasic}";
                returnData.Data = get_patient;
                returnData.Result = $"無資料";
                return returnData.JsonSerializationt(true);
            }
            catch (Exception ex)
            {
                returnData.Code = -200;
                returnData.Result = $"Exception:{ex.Message}";
                return returnData.JsonSerializationt(true);
            }
        }
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
                List<medCpoeClass> medCpoeClasses = ExcuteCSV(藥局, 護理站);
                Dictionary<string, List<medCpoeClass>> medCpoeDict = CoverToDictByPatID(medCpoeClasses);
                List<medCarInfoClass> medCarInfoClasses = new List<medCarInfoClass>();
                List<medCpoeClass> medCpoe = new List<medCpoeClass>();
                foreach (string 病歷號 in medCpoeDict.Keys)
                {
                    List<medCpoeClass> medCpoes = SortDictByPatID(medCpoeDict, 病歷號);
                    medCarInfoClass medCarInfoClass = new medCarInfoClass
                    {
                        GUID = Guid.NewGuid().ToString(),
                        藥局 = medCpoes[0].藥局,
                        更新時間 = DateTime.Now.ToDateTimeString(),
                        護理站 = medCpoes[0].護理站,
                        床號 = medCpoes[0].床號,
                        病歷號 = medCpoes[0].病歷號,
                        姓名 = medCpoes[0].姓名,
                        入院日期 = DateTime.MinValue.ToDateTimeString_6()
                    };
                    medCarInfoClasses.Add(medCarInfoClass);
                    foreach (var item in medCpoes)
                    {
                        item.Master_GUID = medCarInfoClass.GUID;
                    }
                    medCpoe.AddRange(medCpoes);
                }
                List<medCarInfoClass> out_medCarInfoClass = medCarInfoClass.update_med_carinfo(API01, medCarInfoClasses);
                List<medCpoeClass> out_medCpoe = medCpoeClass.update_med_cpoe(API01, medCpoe);

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
      
        [HttpPost("get_medChange_by_cart")]
        public string get_medChange_by_cart([FromBody] returnData returnData)
        {
            MyTimerBasic myTimerBasic = new MyTimerBasic();
            try
            {
                if (returnData.ValueAry == null || returnData.ValueAry.Count != 2)
                {
                    returnData.Code = -200;
                    returnData.Result = $"returnData.ValueAry 內容應為[藥局, 護理站]";
                    return returnData.JsonSerializationt(true);
                }              
                List<medCarInfoClass> bedList = medCarInfoClass.get_bed_list_by_cart(API01, returnData.ValueAry);
                //List<medCpoeRecClass> medCpoe_change = ExecuteUDPDPORD(bedList);
                //List<medCpoeRecClass> update_medCpoe_change = medCpoeRecClass.update_med_CpoeRec(API01, medCpoe_change);

                string 藥局 = returnData.ValueAry[0];
                string 護理站 = returnData.ValueAry[1];

                returnData.Code = 200;
                returnData.TimeTaken = $"{myTimerBasic}";
                returnData.Data = new List<medCpoeRecClass> ();
                returnData.Result = $"取得{藥局} {護理站} 處方異動資料共{new List<medCpoeRecClass>().Count}筆";
                return returnData.JsonSerializationt(true);
            }
            catch (Exception ex)
            {
                returnData.Code = -200;
                returnData.Result = $"Exception:{ex.Message}";
                return returnData.JsonSerializationt(true);
            }
        }
    
        private List<medCpoeClass> ExcuteCSV(string phar,string hnursta)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string filePath = @"C:\Users\Administrator\Desktop\快速配藥單_202411011533.csv";
            List<medCarInfoClass> medCarInfoClasses = new List<medCarInfoClass>();
            List<medCpoeClass> medCpoeClasses = new List<medCpoeClass>();
            using (StreamReader sr = new StreamReader(filePath, Encoding.GetEncoding("Big5")))
            {
                sr.ReadLine();
                while (!sr.EndOfStream)
                {
                    string row = sr.ReadLine();
                    string[] values = row.Split(",");
                    for(int i = 0; i < values.Length; i++)
                    {
                        values[i] = values[i].Trim('"');
                    }
                    if (values[0] != hnursta) continue;
       
                    medCpoeClass medCpoeClass = new medCpoeClass
                    {
                        GUID = Guid.NewGuid().ToString(),                 
                        藥局 = phar,
                        護理站 = values[0],
                        床號 = values[1],
                        病歷號 = $"{values[0]}-{values[1]}-{values[2]}",
                        姓名 = values[2],
                        更新時間 = DateTime.Now.ToDateTimeString(),
                        序號 = $"{values[4]}-{values[5]}-{values[8]}",
                        開始時間 = DateTime.MinValue.ToDateTimeString_6(),
                        結束時間 = DateTime.MinValue.ToDateTimeString_6(),
                        頻次代碼 = values[6],
                        藥品名 = values[4],
                        途徑 = values[5],
                        數量 = values[8],
                        單位 = values[9],
   
                    };
                    medCpoeClasses.Add(medCpoeClass);
                }
                return  medCpoeClasses;
            }            
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
        private medCarInfoClass abnormal(medCarInfoClass medCarInfoClasses)
        {
            List<string> abnormalList = new List<string>();
            double 白蛋白 = medCarInfoClasses.白蛋白.StringToDouble();
            double 肌酸酐 = medCarInfoClasses.肌酸酐.StringToDouble();
            double 估算腎小球過濾率 = medCarInfoClasses.估算腎小球過濾率.StringToDouble();
            double 丙氨酸氨基轉移酶 = medCarInfoClasses.丙氨酸氨基轉移酶.StringToDouble();
            double 鉀離子 = medCarInfoClasses.鉀離子.StringToDouble();
            double 鈣離子 = medCarInfoClasses.鈣離子.StringToDouble();
            double 總膽紅素 = medCarInfoClasses.總膽紅素.StringToDouble();
            double 鈉離子 = medCarInfoClasses.鈉離子.StringToDouble();
            double 白血球 = medCarInfoClasses.白血球.StringToDouble();
            double 血紅素 = medCarInfoClasses.血紅素.StringToDouble();
            double 血小板 = medCarInfoClasses.血小板.StringToDouble();
            double 國際標準化比率 = medCarInfoClasses.國際標準化比率.StringToDouble();


            if (白蛋白 < 3.7 || 白蛋白 > 5.3) abnormalList.Add("alb");
            if (肌酸酐 < 0.5 || 肌酸酐 > 0.9) abnormalList.Add("scr");
            if (估算腎小球過濾率 <= 60) abnormalList.Add("egfr");
            if (丙氨酸氨基轉移酶 < 33) abnormalList.Add("alt");
            if (鉀離子 <= 3.5 || 鉀離子 >= 5.1) abnormalList.Add("k");
            if (鈣離子 <= 8.6 || 鈣離子 >= 10.0) abnormalList.Add("ca");
            if (總膽紅素 < 1.2) abnormalList.Add("tb");
            if (鈉離子 <= 136 || 鈉離子 >= 145) abnormalList.Add("na");
            if (白血球 <= 4180 || 白血球 >= 9380) abnormalList.Add("wbc");
            if (血紅素 <= 10.9 || 血紅素 >= 15.6) abnormalList.Add("hgb");
            if (血小板 <= 145000.0 || 血小板 >= 383000) abnormalList.Add("plt");
            if (國際標準化比率 < 0.82 || 國際標準化比率 > 1.15) abnormalList.Add("inr");

            string[] abnormalArray = abnormalList.ToArray();
            string abnormal = string.Join(";", abnormalArray);
            medCarInfoClasses.檢驗數值異常 = abnormal;
            return medCarInfoClasses;
        }
        private string ReplaceInvalidCharacters(string input)
        {
            char replacementChar = '?';
            var output = new StringBuilder();

            foreach (char c in input)
            {
                if (char.IsSurrogate(c) || c > '\uFFFF')
                {
                    output.Append(replacementChar); // 替換成 ?
                }
                else
                {
                    output.Append(c); // 保留原始字元
                }
            }

            return output.ToString();
        }
        private Dictionary<string, List<medCpoeClass>> CoverToDictByPatID(List<medCpoeClass> medCpoeClasses)
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
        private List<medCpoeClass> SortDictByPatID(Dictionary<string, List<medCpoeClass>> dict, string 病歷號)
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

    }
}
