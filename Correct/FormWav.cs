using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UltraChart;
using System.Threading;

namespace Correct
{
    public partial class FormWav : Form
    {

        Device device;

        List<ChartGraph> listChart = new List<ChartGraph>();

        List<Thread> listThread = new List<Thread>();
        List<bool> listEnable = new List<bool>();

        public FormWav()
        {
            InitializeComponent();


            listChart.Add(chartGraph1);
            listChart.Add(chartGraph2);
            listChart.Add(chartGraph3);

            listEnable.Add(true);
            listEnable.Add(true);
            listEnable.Add(true);
        }


        public FormWav(Device dev)
            : this()
        {

            this.device = dev;


            for (int i = 0; i < listChart.Count; i++)
            {
                ChartGraph chart = listChart[i];
                UltraChart.CurveGroup grp = chart.AddNewGroup();
                grp.XAxes.MaxScale = 100000000L;
                grp.XAxes.MinScale = 100;
                grp.XAxes.SetScale(100000);
                grp.XAxes.SetOrgTime(ChartGraph.DateTime2ChartTime(DateTime.Now), 0);
                grp.CursorType = CurveCursorType.CURSOR_CROSSLINE;
                grp.XAxes.XAxesMode = XAxesMode.Relative;

                grp.DrawPointFlagXAxesScale = 500;

            }

      
        }



        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            for (int i = 0; i < 3; i++)
            {
                Thread th = new Thread(new ParameterizedThreadStart(ProcADSample));
                th.IsBackground = true;
                th.Start(i);
                listThread.Add(th);
            }
        }


        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            for (int i = 0; i < listChart.Count-1; i++)
            {
                listChart[i].Height = (this.ClientSize.Height-20) / 3;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            for (int i = 0; i < listThread.Count; i++)
            {
                listThread[i].Abort();
            }
            listThread.Clear();
        }



        private void ProcADSample(object obj)
        {

            int channel = (int)obj;

            ADBlock block=new ADBlock(8000,4000);
            this.device.AddADBlock(channel, block);

            try
            {


                while (true)
                {
                   short[] adData= block.GetAdData(-1);

                   if (adData == null) continue;

                   if (listEnable[channel] == false) continue;

                   int adMin = int.MaxValue;
                   int adMax = int.MinValue;


                   for (int i = 0; i < adData.Length; i++)
                   {
                       if (adData[i] > adMax)
                       {
                           adMax = adData[i];
                       }
                       if (adData[i] < adMin)
                       {
                           adMin = adData[i];
                       }
                   }

                   float adMaxf = adMax + (adMax - adMin) * 0.2f;

                   float adMinf = adMin - (adMax - adMin) * 0.2f;

                       this.Invoke((EventHandler)delegate
                       {

                           ChartGraph chart = listChart[channel];
                           UltraChart.CurveGroup grp = chart.GroupList[0];
                           grp.ClearChartObject();

                           LineArea area = new LineArea(chart, "AD曲线", true);
                           area.IsShowFoldFlag = false;
                           area.IsFold = false;
                           area.YAxes.Mode = YAxesMode.Manual;
                           area.YAxes.YAxesMin = adMaxf;
                           area.YAxes.YAxesMax = adMinf;
                           area.YAxes.Precision = 3;
                           area.YAxes.UnitString = "";
                        

                           LineCurve lcAmpl = new LineCurve(chart, "原始波形", 0);

                           lcAmpl.LineColor = Color.Lime;
                           area.AddLine(lcAmpl);

                           DateTime timeNow = DateTime.Now;
                           long startTm = ChartGraph.DateTime2ChartTime(timeNow);
                           for (int j = 0; j < adData.Length; j++)
                           {

                               long tm = startTm + j * 1000000L / 8000;
                               // var tm = timeQuery.AddMilliseconds(j / 8.0);
                               lcAmpl.AddPoint(new LinePoint(tm, adData[j]));
                           }


                           grp.AddChartObject(area);
                           grp.XAxes.SetOrgTime(ChartGraph.DateTime2ChartTime(timeNow), 0);
                           chart.AutoSetXScale();


                           chart.Draw();

                       });



                }




            }
            catch (Exception ex)
            { 

            }
            finally {
                this.device.RemoveADBlock(channel,block);
            }

            


        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            listEnable[0] = !listEnable[0];
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            listEnable[1] = !listEnable[1];
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            listEnable[2] = !listEnable[2];
        }


    }
}
