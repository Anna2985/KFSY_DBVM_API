using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Basic;
using HIS_DB_Lib;
using System.Collections.Concurrent;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DB2VM_API.Controller._API_藥檔圖片
{
    [Route("api/[controller]")]
    [ApiController]
    public class med_pic : ControllerBase
    {
        static private string API_Server = "http://127.0.0.1:4433";
        [HttpGet]
        public string get()
        {
            MyTimerBasic myTimerBasic = new MyTimerBasic();
            returnData returnData = new returnData();
            try
            {
        
                List<medClass> medClasses = medClass.get_med_cloud(API_Server);       
                List<medPicClass> medPicClasses = new List<medPicClass>();
                
                ConcurrentBag<medPicClass> localList = new ConcurrentBag<medPicClass>();

                Parallel.ForEach(medClasses, new ParallelOptions { MaxDegreeOfParallelism = 4 }, medClass =>
                {
                    string code = medClass.料號;
                    string 藥碼 = medClass.藥品碼;
                    medPictureClass medPictureClass = medPictureClass.get_pic(code);
                    if (medPictureClass != null && medPictureClass.pic_base64.StringIsEmpty() == false)
                    {
                        medPicClass medPicClass = new medPicClass
                        {
                            藥碼 = 藥碼,
                            藥名 = medPictureClass.藥名,
                            副檔名 = "png",
                            pic_base64 = medPictureClass.pic_base64,
                        };
                        localList.Add(medPicClass);
                    }
                });
                lock (medPicClasses)
                {
                    medPicClasses.AddRange(localList);
                }

                medPicClass.add(API_Server, medPicClasses);
                //for (int i = 0; i < medPicClasses.Count; i++) medPicClass.add(API_Server, medPicClasses[i]);

                returnData.Code = 200;
                returnData.Result = $"新增<{medPicClasses.Count}>筆圖片";
                returnData.TimeTaken = $"{myTimerBasic}";
                //returnData.Data = medPicClasses;
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
