using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Correct
{
    public class ADBlock
    {

        Queue<Int16> queueAd = new Queue<Int16>();


        Int16[] dataSample;

        AutoResetEvent adEvent = new AutoResetEvent(false);


        int slideNum = 0;

        public ADBlock(int window, int slide)
        {
            this.Window = window;
            this.Slide = slide;
            this.dataSample = new Int16[window];
        }

        public ADBlock()
        { 
        }

        public int Window
        {
            get;
            private set;
        }

        public int Slide
        {
            get;
            private set;
        }



        public void PutAdData(Int16[] data,int offset,int length)
        {

            while (slideNum > 0)
            {
                if (queueAd.Count <= 0) break;

                queueAd.Dequeue();

                slideNum--;
            }
            for (int i = 0; i < length; i++)
            {
                if (slideNum > 0)
                {
                    slideNum--;
                   
                }
                else
                {
                    queueAd.Enqueue(data[offset + i]);
                    if (queueAd.Count == Window)
                    {
                        dataSample = queueAd.ToArray();

                        adEvent.Set();

                        slideNum = this.Slide;
                    }
                }
            }
        }

        public Int16[] GetAdData(int timeout)
        {
            if (timeout == 0) return (Int16[])dataSample.Clone();
            if (timeout > 0)
            {
                if (adEvent.WaitOne(timeout, false)==false)
                {
                    return null;
                }
            }
            else
            {
                adEvent.WaitOne();
            }
            return (Int16[])dataSample.Clone();
        }
        

    }
}
