using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using HIS_DB_Lib;
using Basic;
using Newtonsoft.Json;

namespace DB2VM_API
{
    public class orderlistClass
    {
        [JsonPropertyName("opdDate")]
        public string 開方日期 { get; set; }
        [JsonPropertyName("patientNo")]
        public int 病歷號 { get; set; }
        [JsonPropertyName("medicationOrderNumber")]
        public int 領藥號 { get; set; }
        [JsonPropertyName("patientName")]
        public string 病人姓名 { get; set; }
        [JsonPropertyName("nsName")]
        public string 病房 { get; set; }
        [JsonPropertyName("bedNo")]
        public string 床號 { get; set; }
        [JsonPropertyName("medicationItems")]
        public List<MedicationOrder> medicationItems { get; set; }

        static public orderlistClass get_order(string Barcode)
        {
            string url = "";
            if (Barcode.Length > 12)
            {
                url = "http://192.168.16.230:8132/api/medication/scanclinicmedicationbag?barcode={Barcode}";
            }
            else
            {
                url = "http://192.168.16.230:8132/api/Medication/ScanInpMedicationBag?BarCode={Barcode}";
            }
            returnData returnData = new returnData();
            string json_out = Net.WEBApiGet(url);
            orderlistClass orderlistClass = json_out.JsonDeserializet<orderlistClass>();
            return orderlistClass;
        }

    }
    public class MedicationOrder
    {
        [JsonPropertyName("code")]
        public string 料號 { get; set; }
        [JsonPropertyName("fullName")]
        public string 藥品名稱 { get; set; }
        [JsonPropertyName("qty")]
        public double 單次劑量 { get; set; }
        [JsonPropertyName("useName")]
        public string 頻次 { get; set; }
        [JsonPropertyName("medthodName")]
        public string 途徑 { get; set; }
        [JsonPropertyName("tqty")]
        public double 交易量 { get; set; }
        [JsonPropertyName("acntPtr")]
        public int 批序 { get; set; }
        [JsonPropertyName("type")]
        public string 藥袋類型 { get; set; }
    }

}
