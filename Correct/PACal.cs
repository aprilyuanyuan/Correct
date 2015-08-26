using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace Correct
{
    /// <summary>
    /// 功放校准
    /// </summary>
    class PACal
    {
        CodeSend3022 sigSource;
        Meter meter;
        List<CalItem> listItemACA = new List<CalItem>();
        List<CalItem> listItemACV = new List<CalItem>();
        List<CalItem> listItemDCA = new List<CalItem>();
        List<CalItem> listItemDCV = new List<CalItem>();


        int mode = -1; //0-ACA 1-ACV 2-DCA 3-DCV 


        public PACal(CodeSend3022 sigSource,Meter meter)
        {
            this.sigSource = sigSource;
            this.meter = meter;
        }

        public void LoadCfg(string fileName)
        {
            if (File.Exists(fileName) == false) return;

            string[] lines = File.ReadAllLines(fileName);

            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrEmpty(lines[i])) continue;

                string line = lines[i];
                string[] paramList=line.Split(',');

                int mode =int.Parse(paramList[0]);

                int freq = int.Parse(paramList[1]);

                float input = float.Parse(paramList[2]);
                float output = float.Parse(paramList[3]);

                //0-ACA 1-ACV 2-DCA 3-DCV 
                switch (mode)
                {
                    case 0:
                        this.CalACA = true;
                        listItemACA.Add(new CalItem(freq, input, output));
                        break;
                    case 1:
                        this.CalACV = true;
                        listItemACV.Add(new CalItem(freq, input, output));
                        break;
                    case 2:
                        this.CalDCA = true;
                        listItemDCA.Add(new CalItem(freq, input, output));
                        break;
                    case 3:
                        this.CalDCV = true;
                        listItemDCV.Add(new CalItem(freq, input, output));
                        break;
                }


            }
        }

        public void StoreCfg(string fileName)
        {

            StringBuilder sb = new StringBuilder();
            List<List<CalItem>> listList = new List<List<CalItem>>() { listItemACA, listItemACV, listItemDCA, listItemDCV };
            for (int i = 0; i < 4; i++)
            {
                List<CalItem> list = listList[i];
                for (int j = 0; j < list.Count; j++)
                {
                    sb.AppendLine(string.Format("{0},{1},{2},{3}", i, list[j].Freq, list[j].Input, list[j].Output));
                }

            }

            File.WriteAllText(fileName, sb.ToString());
        }

      

        public bool CalACA
        {
            get;
            private set;
        }

        public bool CalACV
        {
            get;
            private set;
        }

        public bool CalDCA
        {
            get;
            private set;
        }

        public bool CalDCV
        {
            get;
            private set;
        }

        public void StartCalACA()
        {

            mode = 0;
            if (this.CalACA) return;
            listItemACA.Clear();
    
            sigSource.SetFreq(1000, false);

            meter.SetAcCurrent();
            Thread.Sleep(8000);

            for (int i = 0; i < 5; i++)
            {
                float ampl = (i + 1) * 0.2f;
                sigSource.SetAmpli(ampl);

                Thread.Sleep(5000);
                float meteVal = Meter.MeterReadingValue;
                

                listItemACA.Add(new CalItem(1000,ampl,meteVal));


            }

            listItemACA.Sort();
            CalACA = true;
        }

        public void StartCalACV()
        {

            mode = 1;
            if (this.CalACV) return;
            listItemACV.Clear();

            meter.SetAcVolt();
            Thread.Sleep(8000);

            float[] inputList = new float[] {0.01f,0.02f,0.05f,0.1f,0.2f,0.5f };

            int[] freqList = new int[] { 25,50,100,200,750,2000};

            for (int j = 0; j < freqList.Length; j++)
            {
                int freq = freqList[j];
                for (int i = 0; i < inputList.Length; i++)
                {
                    float ampl = inputList[i];
                    sigSource.SetFreq(freq, false);
                    sigSource.SetAmpli(ampl);

                    Thread.Sleep(5000);
                    float meteVal = Meter.MeterReadingValue;


                    listItemACV.Add(new CalItem(freq, ampl, meteVal));
                }

            }

            CalACV = true;
        }


        public void StartCalDCA()
        {

            mode = 2;
            if (this.CalDCA) return;
            listItemACV.Clear();

            sigSource.SetFreq(25, false);
            sigSource.SetAmpli(0.01f);
            meter.SetDcCurrent();
            Thread.Sleep(8000);

            for (int i = 0; i <5 ; i++)
            {
                float ampl = (i + 1);
                sigSource.SetOffset(1,ampl);

                Thread.Sleep(5000);
                float meteVal = Meter.MeterReadingValue;
                listItemDCA.Add(new CalItem(1000,ampl, meteVal));
            }

            listItemDCA.Sort();
            CalDCA = true;
        }

        public void StartCalDCV()
        {

            mode = 3;
            float[] value = {0.01f,0.02f,0.05f,0.1f,0.2f,0.3f,0.5f};
        
            if (this.CalDCV) return;
            listItemDCV.Clear();

            sigSource.SetFreq(25, false);

            meter.SetDcVolt();
            Thread.Sleep(8000);

            for (int i = 0; i < value.Length; i++)
            {
                float ampl = (value[i] + 1) * 1f;
                sigSource.SetAmpli(ampl);

                Thread.Sleep(5000);
                float meteVal = Meter.MeterReadingValue;
                listItemDCV.Add(new CalItem(0,ampl, meteVal));
            }

            listItemDCV.Sort();
            CalDCV = true;
        }



        public float CalInput(float freq, float output)
        {

        
            List<CalItem> listCal = new List<CalItem>();
            if (mode == 0)
                listCal = listItemACA;
            if (mode == 1)
                listCal = listItemACV;
            if (mode == 2)
                listCal = listItemDCA;
            if (mode == 3)
                listCal = listItemDCV;

            Dictionary<int, List<CalItem>> dicList = new Dictionary<int, List<CalItem>>();

            for (int i = 0; i < listCal.Count; i++)
            {

                int freqKey=listCal[i].Freq;
                if (dicList.ContainsKey(freqKey))
                {
                    dicList[freqKey].Add(listCal[i]);
                }
                else
                {
                    List<CalItem> items = new List<CalItem>();
                    items.Add(listCal[i]);
                    dicList.Add(freqKey, items);
                }
            }

            int keyFreq = -1;
            float freqDiff = float.MaxValue;

            
            foreach (int key in dicList.Keys)
            {
                if (Math.Abs(key - freq) < freqDiff)
                {
                    keyFreq = key;
                    freqDiff = Math.Abs(key - freq);
                }
            }


            int index = -1;
            List<CalItem> listSel = dicList[keyFreq];
            listSel.Sort();
            for (int i = 0; i < listSel.Count - 1; i++)
            {
                if (output < listSel[i].Output)
                {
                    break;
                }
                index = i;
            }

            if (index < 0)
            {
                return listSel[0].Input * output / listSel[0].Output;
            }
            else
            {
                float K = (listSel[index].Input - listSel[index + 1].Input) / (listSel[index].Output - listSel[index + 1].Output);
                float B = listSel[index].Input - K * listSel[index].Output;

                return K * output + B;
            }

        }

        public float CalInput(float output)
        {
            return CalInput(0, output);

        }



        class CalItem:IComparable
        {


            public CalItem(int freq, float input, float output)
            {
                this.Freq = freq;
                this.Input = input;
                this.Output = output;
            }


            public int Freq
            {
                get;
                private set;
            }

            public float Input
            {
                get;
                private set;
            }

            public float Output
            {
                get;
                private set;
            }

            #region IComparable 成员

            public int CompareTo(object obj)
            {
                CalItem other=obj as CalItem;
                return (this.Output > other.Output)?0:1;
            }

            #endregion
        }


    }
}
