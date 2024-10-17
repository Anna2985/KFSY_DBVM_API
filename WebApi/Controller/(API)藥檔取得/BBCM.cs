using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Basic;
using HIS_DB_Lib;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DB2VM_API.Controller._API_藥檔取得
{
    [Route("api/[controller]")]
    [ApiController]
    public class BBCM : ControllerBase
    {
        static public string API_Server = "http://127.0.0.1:4433";
        [HttpGet]
        public string Get(string? code)
        {
            MyTimerBasic myTimerBasic = new MyTimerBasic();
            returnData returnData = new returnData();
            try
            {
                List<medicineClass> medicineClasses = medicineClass.get_med(code);

                List<medClass> medClasses = new List<medClass>();
                foreach (var medicineClass in medicineClasses)
                {
                    medClass medClass = new medClass
                    {
                        藥品碼 = medicineClass.藥品碼,
                        藥品名稱 = medicineClass.藥品名稱,
                        藥品學名 = medicineClass.藥品學名,
                        中文名稱 = medicineClass.中文名稱,
                        最小包裝單位 = medicineClass.最小包裝單位,
                        包裝單位 = medicineClass.最小包裝單位,
                        警訊藥品 = medicineClass.警訊藥品.ToString(),
                        管制級別 = medicineClass.管制級別.ToString(),
                        開檔狀態 = medicineClass.開檔狀態,
                        料號 = medicineClass.料號.Trim(),
                        ATC = medicineClass.healthInsurance.ATC,
                        中西藥 = "西藥"
                    };
                    if (medClass.管制級別 == "0") medClass.管制級別 = "N";
                    if (medClass.開檔狀態 == "N") medClass.開檔狀態 = "停用中";
                    if (medClass.開檔狀態 == "Y") medClass.開檔狀態 = "開檔中";
                    if(medClass.料號.StringIsEmpty() == false && medClass.藥品碼.StringIsEmpty() == false) medClasses.Add(medClass);

                }

                //List<medClass> medClasses = medClass.get_med_cloud(API);
                medCarInfoClass.update_med_page_cloud(API_Server, medClasses);
                //medClass.add_med_clouds(API_Server, medClasses);
                returnData.Code = 200;
                returnData.Result = $"取得藥品資料共{medClasses.Count}筆";
                returnData.TimeTaken = $"{myTimerBasic}";
                returnData.Data = medClasses;
                return returnData.JsonSerializationt(true);
            }
            catch(Exception ex)
            {
                returnData.Code = -200;
                returnData.Result = ex.Message;
                return returnData.JsonSerializationt(true);
            }
        
        }
        [HttpPost("get_med")]
        public string get_med([FromBody] returnData returnData)
        {
            try
            {
                List<medicineClass> medicineClasses = returnData.Data.ObjToClass<List<medicineClass>>();
                List<medClass> medClasses = new List<medClass>();
                foreach(var medicineClass in medicineClasses)
                {
                    medClass medClass = new medClass 
                    {
                        藥品碼 = medicineClass.藥品碼,
                        藥品名稱 = medicineClass.藥品名稱,
                        藥品學名 = medicineClass.藥品學名,
                        中文名稱 = medicineClass.中文名稱,
                        最小包裝單位 = medicineClass.最小包裝單位,
                        包裝單位 = medicineClass.最小包裝單位,
                        警訊藥品 = medicineClass.警訊藥品.ToString(),
                        管制級別 = medicineClass.管制級別.ToString(),
                        開檔狀態 = medicineClass.開檔狀態,
                        料號 = medicineClass.料號.Trim(),
                        ATC = medicineClass.healthInsurance.ATC,
                        中西藥 = "西藥"
                    };
                    
                    medClasses.Add(medClass);
                }
                medClass.add_med_clouds(API_Server, medClasses);
                //List<medClass> update_medClass = medCarInfoClass.update_med_page_cloud(API, medClasses);
                returnData.Code = 200;

                //returnData.TimeTaken = $"{myTimerBasic}";
                returnData.Data = medClasses;            
                return returnData.JsonSerializationt(true);
            }
            catch(Exception ex)
            {
                returnData.Code = -200;
                returnData.Result = ex.Message;
                return returnData.JsonSerializationt(true);
            }
        }

    }
}
