using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.IO;
using Lon.IO.Ports;
using System.Windows.Forms;
using System.Collections;

namespace Correct
{
    public class Device
    {
        private static byte[] lastFrame = null;
        public SerialPort sp;
        static readonly string NewLine = Environment.NewLine;      
        List<FrameMonitor> listMonitor = new List<FrameMonitor>();
        AutoResetEvent eventRequest = new AutoResetEvent(false);
        Queue<DeviceRequest> listRequest = new Queue<DeviceRequest>();


        List<ChannelParam> listChannel = new List<ChannelParam>();




        List<ADBlock> listBlock = new List<ADBlock>();


        int[] gearList = new int[10];


        Thread threadRequest;


        public ADBlock GetADBlock(int channel)
        {
            return listBlock[channel];
        }

        public int GetGear(int channel)
        {
            return gearList[channel];
        }
        public void SendRequest(DeviceRequest request)
        {

            lock (((ICollection)listRequest))
            {
                listRequest.Enqueue(request);
                eventRequest.Set();
            }
        }

        public byte ChannelGear
        {
            get;
            set;
        }

        public string ChannelInformation
        {
            get;
            set;
        }

        public string DeviceInformation
        {
            get;
            set;
        }

        public Dictionary<float, Record> RecordResult //上一次校验结果
        {
            set;
            get;
        }

        public List<float> frequency
        {
            get;
            set;
        }


        public void UpdateChannelParam(ChannelParam param)
        {
            int chNum = param.ChannnelNum;
            listChannel[chNum] = param;
        }

        public float CalRealVal(int gear,int channel, float freq, float adVal)
        {
            ChannelParam param = listChannel[channel];

            int index = -1;
            float diffFreq = float.MaxValue;
            IList<CalcItem> listCal=param.CalcList;

            for (int i = 0; i < listCal.Count; i++)
            {

                if (listCal[i].Gear != gear) continue;
                if (Math.Abs(listCal[i].Freq - freq) < diffFreq)
                {
                    index = i;
                    diffFreq = Math.Abs(listCal[i].Freq - freq);
                }
            }


            float realValue = adVal;

            if (index >= 0)
            {
                 realValue = listCal[index].CoeffK * adVal + listCal[index].CoeffB;
            }



            return realValue;
        }


        public Device()
        {
            frequency = new List<float>();

            for (int i = 0; i < 3; i++)
            {
                listBlock.Add(new ADBlock(8000, 8000));
            }

            for (int i = 0; i < 3; i++)
            {
                listChannel.Add(new ChannelParam());
            }

           threadRequest = new Thread(new ThreadStart(ProcessRequest));
            threadRequest.IsBackground = true;
            threadRequest.Start();
        }

        public FrameMonitor AddMonitor()
        {
            FrameMonitor monitor = new FrameMonitor();

            lock (((ICollection)listMonitor).SyncRoot)
            {
                listMonitor.Add(monitor);
            }

            return monitor;
        }

        public void DeleteMonitor(FrameMonitor monitor)
        {
            lock (((ICollection)listMonitor).SyncRoot)
            {
                listMonitor.Remove(monitor);
            }
        }


        public void IniPort(string portname, int baud)
        {
            sp = new SerialPort(portname, baud);

            sp.Parity = Parity.None;
            sp.StopBits = StopBits.One;
            sp.DataBits = 8;
            sp.ReadBufferSize = 1024 * 1024;
            if (!sp.IsOpen)
            {
                sp.Open(); //打开串口                 
            }
            Thread rxThread = new Thread(new ThreadStart(ReceiveData));
            rxThread.Name = "RxProc";
            rxThread.IsBackground = true;
            rxThread.Start();
        }

        public void ClosePort()
        {          
            sp.Close();
        }

        public DeviceRequest WriteDeviceInformation(byte[] cmdlist, int timeOut)
        {
            DeviceRequest req = new DeviceRequest(0x81, cmdlist);
            return req;
        }
   
        /// <summary>
        /// 写入数据帧
        /// </summary>
        /// <param name="df"></param>
        /// <returns>应答数据帧</returns>
    

        private Int16 CalcSum(byte[] Data)
        {
            Int16 sum = 0;
            for (int i = 0; i < Data.Length; i++)
            {
                sum += Data[i];
            }
            return sum;
        }

        #region
        public DeviceRequest GetVersionInformation(int timeOut)
        {
            DeviceRequest req = new DeviceRequest(0x97, new byte[] { });
            return req;
        }

        public DeviceRequest ReadDeviceInformation(int timeOut)
        {
            DeviceRequest req = new DeviceRequest(0x91, new byte[] { });
            return req;
        }

        public DeviceRequest GetTimeNow()
        {
            DeviceRequest req = new DeviceRequest(0x92, new byte[] { 0});
            return req;            
        }

        public DeviceRequest WriteChannelInfo(byte[] RequestData)
        {
            DeviceRequest req = new DeviceRequest(0x83, RequestData);
            return req;   
        }

        public DeviceRequest SetTime()
        {
            DateTime dt = DateTime.Now;
            byte year = Convert.ToByte(dt.Year - 2000);
            byte month = Convert.ToByte(dt.Month);
            byte day = Convert.ToByte(dt.Day);
            byte hour = Convert.ToByte(dt.Hour);
            byte minute = Convert.ToByte(dt.Minute);
            byte seconds = Convert.ToByte(dt.Second);
            List<byte> bytes = new List<byte>();
            bytes.Add(year);
            bytes.Add(month);
            bytes.Add(day);
            bytes.Add(hour);
            bytes.Add(minute);
            bytes.Add(seconds);
            byte[] timeData = bytes.ToArray();
            DeviceRequest req = new DeviceRequest(0x82, timeData);
            return req;
        }
       
        public DeviceRequest ReadLastTime()
        {
            DeviceRequest req = new DeviceRequest(0x92, new byte[] { 1 });
            return req;
        }
        

        public DeviceRequest ReadChannelInformation(byte channelId)
        {
            DeviceRequest req = new DeviceRequest(0x93, new byte[] { channelId });
            return req; 
        }
        

        public DeviceRequest SetSpan(byte channelId, byte gearvalue, int timeOut) //设置量程
        {
            DeviceRequest req = new DeviceRequest(0x84, new byte[] { channelId,gearvalue });
            return req;
        }

        #endregion

        private void SendSerialData(byte cmd, byte[] data)
        {
            List<byte> cmdList = new List<byte>();


            //cmdList.Add(0xaa);
            //cmdList.Add(0xaa);
            //cmdList.Add(0x01);
            //cmdList.Add(0x0a);
            //cmdList.Add(0x00);

            cmdList.Add(0xaa);
            cmdList.Add(0xaa);

            cmdList.Add(0x01);

            UInt16 length = (UInt16)(1 + data.Length);

            cmdList.Add((byte)(length & 0xff));

            cmdList.Add((byte)((length >> 8) & 0xff));

            Int16 sumcheck = cmd;

            cmdList.Add(cmd);


            for (int i = 0; i < data.Length; i++)
            {
                cmdList.Add(data[i]);

                sumcheck += data[i];
            }

            cmdList.Add((byte)(sumcheck & 0xff));

            cmdList.Add((byte)((sumcheck >> 8) & 0xff));

            cmdList.Add((byte)'\r');
            cmdList.Add((byte)'\n');


            byte[] rawData = cmdList.ToArray();

            try
            {
                sp.Write(rawData, 0, rawData.Length);
            }
            catch (Exception)
            { }



        }


        /// <summary>
        /// 处理设备的请求命令
        /// </summary>
        private void ProcessRequest()
        {

            try
            {                
                FrameMonitor monitor = AddMonitor();
                while (true)
                {
                    DeviceRequest[] requests = null;
                    lock (((ICollection)listRequest).SyncRoot)
                    {
                       requests= listRequest.ToArray();

                       listRequest.Clear();
                    }


                    if (requests == null || requests.Length <= 0)
                    {

                        eventRequest.WaitOne();
                        continue;
                    }

                    for (int i = 0; i < requests.Length; i++)
                    {

                        monitor.Cmd = requests[i].Cmd - 0x80;

                        SendSerialData(requests[i].Cmd, requests[i].Request);

                        monitor.Reset();


                       SerialFrame sf= monitor.GetFrame(500);
                       if (sf != null)
                       {
                           requests[i].AddResponse(sf.RawData);
                       }

                    }
                }
            }
            catch (Exception)
            {
 
            }


        }


        private void ReceiveData()
        {
            byte[] buffer = new byte[128 * 1024 * 2];

            byte[] frameBuffer = new byte[128 * 1024];

            int framenum0 = 1;

            int Lastframesequence0 = 0;

            int framenum1 = 1;

            int Lastframesequence1 = 0;

            int framenum2 = 1;

            int Lastframesequence2 = 0;

            int frameLength = 0;

            int leftLength = 0;

            int dataLength = 0;

            int realLength = 0;

            bool rollback = false;

            try
            {
                sp.ReadTimeout = 500;

                while (true)
                {

                    dataLength=0; 

                    if (rollback) //前一个数据帧回滚
                    {
                        for (int i = 1; i < frameLength; i++)
                        {
                            if (frameBuffer[i] == 0xaa && ((i==frameLength-1) || (frameBuffer[i+1]==0xaa)))
                            {
                                dataLength = frameLength - i;
                                Array.Copy(frameBuffer, i, buffer, 0, dataLength);
                                break;
                            }
                        }
                        frameLength = 0;
                        rollback = false;
                    }

                    if (leftLength > 0) //前一个BUffer剩下的Data
                    {
                        Array.Copy(buffer, buffer.Length / 2, buffer, dataLength, leftLength);
                        dataLength += leftLength;
                        leftLength = 0;
                    }

                    if(dataLength<=0) //串口数据
                    {
                        try
                        {
                            dataLength = sp.Read(buffer, 0, buffer.Length / 2);
                        }
                        catch (TimeoutException)
                        {
                            rollback = true;
                            continue;
                        }
                        
                    }

                    for (int i = 0; i < dataLength; i++)
                    {
                        frameBuffer[frameLength++] = buffer[i];
                        leftLength = dataLength - i - 1;
                        switch (frameLength)
                        {
                            case 1:
                            case 2:
                                if (frameBuffer[frameLength - 1] != 0xaa) //帧头
                                {
                                    rollback = true;
                                }
                                break;
                            case 3:
                                if (frameBuffer[2] != 1) //版本协议
                                {
                                    rollback = true;
                                }
                                break;
                            case 4:
                                break;
                            case 5:
                                {
                                     realLength = frameBuffer[3] + (frameBuffer[4] << 8);//数据长度
                                     if (realLength+9 > frameBuffer.Length || (realLength <= 0))
                                     {
                                         rollback = true;
                                     }
                                }
                                break;
                            default:
                                if (frameLength == realLength + 9) //收到完整数据
                                {
                                    if (frameBuffer[5] == 0x15)
                                    {
                                        //0通道帧序号的判断
                                        if (framenum0 == 2 && frameBuffer[6] == 0)
                                        {
                                            int Presentframesequence = BitConverter.ToInt32(frameBuffer, 7);
                                            if (Presentframesequence != Lastframesequence0 + 1)
                                            {
                                                rollback = true;
                                            }
                                            Lastframesequence0 = Presentframesequence;
                                        }
                                        else if (framenum0 == 1 && frameBuffer[6] == 0)
                                        {
                                            framenum0 = 2;
                                            Lastframesequence0 = BitConverter.ToInt32(frameBuffer, 7);
                                        }

                                        //1通道帧序号的判断
                                        if (framenum1 == 2 && frameBuffer[6] == 1)
                                        {
                                            int Presentframesequence = BitConverter.ToInt32(frameBuffer, 7);
                                            if (Presentframesequence != Lastframesequence1 + 1)
                                            {
                                                rollback = true;
                                            }
                                            Lastframesequence1 = Presentframesequence;
                                        }
                                        else if (framenum1 == 1 && frameBuffer[6] == 1)
                                        {
                                            framenum1 = 2;
                                            Lastframesequence1 = BitConverter.ToInt32(frameBuffer, 7);
                                        }

                                        //2通道帧序号的判断
                                        if (framenum2 == 2 && frameBuffer[6] == 2)
                                        {
                                            int Presentframesequence = BitConverter.ToInt32(frameBuffer, 7);
                                            if (Presentframesequence != Lastframesequence2 + 1)
                                            {
                                                rollback = true;
                                            }
                                            Lastframesequence2 = Presentframesequence;
                                        }
                                        else if (framenum0 == 1 && frameBuffer[6] == 2)
                                        {
                                            framenum2 = 2;
                                            Lastframesequence2 = BitConverter.ToInt32(frameBuffer, 7);
                                        }
                                    }

                                   UInt16 calSum= CalSUMCheck(frameBuffer, 5, realLength);

                                   UInt16 realSum = (UInt16)(frameBuffer[realLength + 5] + (frameBuffer[realLength + 6] << 8));
                                   if (calSum != realSum)
                                   {
                                       rollback = true;
                                   }                               
                                   else
                                   {
                                       
                                       //获取到完整的数据帧
                                       if (frameBuffer[5] == 0x15)
                                       {
                                           int channel = frameBuffer[6];
                                           byte gearvalue = (byte)(frameBuffer[12]  >> 4);
                                           if(gearvalue > 3)
                                           {
                                               rollback = true;
                                           }

                                           gearList[channel] = gearvalue;
                                           if (channel < 3)
                                           {
                                               int adLen = frameBuffer[3] + (frameBuffer[4] << 8) - 6;//数据长度

                                               List<Int16> listAd = new List<Int16>();

                                               for (int j = 0; j < adLen/2; j++)
                                               {
                                                   Int16 adVal = (Int16)(frameBuffer[j*2+11] +( (frameBuffer[j*2 + 12]&0x0F)<<8));
                                                   listAd.Add(adVal);
                                               }
                                               Int16[] adArray=listAd.ToArray();
                                               listBlock[channel].PutAdData(adArray, 0, adArray.Length);

                                           }

                                       }
                                       SerialFrame sf = new SerialFrame(frameBuffer, frameLength);
                                       lock (((ICollection)listMonitor).SyncRoot)
                                       {
                                           foreach (FrameMonitor monitor in listMonitor)
                                           {
                                               monitor.PutFrame(sf);
                                           }
                                       }                                      
                                       
                                       frameLength = 0;                                    
                                     
                                   } 
                                }
                                break;
                        }

                        if (rollback || frameLength>=frameBuffer.Length)
                        {
                            Array.Copy(buffer, i + 1, buffer, buffer.Length / 2, leftLength);
                            break;
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }


        public UInt16 CalSUMCheck(byte[] data,int index,int cnt)
        {

            int sum = 0;
            for (int i = index; i < index + cnt; i++) {

                sum += data[i];
            }

            return (UInt16)(sum & 0xffff);

        }





        byte[] tempbuf = new byte[1024 * 4];

       
       

      

        public float Ampli = 0;
      
        public DeviceRequest ReadSpan(byte channelid)
        {
            DeviceRequest req2 = new DeviceRequest(0x94, new byte[]{channelid});
            return req2;
        }

        public DeviceRequest Reset()
        {
            DeviceRequest req2 = new DeviceRequest(0x86, new byte[] { });
            return req2;
        }
    }

}
