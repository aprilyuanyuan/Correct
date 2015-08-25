using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Threading;
using System.Diagnostics;
using NationalInstruments.VisaNS;

namespace Lon.IO.Ports
{
    public class FunGen
    {
        List<string> deviceList = null;
        public List<string> GetDevDescList()
        {
            deviceList = null;
            try
            {
                string[] resources = ResourceManager.GetLocalManager().FindResources("USB?*INSTR");
                deviceList = new List<string>(resources);
            }
            catch(Exception ex)
            {
            }
            return deviceList;

            
        }
        private static MessageBasedSession afg3022Session;
        public FunGen(string descriptor)
        {           

        }
        static String desc = null;
        public FunGen()
        {
            if (desc != null)
            {
               // afg3022.Descriptor = desc;
                return;
            }
            if (GetDevDescList() != null && deviceList.Count != 0)
            {
                afg3022Session = (MessageBasedSession)ResourceManager.GetLocalManager().Open(deviceList[0]);
            }
        }

        public bool SetFreq(int chNum, float freq)
        {

            afg3022Session.Write("SOURce" + (chNum+1)+ ":FREQuency:FIXed " + freq);
            return true;

        }
        public bool SetAmpli(int chNum, float ampli)
        {

            Thread.Sleep(300);
            afg3022Session.Write("SOURce" + (chNum + 1).ToString() + ":VOLTage:LEVel:IMMediate:AMPLitude " + ampli + "\r\n");
            //afg3022.WriteString("SOURce" + (chNum + 1).ToString() + ":VOLTage:LEVel:IMMediate:AMPLitude " + ampli  + "\r\n");
            return true;

        }
        public bool SetVOLOffset(int chNum, float ampli)
        {

            Thread.Sleep(300);
            //SOURce1:VOLTage:LEVel:IMMediate:OFFSet 3            
            afg3022Session.Write("SOURce" + (chNum + 1).ToString() + ":VOLTage:LEVel:IMMediate:OFFSet " + ampli);
            //afg3022.WriteString("SOURce" + (chNum + 1).ToString() + ":VOLTage:LEVel:IMMediate:AMPLitude " + ampli  + "\r\n");
            return true;
        }

       

        public void CreateDC(int channel,float ampl)
        {
            int sampleCnt = 500;
            byte[] data = new byte[sampleCnt * 2];

            for (int i = 0; i < sampleCnt; i++)
            {
                data[i * 2] = 0xff;
                data[i * 2 + 1] = 0xff;
            }
            afg3022Session.Write("SOUR" + (channel + 1) + ":FM:STATE OFF");
            afg3022Session.Write("TRACe:DEFine EMEMory," + sampleCnt);

            //afg3022Session.SendEndEnabled = false;
            afg3022Session.Write("TRACE:DATA EMEMORY,#" + data.Length.ToString().Length + data.Length);
            //afg3022Session.SendEndEnabled = true;
            afg3022Session.Write(data);
            afg3022Session.Write("SOUR" + (channel + 1) + ":FREQ 1");
            afg3022Session.Write("SOUR" + (channel + 1) + ":FUNC EMEMORY");
            afg3022Session.Write("SOURce" + (channel + 1) + ":VOLTage:LEVel:IMMediate:AMPLitude " + ampl);
            afg3022Session.Write("OUTPut ON");
           
        }


        public void Output(bool state)
        {
            if (state)
            {
                afg3022Session.Write("OUTPut ON");
            }
            else
            {
                afg3022Session.Write("OUTPut OFF");
            }
        }

        public bool SetSinMode(int chNum)
        {

            Thread.Sleep(100);
            afg3022Session.Write("SOURce" + (chNum + 1).ToString() + ":FUNCtion:SHAPe SIN");
            //afg3022.WriteString("SOURce" + (chNum + 1).ToString() + ":FUNCtion:SHAPe SIN");
            return true;

        }
        private bool WaitDevRdy(int waitTime)
        {
            int i = 0;
            while (afg3022Session == null || deviceList.Count == 0)
            {
                Thread.Sleep(100);
                if (GetDevDescList() != null && deviceList.Count != 0)
                {
                    afg3022Session = (MessageBasedSession)ResourceManager.GetLocalManager().Open(deviceList[0]);
                }
                if (i > waitTime)
                {
                    return false;
                }
                i++;
            }
            return true;
        }
        public bool SetFmMode(int chNum,bool state,float freq1,float freq2)
        {

            //Thread.Sleep(100);
            if(state)
            {
            afg3022Session.Write("SOURce" + (chNum + 1).ToString() + ":FM:STATe ON");
            afg3022Session.Write("SOURce" + (chNum + 1).ToString() + ":FM:INTernal:FREQuency " + freq1.ToString());
            afg3022Session.Write("SOURce" + (chNum + 1).ToString() + ":FM:Deviation " + freq2.ToString() + "Hz");

            } 
            else
                afg3022Session.Write("SOURce" + (chNum + 1).ToString() + ":FM:STATe OFF");

                return true;

        }
        public bool SetFmSquare(int chNum)
        {

            Thread.Sleep(100);
            //afg3022.WriteString("SOURce" + (chNum + 1).ToString() + ":FM:STATe ON");
            return true;

        }
        public bool SetFmFreq(int chNum, float freq)
        {
            bool ret;

            afg3022Session.Write("SOURce" + (chNum + 1).ToString() + ":FM:INTernal:FREQuency " + freq.ToString());
            ret = true;
            return ret;
        }
    

        public bool SetFmDevia(int chNum, float freq)
        {
            afg3022Session.Write("SOURce" + (chNum + 1).ToString() + ":FM:Deviation " + freq.ToString() + "Hz");
            return true;   
         
        }

        internal void Write(string text)
        {
            afg3022Session.Write(text);
        }
    }
}
