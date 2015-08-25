using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace Correct
{
    [XmlRoot("模块")]
    public  class SampleInterface
    {
        [XmlAttribute("名称")]
        public String Name;
        [XmlAttribute("设备型号")]
        public string DeviceType; 
        [XmlAttribute("设备标示")]
        public byte DeviceInfo;       
        [XmlAttribute("采样率")]
        public Int16 SampleRate;
        [XmlElement("通道")]       
        public List<Channel> Channels = new List<Channel>();

        public override string ToString()//在对应控件中加载内容时会调用这个函数
        {
            return this.Name;
        }
        public  void WriteToXml(List<SampleInterface> mk, String filePath)
        {            
            using (StreamWriter sw = new StreamWriter(filePath, false))
            {
                
                try
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<SampleInterface>), new XmlRootAttribute("模块列表"));
                    xmlSerializer.Serialize(sw, mk);
                }
                catch (Exception ex)
                {
                    
                }
            }
        }

        public static List<SampleInterface> ReadFormXml(String filePath)
        {
            try
            {
                FileStream fs = new FileStream(filePath, FileMode.Open);

                XmlSerializer ser = new XmlSerializer(typeof(List<SampleInterface>), new XmlRootAttribute("模块列表"));

                List<SampleInterface> result = (List<SampleInterface>)ser.Deserialize(fs);

                return result;
            }
            catch(Exception ex)
            {
                return new List<SampleInterface>();
            }
        }
    }

    public class Channel
    {
        [XmlAttribute("通道号")]
        public byte ChannelID;
        [XmlElement("类型")]
        public ChannelType channeltype = new ChannelType();
        [XmlElement("档位")]
        public List<Gear> gears = new List<Gear>();
    }

    public class ChannelType
    {
        [XmlAttribute("类型")]
        public SingelType SingnelType;      
        [XmlAttribute("类型标示")]
        public byte TypeID;      
 
    }

    public class Calibration
    {
        [XmlAttribute("频率")]
        public Int16 frequency;
        [XmlAttribute("校准系数")]
        public float calibrationfactor;
       
    }

    public class Gear
    {
        [XmlAttribute("档位")]
        public byte GearID;
        [XmlAttribute("名称")]
        public string Name;
        //[XmlElement("量程")]
        //public List<Span> spans = new List<Span>();
        [XmlElement("频率")]
        public List<Calibration> calibration = new List<Calibration>();
    }

    //public class Span
    //{
    //    [XmlElement("最小值")]
    //    public float Min;
    //    [XmlElement("最大值")]
    //    public float Max;
    //}

    /// <summary>
    /// 信号类型
    /// </summary>
    public enum SingelType
    {
        /// <summary>
        /// 直流电流
        /// </summary>
        DCCurrent = 0,
        /// <summary>
        /// 交流电流
        /// </summary>
        ACCurrent = 1,
        /// <summary>
        /// 直流电压
        /// </summary>
        DCVolt = 2,
        /// <summary>
        /// 
        /// 交流电压
        /// </summary>
        ACVolt = 3,
    }
}
