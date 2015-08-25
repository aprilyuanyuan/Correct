using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Correct
{
   public class ChannelInformation
   {
      public Dictionary<int, string> channelflag = new Dictionary<int, string> ();
       
       
       public  string state = "";
       public  string AllChannelInformation  =  "";

       public void InitChannelFlag()
       {
           channelflag.Add(10,"AC 5A");
           channelflag.Add(11,"空");
           channelflag.Add(12, "AC 20V");
           channelflag.Add(20,"AC 5A");
           channelflag.Add(21,"AC 500V");
           channelflag.Add(22, "DC ±300V");
           channelflag.Add(30, "DC ±30A");
           channelflag.Add(31, "DC ±500V");
           channelflag.Add(32, "DC ±300V");
           channelflag.Add(40,"AC 5A");
           channelflag.Add(41, "空");
           channelflag.Add(42, "AC 20V");
           channelflag.Add(50, "DC ±30A");
           channelflag.Add(51,"空");
           channelflag.Add(52, "DC ±20V");
           channelflag.Add(60,"AC 5A");
           channelflag.Add(61,"AC 5A");
           channelflag.Add(62, "DC ±30A");
           channelflag.Add(70, "DC ±10/40/200/500V");
           channelflag.Add(71, "DC ±10/40/200/500V");
           channelflag.Add(72,"DC ±10/40/200/500V");


       }
       
       public string IntiChannelInfo(byte[] response)
       {
           
           if(response == null)
           {
               return "";
           }
           if (response[6] != 0xFF)
           {
                string channelid = response[6].ToString();
                byte stateflag = (byte)(response[8] & 0x80);
                if (stateflag == 0x80)
                {
                    state = "打开" + "\r\n";
                }
                else if(stateflag == 0x00)
                {
                    state = "关闭" + "\r\n";
                }
                byte value1 = (byte)(response[8] & 0x1F);
                byte[] value = new byte[2] { response[7], value1};
                Int16 Sampleratevalue = BitConverter.ToInt16(value, 0);
                string Sampleratevalue2 = Sampleratevalue.ToString() + "\r\n"; //采样率
                byte channelshow = response[9];
                string channelDescription = channelflag[channelshow] + "\r\n";
                byte yearvalue = response[10];
                byte monthvalue = response[11];
                byte dayvalue = response[12];
                string caliDate = "校准时间为：" + (yearvalue + 2000) + "-" + monthvalue + "-" + dayvalue + "\r\n";
                byte namebytes = response[13];
               string usernamebyte = "校准人员为：" + Encoding.Default.GetString(response, 13, 8) + "\r\n";
                byte calibrationNum = response[21];
                string calibrationinfo = "";
                for (int k = 0; k < calibrationNum; k++)
                {
                    Int16 Frequency = BitConverter.ToInt16(response, 22 + 10 * k);
                    float calibration = BitConverter.ToSingle(response, 24 + 8 * k);
                    float offsetvalue = BitConverter.ToSingle(response, 28 + 8 * k);
                    calibrationinfo += ("频率" + Frequency.ToString() + "\r\n" + "校准系数：" + calibration.ToString() + "\r\n " + "偏置：" + offsetvalue.ToString() + "\r\n");
                }
                AllChannelInformation = "通道号:" + channelid + state + "采样率为:" + Sampleratevalue2 + "通道类型为：" + channelDescription + caliDate + usernamebyte +
                    calibrationinfo;
                return AllChannelInformation;
           }
          else
          {
                MessageBox.Show("读取通道信息失败!");
                return "";
          }          
 
       }
       
    }


}
