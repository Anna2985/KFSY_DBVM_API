using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Basic;
using HIS_DB_Lib;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DB2VM_API.Controller._API_處方取得
{
    [Route("api/[controller]")]
    [ApiController]
    public class BBAR : ControllerBase
    {
        //static public string API_Server = "http://127.0.0.1:4433";
        //[HttpGet]
        //public string get_order(string? BarCode)
        //{
        //    MyTimerBasic myTimerBasic = new MyTimerBasic();
        //    returnData returnData = new returnData();
        //    try
        //    {
        //        if (BarCode.StringIsEmpty())
        //        {
        //            returnData.Code = -200;
        //            returnData.Result = "Barcode空白";
        //            return returnData.JsonSerializationt(true);
        //        }
        //        orderlistClass orderlistClass = orderlistClass.get_order(BarCode);
        //        List<medClass> medClasses = medClass.get_med_cloud(API_Server);
        //        List<OrderClass> orderClasses = new List<OrderClass>();
           
        //        foreach (var medicationItems in orderlistClass.medicationItems)
        //        {
        //            medClass targetMed = medClasses.Where(temp => temp.料號 == medicationItems.料號).FirstOrDefault();
        //            if (targetMed == null) 
        //            {
        //                targetMed = new medClass();
        //                targetMed.藥品碼 = medicationItems.料號;
        //            }
        //            OrderClass orderClass = new OrderClass
        //            {
        //                PRI_KEY = BarCode,
        //                藥袋條碼 = BarCode,
        //                開方日期 = orderlistClass.開方日期.StringToDateTime().ToDateTimeString_6(),
        //                病歷號 = orderlistClass.病歷號.ToString(),
        //                領藥號 = orderlistClass.領藥號.ToString(),
        //                病人姓名 = orderlistClass.病人姓名,
        //                藥品碼 = targetMed.藥品碼,
        //                藥品名稱 = medicationItems.藥品名稱,
        //                單次劑量 = medicationItems.單次劑量.ToString(),
        //                頻次 = medicationItems.頻次,
        //                途徑 = medicationItems.途徑,
        //                交易量 = (medicationItems.交易量 * -1).ToString(),
        //                批序 = medicationItems.批序.ToString(),
        //                藥袋類型 = medicationItems.藥袋類型,
        //                病房 = orderlistClass.病房,
        //                床號 = orderlistClass.床號,
        //                狀態 = "未過帳"                        
        //            };
        //            if (orderClass.藥袋類型 == "A") orderClass.藥袋類型 = "New";
        //            if (orderClass.藥袋類型 == "D") orderClass.藥袋類型 = "DC";
        //            orderClasses.Add(orderClass);
        //        }
        //        medCarInfoClass.update_order_list(API_Server, orderClasses);
        //        //List<OrderClass> update_OrderClass = medCarInfoClass.update_order_list(API_Server, orderClasses);
        //        returnData.Data = orderClasses;
        //        returnData.Code = 200;
        //        return returnData.JsonSerializationt(true);
        //    }
        //    catch(Exception ex)
        //    {
        //        returnData.Code = -200;
        //        returnData.Result = $"Exception:{ex.Message}";
        //        return returnData.JsonSerializationt(true);
        //    }
        //}
        //[HttpPost("get_order")]
        //public string get_order([FromBody] returnData returnData)
        //{
        //    List<orderlistClass> orderlistClasses = returnData.Data.ObjToClass<List<orderlistClass>>();
        //    List<OrderClass> orderClasses = new List<OrderClass>();
        //    foreach (var orderlistClass in orderlistClasses)
        //    {
        //        foreach(var medicationItems in orderlistClass.medicationItems)
        //        {
        //            OrderClass orderClass = new OrderClass
        //            {
        //                開方日期 = orderlistClass.開方日期.StringToDateTime().ToDateTimeString(),
        //                病歷號 = orderlistClass.病歷號.ToString(),
        //                藥品碼 = medicationItems.料號,
        //                藥品名稱 = medicationItems.藥品名稱,
        //                單次劑量 = medicationItems.單次劑量.ToString(),
        //                頻次 = medicationItems.頻次,
        //                途徑 = medicationItems.途徑,
        //                交易量 = medicationItems.交易量.ToString()
        //            };
        //            orderClasses.Add(orderClass);
        //        }               
        //    }
        //    returnData.Data = orderClasses;
        //    return returnData.JsonSerializationt(true);
        //}
    }

}
