using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Correct
{
   public class ChannelParam
    {

       public Dictionary<byte,byte[]> myDictionary= new Dictionary<byte,byte[]>();
       
        List<CalcItem> calList = new List<CalcItem>();

        public void InitialMydictionary()
        {
            byte[] data1 = new byte[] { 12, 0 };
            myDictionary.Add(10, data1);
            myDictionary.Add(12, data1);
            myDictionary.Add(20, data1);
            myDictionary.Add(21, data1);
            byte[] data2 = new byte[] { 12, 1 };
            myDictionary.Add(22, data2);
            byte[] data3 = new byte[] { 0, 1 };
            myDictionary.Add(30, data3);
            myDictionary.Add(31, data2);
            myDictionary.Add(32, data2);
            myDictionary.Add(40, data1);
            myDictionary.Add(42, data1);
            myDictionary.Add(50, data3);
            myDictionary.Add(52, data2);
            myDictionary.Add(60, data1);
            myDictionary.Add(61, data1);
            myDictionary.Add(62, data3);
            byte[] data4 = new byte[] { 48, 4 };
            myDictionary.Add(70, data4);
            myDictionary.Add(71, data4);
            myDictionary.Add(72, data4);
        }

        public ChannelParam()
        {
            InitialMydictionary();
        }


        public override string ToString()
        {

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < calList.Count; i++)
            {
                sb.AppendLine(calList[i].ToString());
            }
            return sb.ToString();


        }

        public IList<CalcItem> CalcList
        {
            get
            {
                return calList.AsReadOnly();
            }
        }
     /// <summary>
     /// 小盒子
     /// </summary>
     /// <param name="data"></param>
     /// <param name="offset"></param>
     /// <param name="flag"></param>
       public ChannelParam(byte[] data, int offset, bool flag)
        {

            UInt16 crc = CRC16.ComputeCRC16(data, offset + 1, 124 - 2);


            UInt16 crcCal = BitConverter.ToUInt16(data, offset + 1 + 124 - 2);


            if (crc != crcCal) return;

            this.ChannnelNum = data[0 + offset];
            this.State = (byte)(data[2 + offset] >> 7);
            this.SampleRate = data[1 + offset] + ((data[2 + offset] & 0x1f) << 8);

            this.ChannelType = data[3 + offset];
            //时间
            int year = data[4 + offset] + 2000;
            int month = data[5 + offset];
            int day = data[6 + offset];
            this.Time = new DateTime(year, month, day);

            //名字
            int num = data[7 + offset];

            if (num > 0)
            {
                num = num > 8 ? 8 : num;

                this.Person = ASCIIEncoding.UTF8.GetString(data, 8 + offset, num);
            }
            else
            {

                this.Person = "";
            }


            for (int i = 0; i < 21; i++)
            {
                CalcItem calItm = new CalcItem(data, 16 + i * 5 + offset);
                if (calItm.IsValid)
                {
                    calList.Add(calItm);
                }
            }

        }
       /// <summary>
       /// 综合电压采集模块
       /// </summary>
       /// <param name="data"></param>
       /// <param name="offset"></param>
        public ChannelParam(byte[] data,int offset)
        {
            InitialMydictionary();
           UInt16 crc= CRC16.ComputeCRC16(data, offset+1, 128 - 2);


           UInt16 crcCal = BitConverter.ToUInt16(data, offset + 1 + 128 - 2);


           if (crc != crcCal) return;

            this.ChannnelNum = data[0 + offset];
            this.State = (byte)(data[2 + offset] >> 7);
            this.SampleRate = data[1 + offset] + ((data[2 + offset] & 0x1f) << 8);

            this.ChannelType = data[3 + offset];
            //时间
            int year = data[4 + offset] + 2000;
            int month = data[5 + offset];
            int day = data[6 + offset];
            this.Time = new DateTime(year, month, day);

            //名字
            int num = data[7 + offset];
            
            if (num > 0)
            {
                num = num > 7 ? 7 : num;

                this.Person = ASCIIEncoding.UTF8.GetString(data, 8 + offset, num);
            }
            else {

                this.Person = "";
            }

            
            for (int i = 0; i < 52; i++)
            {
               
                if (i < 13)
                {
                    if (i == 0)
                    {
                        CalcItem calItm2 = new CalcItem(data, 15 + offset, 0, 0, true);
                        if (calItm2.IsValid)
                        {
                            calList.Add(calItm2);
                        }
                    }
                    else
                    {
                        CalcItem calItm = new CalcItem(data, 15 + (i - 1) * 2 + offset + 4, i, 0);
                        if (calItm.IsValid)
                        {
                            calList.Add(calItm);
                        }
                    }
                }
                else if (i < 26)
                {
                    if (i == 13)
                    {
                        CalcItem calItm2 = new CalcItem(data, 15 + 12 * 2 + 4 + offset, 0, 1, true);
                        if (calItm2.IsValid)
                        {
                            calList.Add(calItm2);
                        }
                    }
                    else
                    {
                        CalcItem calItm = new CalcItem(data, 15 + (i - 2) * 2 + offset + 8, i - 13, 1);
                        if (calItm.IsValid)
                        {
                            calList.Add(calItm);
                        }
                    }
                }
                else if (i < 39)
                {
                    if (i == 26)
                    {
                        CalcItem calItm2 = new CalcItem(data, 15 + 24 * 2 + offset + 8, 0, 2,true);
                        if (calItm2.IsValid)
                        {
                            calList.Add(calItm2);
                        }
                    }
                    else
                    {
                        CalcItem calItm = new CalcItem(data, 15 + (i - 3) * 2 + offset + 12, i - 26, 2);
                        if (calItm.IsValid)
                        {
                            calList.Add(calItm);
                        }
                    }
                }
                else if (i < 52)
                {
                    if (i == 39)
                    {
                        CalcItem calItm2 = new CalcItem(data, 15 + 36 * 2 + offset + 12, 0, 3,true);
                        if (calItm2.IsValid)
                        {
                            calList.Add(calItm2);
                        }
                    }
                    else
                    {
                        CalcItem calItm = new CalcItem(data, 15 + (i - 4) * 2 + offset + 16, i -39, 3);
                        if (calItm.IsValid)
                        {
                            calList.Add(calItm);
                        } 
                    }
                }
                
            }
            
        }

        public void AddCalcItem(CalcItem itm)
        {
            calList.Add(itm);
        }
        public void ReplaceCalcItem(int m, CalcItem itm)
        {
            calList[m] = itm;
        }

        public byte ChannnelNum
        {
            get;
            set;
        }

        public byte State
        {
            get;
            set;
        }

        public int SampleRate
        {
            get;
            set;
        }
        public byte ChannelType
        {
            get;
            set;
        }

        public DateTime Time
        {
            get;
            set;
        }


        public string Person
        {
            get;
            set;
        }

       /// <summary>
       /// 小盒子
       /// </summary>
       /// <returns></returns>
        public byte[] ToArray2()
        {

            List<byte> data = new List<byte>();

            data.Add(this.ChannnelNum);
            data.Add((byte)(this.SampleRate & 0xff));
            data.Add((byte)(((this.State & 0x01) << 7) | ((this.SampleRate >> 8) & 0xff)));
            data.Add(this.ChannelType);
            //时间
            data.Add((byte)(this.Time.Year - 2000));
            data.Add((byte)(this.Time.Month));
            data.Add((byte)(this.Time.Day));

            //名字
            byte nameLen = 0;
            byte[] name = null;
            if (string.IsNullOrEmpty(this.Person) == false)
            {
                name = ASCIIEncoding.UTF8.GetBytes(this.Person);
                nameLen = (byte)name.Length;
                if (nameLen > 8)
                {
                    nameLen = 8;
                }
            }
            data.Add(nameLen);
            for (int i = 0; i < 8; i++)
            {
                if (name != null && name.Length > i)
                {
                    data.Add(name[i]);
                }
                else
                {
                    data.Add(0);
                }
            }

            //校准参数

            for (int i = 0; i < 21; i++)
            {
                if (calList.Count > i)
                {
                    data.AddRange(calList[i].ToArray3());
                }
                else
                {
                    data.AddRange(CalcItem.NULLData3);
                }
            }

            //
            for (int i = 0; i < 2; i++)
            {
                data.Add(0);
            }


            byte[] dat = data.ToArray();

            UInt16 crc = CRC16.ComputeCRC16(dat, 1, dat.Length - 1);


            data.Add((byte)(crc & 0xff));
            data.Add((byte)(crc >> 8));

            return data.ToArray();
        }

      
        public int ExistMyobject(int freqvalue,byte gearvalue)
        {
            for (int i = 0; i < calList.Count;i++)
            {
                if (calList[i].Freq == freqvalue && calList[i].Gear == gearvalue)
                {
                    return i;                  
                }
            }            
            return 100;
        }

        public byte[] ToArray()
        {

            List<byte> data = new List<byte>();

            data.Add(this.ChannnelNum);
            data.Add((byte)(this.SampleRate&0xff));
            data.Add((byte)(((this.State&0x01)<<7)|((this.SampleRate>>8)&0xff)));
            data.Add(this.ChannelType);
            //时间
            data.Add((byte)(this.Time.Year - 2000));
            data.Add((byte)(this.Time.Month));
            data.Add((byte)(this.Time.Day));

            //名字
            byte nameLen = 0;
            byte[] name = null;
            if (string.IsNullOrEmpty(this.Person)==false)
            {
                name = ASCIIEncoding.UTF8.GetBytes(this.Person);
                nameLen = (byte)name.Length;
                if (nameLen > 7)
                {
                    nameLen = 7;
                }
            }
            data.Add(nameLen);
            for (int i = 0; i < 7; i++)
            {
                if (name != null && name.Length > i)
                {
                    data.Add(name[i]);
                }
                else
                {
                    data.Add(0);
                }
            }
           
            //校准参数         
            AddZLCalcparam(data,0,0);
            AddJLCalcparam(data,25,0);
            AddJLCalcparam(data,50,0);               
            AddJLCalcparam(data,100,0);
            AddJLCalcparam(data,200,0);
            AddJLCalcparam(data,550,0);
            AddJLCalcparam(data,650,0);
            AddJLCalcparam(data,750,0);
            AddJLCalcparam(data,850,0);
            AddJLCalcparam(data,1700,0);
            AddJLCalcparam(data,2000,0);
            AddJLCalcparam(data,2300,0);
            AddJLCalcparam(data,2600,0);

            AddZLCalcparam(data,0,1);
            AddJLCalcparam(data,25,1);
            AddJLCalcparam(data,50,1);               
            AddJLCalcparam(data,100,1);
            AddJLCalcparam(data,200,1);
            AddJLCalcparam(data,550,1);
            AddJLCalcparam(data,650,1);
            AddJLCalcparam(data,750,1);
            AddJLCalcparam(data,850,1);
            AddJLCalcparam(data,1700,1);
            AddJLCalcparam(data,2000,1);
            AddJLCalcparam(data,2300,1);
            AddJLCalcparam(data,2600,1);

            AddZLCalcparam(data,0,2);
            AddJLCalcparam(data,25,2);
            AddJLCalcparam(data,50,2);               
            AddJLCalcparam(data,100,2);
            AddJLCalcparam(data,200,2);
            AddJLCalcparam(data,550,2);
            AddJLCalcparam(data,650,2);
            AddJLCalcparam(data,750,2);
            AddJLCalcparam(data,850,2);
            AddJLCalcparam(data,1700,2);
            AddJLCalcparam(data,2000,2);
            AddJLCalcparam(data,2300,2);
            AddJLCalcparam(data,2600,2);

            AddZLCalcparam(data,0,3);
            AddJLCalcparam(data,25,3);
            AddJLCalcparam(data,50,3);               
            AddJLCalcparam(data,100,3);
            AddJLCalcparam(data,200,3);
            AddJLCalcparam(data,550,3);
            AddJLCalcparam(data,650,3);
            AddJLCalcparam(data,750,3);
            AddJLCalcparam(data,850,3);
            AddJLCalcparam(data,1700,3);
            AddJLCalcparam(data,2000,3);
            AddJLCalcparam(data,2300,3);
            AddJLCalcparam(data,2600,3);


            byte[] dat = data.ToArray();

            UInt16 crc = CRC16.ComputeCRC16(dat, 1, dat.Length -1);


            data.Add((byte)(crc & 0xff));
            data.Add((byte)(crc >> 8));

           return data.ToArray();
        }

        private void AddZLCalcparam(List<byte> data,int freq,byte gear)
        {
            int index = ExistMyobject(freq, gear);
            if (index < 52)
            {
                data.AddRange(calList[index].ToArray2());
            }
            else
            {
                data.AddRange(CalcItem.NULLData);
            }
        }
        private void AddJLCalcparam(List<byte> data, int freq, byte gear)
        {
            int index = ExistMyobject(freq, gear);
            if (index < 52)
            {
                data.AddRange(calList[index].ToArray());
            }
            else
            {
                data.AddRange(CalcItem.NULLData2);
            }
        }

    }


    /// <summary>
    /// 校准条目
    /// </summary>
    public  class CalcItem
    {

       int[] FrequencyArray = {0,25, 50, 100, 200, 550, 650, 750, 850, 1700, 2000, 2300, 2600};

        /// <summary>
        /// 综合板直流
        /// </summary>
        public static byte[]  NULLData            
        {
            get
            {
                byte[] data = new byte[4];
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = 0xff;
                }
                return data;
            }
        }
        /// <summary>
        /// 综合板交流
        /// </summary>
        public static byte[] NULLData2
        {
            get
            {
                byte[] data = new byte[2];
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = 0xff;
                }
                return data;
            }
        }
        /// <summary>
        /// 小盒子
        /// </summary>
        public static byte[] NULLData3
        {
            get
            {
                byte[] data = new byte[5];
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = 0xff;
                }
                return data;
            }
        }

        /// <summary>
        /// 小盒子
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        public CalcItem(byte[] data, int offset)
        {
            this.IsValid = false;
            if (data[offset] == 0xff) return;

            byte freqindex = (byte)(data[offset] >> 4);
            byte gearval = (byte)(data[offset] & 0x0F);
            this.Freq = FrequencyArray[freqindex];
            this.Gear = gearval;

            this.CoeffK = HalfFloat.ToSingle(BitConverter.ToInt16(data, offset + 1));
            this.CoeffB = HalfFloat.ToSingle(BitConverter.ToInt16(data, offset + 3));

            this.IsValid = true;
        }
        /// <summary>
        /// 综合电压采集模块交流电压
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        public CalcItem(byte[] data, int offset,int freqindex,int gearindex)
        {
            this.IsValid = false;
            if (data[offset] == 0xff) return;

            //byte freqindex = (byte)(data[offset]>>4);
            //byte gearval = (byte)(data[offset]&0x0F);
            this.Freq = FrequencyArray[freqindex];
            this.Gear = (byte)gearindex;

            this.CoeffK = HalfFloat.ToSingle(BitConverter.ToInt16(data, offset));
            this.CoeffB = 0;
            //this.CoeffB = HalfFloat.ToSingle(BitConverter.ToInt16(data, offset + 3));           
            
            this.IsValid = true;
        }

        /// <summary>
        /// 综合电压采集模块直流电压
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="flag"></param>
        public CalcItem(byte[] data, int offset, int freqindex, int gearindex, bool flag)
        {
            this.IsValid = false;
            if (data[offset] == 0xff) return;

            //byte freqindex = (byte)(data[offset]>>4);
            //byte gearval = (byte)(data[offset]&0x0F);
            this.Freq = FrequencyArray[freqindex];
            this.Gear = (byte)gearindex;

            this.CoeffK = HalfFloat.ToSingle(BitConverter.ToInt16(data, offset));
            this.CoeffB = HalfFloat.ToSingle(BitConverter.ToInt16(data, offset + 2));

            this.IsValid = true;
        }

        public CalcItem(string cfg)
        {
            string[] param = cfg.Split(new char[] { ','});
            this.Freq = FrequencyArray[byte.Parse(param[0])];
            this.Gear = byte.Parse(param[1]);

            this.CoeffK = float.Parse(param[2]);
            this.CoeffB = float.Parse(param[3]);

            this.IsValid = true;

        }

        public CalcItem(int freq, byte gear, float coeffK, float coeffB)
        {
            this.Freq = freq;
            this.Gear = gear;
            this.CoeffK = coeffK;
            this.CoeffB = coeffB;
        }

        public bool IsValid
        {
            get;
            private set;
        }

        public int Freq
        {
            get;
            private set;
        }

        public byte Gear
        {
            get;
            private set;
        }

        public float CoeffK
        {
            get;
            private set;
        }

        public float CoeffB
        {
            get;
            private set;
        }
        public byte databytes = 0;
      /// <summary>
        /// 直流
      /// </summary>
      /// <returns></returns>

        public byte[] ToArray2()
        {
            //byte[] data = new byte[4];
            List<byte> data = new List<byte>();
          
            //int freq = Array.IndexOf(FrequencyArray, this.Freq);
            //byte freqval = (byte)freq;
            //byte freqtmp = (byte)(freqval << 4);
            //byte gearval = this.Gear;
            //byte xx = (byte)(freqtmp + gearval);
           
            //data[0] = (byte)((freqval << 4) + gearval);
            //data[0] = freqval;
            Int16 tmp = HalfFloat.ToInt16(this.CoeffK);
            data.Add((byte)(tmp & 0xff));
            data.Add((byte)((tmp >> 8) & 0xff));
            //if(freqval == 0)
            //{
            tmp = HalfFloat.ToInt16(this.CoeffB);
            data.Add((byte)(tmp & 0xff));
            data.Add((byte)((tmp >> 8) & 0xff));
            //}          
            return data.ToArray();
        }
      /// <summary>
      /// 交流
      /// </summary>
      /// <returns></returns>
      /// 
        public byte[] ToArray()
        {
           
            List<byte> data = new List<byte>();        
           
            Int16 tmp = HalfFloat.ToInt16(this.CoeffK);
            data.Add((byte)(tmp & 0xff));
            data.Add((byte)((tmp >> 8) & 0xff));          
            //tmp = HalfFloat.ToInt16(this.CoeffB);
            //data.Add((byte)(tmp & 0xff));
            //data.Add((byte)((tmp >> 8) & 0xff));
            ////}          
            return data.ToArray();
        }
        /// <summary>
        /// 小盒子
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray3()
        {
            byte[] data = new byte[5];

            int freq = Array.IndexOf(FrequencyArray, this.Freq);
            byte freqval = (byte)freq;
            //byte freqtmp = (byte)(freqval << 4);
            byte gearval = this.Gear;
            //byte xx = (byte)(freqtmp + gearval);

            data[0] = (byte)((freqval << 4) + gearval);
            Int16 tmp = HalfFloat.ToInt16(this.CoeffK);
            data[1] = (byte)(tmp & 0xff);
            data[2] = (byte)((tmp >> 8) & 0xff);

            tmp = HalfFloat.ToInt16(this.CoeffB);
            data[3] = (byte)(tmp & 0xff);
            data[4] = (byte)((tmp >> 8) & 0xff);

            return data;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            int freq = Array.IndexOf(FrequencyArray, this.Freq);
            sb.Append(freq + ",");

            sb.Append(this.Gear + ",");

            sb.Append(this.CoeffK + ",");
            sb.Append(this.CoeffB + ",");

            return sb.ToString();

        }
    }
}
