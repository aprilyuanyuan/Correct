using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Correct
{
   
    public class DeviceInformation
    {
        public string[] DeviceName = new string[] { "轨道电路监测模块", "交流道岔监测模块", "直流道岔监测模块", "交流信号机监测模块", "直流信号机监测模块", "综合信号板电流采集模块", "综合信号板电压采集模块" };
        public string Deviceflag = "";
        public string Devicename = "";
 
      
        public string Sequence = "";
        public string Username = "";
        public DateTime OperationDate = new DateTime();
        public string AllDeviceInformation = "";

        public DeviceInformation()
        {
            this.IsValid = false;
        }

        public string DeviceVersionInformation(byte[] response)
        {
            string A = response[6].ToString() + ".";
            string B = response[7].ToString() + ".";
            string C = response[8].ToString() + ".";
            string D = response[9].ToString();
            byte year = response[10];
            string month = response[11].ToString();
            string day = response[12].ToString();
            
            string VersionInformation = "版本号:" + A + B + C + D + "\r\n" + "版本日期:" + (year + 2000).ToString() + "-" + month + "-" + day;
            return VersionInformation;
        }

        public string InitDeviceInformation(byte[] response)
        {         
            string Deviceflag = response[6].ToString() + "\r\n";
            string devicename = DeviceName[response[6] - 1] + "\r\n";
            
            Username = Encoding.Default.GetString(response, 12, 8) + "\r\n";
            
            byte year2 = response[20];
            byte month2 = response[21];
            byte day2 = response[22];

            
            byte year3 = response[23];
            byte month3 = response[24];
            byte day3 = response[25];
            string Productiondate = (year3 + 2000).ToString() + "-" + month3.ToString() + "-" + day3.ToString() + "\r\n";

            byte productionsequencelength = response[26];
            string productionsequencename = Encoding.Default.GetString(response, 27, productionsequencelength) + "\r\n";

            byte devicetypelength = response[27 + productionsequencelength];
            string devicetype = Encoding.Default.GetString(response, 28 + productionsequencelength, devicetypelength) + "\r\n";

            
           
            string Openrationtime = (year2 + 2000).ToString() + "-" + month2.ToString() + "-" + day2.ToString() + "\r\n";
            AllDeviceInformation = "设备名称:" + devicename + "型号:" + devicetype + "生产日期:" + Productiondate + "序列号:" + productionsequencename + "操作时间:" + Openrationtime + "操作人员:" + Username;
            return AllDeviceInformation;
        }

        public bool IsValid
        {
            get;
            private set;
        }


        public int DevIndex
        {
            get;
            private set;
        }
        public string Name
        {
            get;
            private set;
        }
        public DateTime DevTime
        {
            get;
            private set;
        }

        public void ParseData(byte[] data)
        {
            int index = data[6] - 1;
            if (index >= DeviceName.Length || index<0) return;
            this.DevIndex = index;
            this.Name = DeviceName[index];
          
            this.IsValid = true;
        }
    }
}
