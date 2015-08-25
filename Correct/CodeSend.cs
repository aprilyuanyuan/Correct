using System;
using System.Collections.Generic;
using System.Text;
using Lon.IO.Ports;

namespace Correct
{
    public interface ICodeSend
    {
        bool SetFreq(float freq);
        bool SetFreq(int chNo,float freq);
        bool SetAmpli(float ampli);
        bool SetAmpli(int chNo,float ampli);
        void SetState(bool statevalue);
        bool SetOffset(int chNum, float ampli);
        void CreateDC(int channel, float ampl);
        bool Write(String text);
       

    }
    public class CodeSend3022 : ICodeSend
    {
        FunGen dev = new FunGen();
        public void SetState(bool stateflag)
        {
            dev.Output(stateflag);
        }
        public bool SetOffset(int chNum, float ampli)
        {
            dev.SetVOLOffset(chNum, ampli);
            return true;
        }
        public void CreateDC(int channel, float ampl)
        {
            dev.CreateDC(channel, ampl);
           
        }

        public bool SetFreq(float frequency)
        {

            bool ret = true;
            ret = dev.SetFreq(0, frequency);
            if (frequency < 450)
            {
                dev.SetFmMode(0, false, 0, 0);

            }
            else if (frequency < 1000)
            {
                dev.SetFmMode(0, true, 16.8f, 55);
                //dev.SetFmMode(0, false, 0, 0);

            }
            else if (frequency < 3000)
            {
                dev.SetFmMode(0, true, 18.0f, 11);
                //dev.SetFmMode(0, false, 0, 0);

            }
            
           
    
            
            if (ret == false)
            {
                return ret;
            }
           

            return true;
        }
        public bool SetFreq(int chNo,float freq)
        {
            bool ret = true;
            ret = dev.SetFreq(chNo, freq);
            if (ret == false)
            {
                return ret;
            }
            //if (freq > 1600)
            //{
            //    ret = dev.SetFmDevia(chNo, 11);
            //    if (ret == false)
            //    {
            //        return ret;
            //    }
            //    dev.SetFmFreq(chNo, 10.3f);
            //    if (ret == false)
            //    {
            //        return ret;
            //    }
            //}
            //else
            //{
            //    dev.SetFmDevia(chNo, 55);
            //    if (ret == false)
            //    {
            //        return ret;
            //    }
            //    dev.SetFmFreq(chNo, 8.5f);
            //    if (ret == false)
            //    {
            //        return ret;
            //    }
            //}
            return true;
        }
        public bool SetAmpli(float ampli)
        {
            return dev.SetAmpli(0, ampli);
        }

        public bool SetAmpli(int chNo,float ampli)
        {
            return dev.SetAmpli(chNo, ampli);
        }


        public bool Write(String text)
        {
            dev.Write(text);
            return true;
        }
    }
}
