using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Correct
{
    class DSP
    {
        FFTPlan plan;

        float[] dataIn;
        float[] dataOut;
        int FFTNUM;
        public DSP(int num)
        {
            plan = new FFTPlan(num);
            dataIn=new float[num*2];
            dataOut=new float[num*2];
            this.FFTNUM = num;
        }


        public float CalAmpl(Int16[] data, int freq)
        {
            float ampl = 0;
            for (int i = 0; i < data.Length; i++)
            {
                dataIn[2 * i] = data[i];
                dataIn[2 * i + 1] = 0;
            }
            plan.FFTForward(dataIn, dataOut);

            int startIndex = freq ;
            int endIndex = freq;
            if (freq == 0)
            {

                return dataOut[0] / FFTNUM;
            
            }


            if (freq < 450)
            {

                startIndex = freq - 2 < 0 ? 0 : freq - 2;
                endIndex = freq + 2;

            }
            else if (freq < 1000)
            {
                startIndex = freq - 150 < 0 ? 0 : freq - 150;
                endIndex = freq + 150;
            }
            else if (freq < 3000)
            {
                startIndex = freq - 200 < 0 ? 0 : freq - 200;
                endIndex = freq + 200;
            }

            for (int i = startIndex; i <= endIndex; i++)
            {
                ampl += (dataOut[2 * i] * dataOut[2 * i] + dataOut[2 * i + 1] * dataOut[2 * i + 1]);
            }
            ampl = (float)(Math.Sqrt(ampl) * 2 / FFTNUM/Math.Sqrt(2));
            return ampl;
        }


    }
}
