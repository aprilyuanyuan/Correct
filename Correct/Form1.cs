using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraEditors.Repository;
using System.Threading;
using Lon.IO.Ports;
using System.IO;
using System.Collections;
using Lon.Util;
using System.Diagnostics;



namespace Correct
{
    //public delegate void gridView_EndSorting(object sender, EventArgs e);
    public partial class Form1 : Form, IRawDataShow
    {
        public const int AckTimeOut = 3000;
        Device dev = new Device();
        Thread threadReadMeter = null;
        User user = new User();
        public static Meter meter = new Meter();
        public byte DeviceFlag;
        public string DeviceType;
        public DateTime Operationtime = DateTime.Now;

        public DateTime ProductionData = new DateTime(2015, 6, 20);
        public Int16 SequenceofProducton = 01;

        delegate void DelegateShowText(Control ctl, string str);
        public List<float> FrequencyList = new List<float>();
        public List<Record> listDataSource = new List<Record>();
        public Dictionary<int, Record> dic = new Dictionary<int, Record>();
        public double[] Result = new double[3];

        DeviceInformation devinfo = new DeviceInformation();
        ChannelInformation ChannelInfo = new ChannelInformation();
        public bool IsRunning = false;
        Thread _runThread;
        AutoResetEvent FrameCalc = new AutoResetEvent(false);
        CodeSend3022 sigSource = new CodeSend3022();
        float AdValue0;
        float OffsetValue0;
        float AdValue1;
        float offsetValue1;
        float AdValue2;
        float OffsetValue2;

        public ChannelParam channelparamvalue = new ChannelParam();
        DSP dsp2 = new DSP(8000);


        DeviceInformation devSelfInfo = new DeviceInformation();

        PACal paCal = null;

        public Form1()
        {
            user.ShowDialog();

            InitializeComponent();


            Thread threadSHowData = new Thread(new ThreadStart(ShowRawData));
            threadSHowData.IsBackground = true;
            threadSHowData.Start();

            Thread threadShowData2 = new Thread(new ThreadStart(ShowRawData2));
            threadShowData2.IsBackground = true;
            threadShowData2.Start();

            Thread threadDealData0 = new Thread(new ThreadStart(DealData0));
            threadDealData0.IsBackground = true;
            threadDealData0.Start();

            Thread threadDealData1 = new Thread(new ThreadStart(DealData1));
            threadDealData1.IsBackground = true;
            threadDealData1.Start();

            Thread threadDealData2 = new Thread(new ThreadStart(DealData2));
            threadDealData2.IsBackground = true;
            threadDealData2.Start();

            ChannelInfo.InitChannelFlag();

            Thread threadSelfCheck = new Thread(new ThreadStart(SelfCheck));
            threadSelfCheck.IsBackground = true;
            threadSelfCheck.Start();

            paCal = new PACal(sigSource, meter);

            paCal.LoadCfg(Path.Combine(Application.StartupPath, "PACAL.txt"));
        }


        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            paCal.StoreCfg(Path.Combine(Application.StartupPath, "PACAL.txt"));
        }

        private void SelfCheck()
        {
            try
            {

                while (true)
                {
                    if (devSelfInfo.IsValid) break;

                    DeviceRequest req = dev.ReadDeviceInformation(2500);

                    dev.SendRequest(req);
                    if (req.WaitResponse(2500))
                    {
                        byte[] data = req.Response;
                        if (data == null || data.Length < 2) continue;
                        devSelfInfo.ParseData(data);
                        if (devSelfInfo.IsValid == false)
                        {
                            //设置类型

                            int devIndex = -1;
                            this.Invoke((EventHandler)delegate
                            {

                                using (FormType form = new FormType())
                                {
                                    form.ShowDialog();
                                    devIndex = form.DevIndex;
                                }
                            });


                            MemoryStream ms = new MemoryStream();
                            BinaryWriter bw = new BinaryWriter(ms);

                            bw.Write((byte)(devIndex + 1));

                            byte[] name = new byte[8];
                            byte[] tmp = Encoding.Default.GetBytes(user.Name);
                            if (tmp.Length > 8)
                            {
                                MessageBox.Show("用户名长度应小于4个汉字或者8个英文字母");
                            }
                            for (int i = 0; i < tmp.Length; i++)
                            {
                                name[i] = tmp[i];
                            }
                            bw.Write(name);
                            bw.Write((byte)(Operationtime.Year - 2000));
                            bw.Write((byte)Operationtime.Month);
                            bw.Write((byte)Operationtime.Day);

                            DeviceRequest req1 = dev.WriteDeviceInformation(ms.ToArray(), 2000);

                            dev.SendRequest(req1);


                        }
                        else
                        {
                            this.Invoke((EventHandler)delegate
                            {
                                toolStripComboBox6.SelectedIndex = devSelfInfo.DevIndex;
                                toolStripComboBox6.Visible = true;
                                toolStripLabel7.Visible = true;
                            });

                            break;
                        }
                    }
                    Thread.Sleep(2500);
                }
            }
            catch (Exception)
            {

            }

        }

        public static int BCD2Int(byte bcd)
        {
            int high = bcd >> 4;
            if (high > 9) return -1;
            int low = bcd & 0x0f;
            if (low > 9) return -1;
            return high * 10 + low;
        }

        private void InitGrid()
        {
            // advBandedGridView1是表格上的默认视图，注意这里声明的是：BandedGridView
            BandedGridView view = advBandedGridView1 as BandedGridView;
            view.BeginUpdate(); //开始视图的编辑，防止触发其他事件
            view.BeginDataUpdate(); //开始数据的编辑
            view.Bands.Clear();

            //添加列标题
            view.OptionsView.ShowColumnHeaders = false;                         //因为有Band列了，所以把ColumnHeader隐藏
            view.OptionsView.ShowGroupPanel = false;                            //如果没必要分组，就把它去掉
            view.OptionsView.EnableAppearanceEvenRow = false;                   //是否启用偶数行外观
            view.OptionsView.EnableAppearanceOddRow = true;                     //是否启用奇数行外观
            view.OptionsView.ShowFilterPanelMode = ShowFilterPanelMode.Never;   //是否显示过滤面板
            view.OptionsCustomization.AllowColumnMoving = false;                //是否允许移动列
            view.OptionsCustomization.AllowColumnResizing = false;              //是否允许调整列宽
            view.OptionsCustomization.AllowGroup = false;                       //是否允许分组
            view.OptionsCustomization.AllowFilter = false;                      //是否允许过滤
            view.OptionsCustomization.AllowSort = true;                         //是否允许排序
            view.OptionsSelection.EnableAppearanceFocusedCell = true;           //???
            view.OptionsBehavior.Editable = true;                               //是否允许用户编辑单元格
            view.Appearance.FocusedRow.BackColor = Color.DarkOrange;


            GridBand bandID = view.Bands.AddBand("ID");
            bandID.Visible = false; //隐藏ID列

            GridBand bandFrequency = view.Bands.AddBand("频点");
            GridBand bandGear = view.Bands.AddBand("档位");
            GridBand band0XActualValue1 = view.Bands.AddBand("实际值1");
            GridBand band0XADValue1 = view.Bands.AddBand("采样值1");
            //GridBand band0XActualValue2 = view.Bands.AddBand("实际值2");
            //GridBand band0XADValue2 = view.Bands.AddBand("采样值2");
            GridBand bandCalibrationParameter = view.Bands.AddBand("校正系数");
            GridBand bandOffsetValue = view.Bands.AddBand("偏置");
            GridBand bandDeviValue = view.Bands.AddBand("偏差%");

            //绑定数据源并显示
            gridControl1.DataSource = listDataSource;
            gridControl1.MainView.PopulateColumns();

            #region
            view.Columns["Frequency"].OwnerBand = bandFrequency;
            view.Columns["Gear"].OwnerBand = bandGear;
            view.Columns["ActualValue1"].OwnerBand = band0XActualValue1;
            view.Columns["Ad1"].OwnerBand = band0XADValue1;
            //view.Columns["ActualValue2"].OwnerBand = band0XActualValue2;
            //view.Columns["Ad2"].OwnerBand = band0XADValue2;
            view.Columns["CalibrationParameter"].OwnerBand = bandCalibrationParameter;
            view.Columns["Offset"].OwnerBand = bandOffsetValue;
            view.Columns["Deviation"].OwnerBand = bandDeviValue;

            view.Columns["Frequency"].OptionsColumn.AllowEdit = false;
            view.Columns["Frequency"].UnboundType = DevExpress.Data.UnboundColumnType.Integer;
            view.Columns["Frequency"].AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;

            view.Columns["Gear"].OptionsColumn.AllowEdit = false;
            view.Columns["Gear"].UnboundType = DevExpress.Data.UnboundColumnType.Integer;
            view.Columns["Gear"].AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;

            view.Columns["ActualValue1"].OptionsColumn.AllowEdit = false;
            view.Columns["ActualValue1"].AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;

            //view.Columns["ActualValue2"].OptionsColumn.AllowEdit = false;
            //view.Columns["ActualValue2"].AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;

            view.Columns["Ad1"].OptionsColumn.AllowEdit = false;
            view.Columns["Ad1"].UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
            view.Columns["Ad1"].AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            view.Columns["Ad1"].AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;


            //view.Columns["Ad2"].OptionsColumn.AllowEdit = false;
            //view.Columns["Ad2"].UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
            //view.Columns["Ad2"].AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            //view.Columns["Ad2"].AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;

            view.Columns["CalibrationParameter"].OptionsColumn.AllowEdit = false;
            view.Columns["CalibrationParameter"].UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
            view.Columns["CalibrationParameter"].AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;


            view.Columns["Offset"].OptionsColumn.AllowEdit = false;
            view.Columns["Offset"].UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
            view.Columns["Offset"].AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            view.Columns["Offset"].AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;

            view.Columns["Deviation"].OptionsColumn.AllowEdit = false;
            view.Columns["Deviation"].UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
            view.Columns["Deviation"].AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            view.Columns["Deviation"].AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;

            #endregion

            view.EndDataUpdate();//结束数据的编辑
            view.EndUpdate();   //结束视图的编辑
            view.OptionsBehavior.AutoSelectAllInEditor = true;

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string path = System.AppDomain.CurrentDomain.BaseDirectory;
            ComboBoxMethod();
            listDataSource = new List<Record>();
            //InitGrid();            

            List<SampleInterface> sampleInterfaces = SampleInterface.ReadFormXml(path + "ChannelInfo.xml");
            foreach (var module in sampleInterfaces)
            {
                this.toolStripComboBox6.Items.Add(module);
            }
            toolStripComboBox6.SelectedItem = sampleInterfaces[0];
        }

        private void ComboBoxMethod()
        {
            string[] portnames = SerialPort.GetPortNames();
            string[] geararray = { "0", "1", "2", "3" };
            for (int i = 0; i < portnames.Length; i++)
            {
                toolStripComboBox1.Items.Add(portnames[i]);  //获取系统所有串口信息函数。

                toolStripComboBox4.Items.Add(portnames[i]);

            }

            if (toolStripComboBox1.Items.Count > 0)
            {
                toolStripComboBox1.SelectedIndex = 0;
            }

            if (toolStripComboBox4.Items.Count > 0)
            {
                toolStripComboBox4.SelectedIndex = 0;
            }

            toolStripComboBox2.Items.AddRange(new string[] { "9600", "19200", "38400", "57600", "115200", "576000" });
            toolStripComboBox2.Text = "576000";

            comboBox1.Items.AddRange(new string[] { "0", "1", "2" });
            comboBox1.Text = "0";


            comboBox3.Items.Add("0");
            comboBox3.Items.Add("1");
            comboBox3.Items.Add("2");
            comboBox3.Items.Add("3");
            comboBox3.Items.Add("4");
            comboBox3.Text = "0";
            comboBox2.Items.Add("打开");
            comboBox2.Items.Add("关闭");
            comboBox2.Text = "打开";
            textBox7.Text = "25";
            textBox3.Text = "0";

        }




        private void btnOpenMeter_Click_1(object sender, EventArgs e)
        {
            //if (threadReadMeter != null)
            //{
            //    threadReadMeter.Abort();
            //}
            //threadReadMeter = new Thread(new ThreadStart(ProcReadMeterVal));
            //threadReadMeter.IsBackground = true;
            //threadReadMeter.Start();
            if (toolStripComboBox4.Text != "")
            {
                if (meter.IsOpen() == false)
                {
                    btnOpenMeter.Text = "关闭串口";
                    btnOpenMeter.ToolTipText = "关闭串口";
                    toolStripComboBox4.Enabled = false;
                    meter.PortName = toolStripComboBox4.Text;
                    meter.Run();
                }
                else
                {
                    toolStrip1.SuspendLayout();
                    btnOpenMeter.Image = Properties.Resources.Port;
                    btnOpenMeter.Text = "打开串口";
                    btnOpenMeter.ToolTipText = "打开串口";
                    toolStripComboBox4.Enabled = true;
                    try
                    {
                        meter.Stop();
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        private void ProcReadMeterVal()
        {

        }

        public float GetValue()
        {
            return dev.Ampli;
        }



        PortDataDisplay m_portDispl = new PortDataDisplay();

        private void m_FormClosing(object sender, FormClosingEventArgs e)
        {
            string PortName = toolStripComboBox1.Text;
            string baudstring = toolStripComboBox2.Text;
            if (PortName != "" && toolStripComboBox2.Text != "")
            {
                int baud = Convert.ToInt32(baudstring);
                m_portDispl.InitSerialPort(PortName, baud);
                m_portDispl.DisconnectDeveice();
            }
            string PortName2 = toolStripComboBox4.Text;
            if (PortName2 != "")
            {
                m_portDispl.InitSerialPort2(PortName2);
                m_portDispl.DisconnectDeveice();
            }
            //base.OnFormClosing(e);           
        }



        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            try
            {
                string[] portnames = SerialPort.GetPortNames();

                toolStripComboBox1.Items.Clear();
                for (int i = 0; i < portnames.Length; i++)
                {
                    toolStripComboBox1.Items.Add(portnames[i]);  //获取系统所有串口信息函数。                   
                }
                string PortName = toolStripComboBox1.Text;
                int baud = Convert.ToInt32(toolStripComboBox2.Text);

                if (toolScripButton1.Text == "打开串口")
                {
                    dev.IniPort(PortName, baud);
                    toolScripButton1.Text = "关闭串口";
                    toolScripButton1.ToolTipText = "关闭串口";
                    toolStripComboBox1.Enabled = false;
                    toolStripComboBox2.Enabled = false;
                }
                else
                {
                    //m_portDispl.InitSerialPort(PortName,baud);
                    //m_portDispl.DisconnectDeveice();
                    dev.ClosePort();

                    toolScripButton1.Text = "打开串口";
                    toolScripButton1.ToolTipText = "打开串口";
                    toolStripComboBox1.Enabled = true;
                    toolStripComboBox2.Enabled = true;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Meter_AcSet(object sender, EventArgs e)
        {
            meter.SetAcVolt();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            meter.SetDcVolt();
        }

        public void ShowText(Control ctl, string str)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new DelegateShowText(ShowText), new object[] { ctl, str });
            }
            else
            {
                ctl.Text = str;
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e) //读取设备信息
        {
            DeviceRequest req = dev.ReadDeviceInformation(AckTimeOut);
            req.Callback += new EventHandler(req_ReadDeviceInformation);
            dev.SendRequest(req);
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e) //写入设备信息
        {
            SampleInterface sa = toolStripComboBox6.SelectedItem as SampleInterface;

            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            if (sa == null)
            {
                return;
            }

            DeviceFlag = sa.DeviceInfo;
            bw.Write(DeviceFlag);
            bw.Write((byte)(Operationtime.Year - 2000));
            bw.Write((byte)(Operationtime.Month));
            bw.Write((byte)(Operationtime.Day));
            bw.Write((byte)0);
            bw.Write((byte)0);


            byte[] name = new byte[8];
            byte[] tmp = Encoding.Default.GetBytes(user.Name);
            if (tmp.Length > 8)
            {
                MessageBox.Show("用户名长度应小于4个汉字或者8个英文字母");
            }
            for (int i = 0; i < tmp.Length; i++)
            {
                name[i] = tmp[i];
            }
            bw.Write(name);
            bw.Write((byte)(Operationtime.Year - 2000));
            bw.Write((byte)Operationtime.Month);
            bw.Write((byte)Operationtime.Day);


            using (DeviceInfo devinfo2 = new DeviceInfo())
            {
                devinfo2.ShowDialog();

                bw.Write((byte)(devinfo2.ProductionDate.Year - 2000));
                bw.Write((byte)(devinfo2.ProductionDate.Month));
                bw.Write((byte)(devinfo2.ProductionDate.Day));
                if (devinfo2.ProductionSequence == null)
                {
                    bw.Write((byte)0);
                }
                else
                {
                    bw.Write((byte)devinfo2.ProductionSequence.Length);
                    bw.Write(Encoding.Default.GetBytes(devinfo2.ProductionSequence));
                }
               
            }

            DeviceType = sa.DeviceType;
            byte[] devicetypearray = Encoding.Default.GetBytes(DeviceType);
            bw.Write((byte)devicetypearray.Length);
            bw.Write(devicetypearray);


            DeviceRequest req = dev.WriteDeviceInformation(ms.ToArray(), 2000);
            req.Callback += new EventHandler(req_Callback);
            dev.SendRequest(req);

        }

        private void 设置时钟ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeviceRequest req = dev.SetTime();
            req.Callback += new EventHandler(req_Callback);
            dev.SendRequest(req);
        }

        private void 读取当前时钟ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeviceRequest req = dev.GetTimeNow();
            req.Callback += new EventHandler(req_ReadTime);
            dev.SendRequest(req);
        }

        void req_ReadTime(object sender, EventArgs e) //响应读取时钟
        {
            DeviceRequest req = sender as DeviceRequest;
            byte[] response = req.Response;
            if (response[6] == 0)
            {
                string time = (2000 + response[7]) + "-" + response[8] + "-" + response[9] + " " + response[10] + ":" + response[11] + ":" + response[12];
                MessageBox.Show(time);
            }
            else if (response[6] == 1)
            {
                string time = (2000 + response[7]) + "-" + response[8] + "-" + response[9] + " " + response[10] + ":" + response[11] + ":" + response[12];
                MessageBox.Show(time);
            }
            else if (response[6] == 0xFF)
            {
                MessageBox.Show("读取失败！");
            }
            else
            {
                MessageBox.Show("返回信息有误！");
            }
        }
        void req_Callback(object sender, EventArgs e) //响应写入信息
        {
            DeviceRequest req = sender as DeviceRequest;
            byte[] response = req.Response;
            if (response[6] == 0)
            {
                MessageBox.Show("写入成功！");
            }
            else if (response[6] == 0xFF)
            {
                MessageBox.Show("写入失败！");
            }
            else
            {
                MessageBox.Show("返回信息有误！");
            }
        }

        void req_Callback2(object sender, EventArgs e) //响应写入信息
        {
            DeviceRequest req = sender as DeviceRequest;
            byte[] response = req.Response;
            if (response[6] == 0)
            {
                //MessageBox.Show("写入成功！");
            }
            else if (response[6] == 0xFF)
            {
                MessageBox.Show("写入失败！");
            }
            else
            {
                MessageBox.Show("返回信息有误！");
            }
        }

        void req_GetVersionInformation(object sender, EventArgs e)
        {
            DeviceRequest req = sender as DeviceRequest;
            byte[] response = req.Response;
            string deviceinformation = devinfo.DeviceVersionInformation(response);
            MessageBox.Show(deviceinformation);

        }

        void req_ReadDeviceInformation(object sender, EventArgs e)
        {
            DeviceRequest req = sender as DeviceRequest;
            byte[] response = req.Response;
            string deviceinformation = devinfo.InitDeviceInformation(response);
            MessageBox.Show(deviceinformation);

        }

        void req_ReadSpan(object sender, EventArgs e)
        {
            DeviceRequest req = sender as DeviceRequest;
            byte[] response = req.Response;
            if (response[6] != 0xff)
            {
                string channelid = response[6].ToString() + "\r\n";
                string channelgear = response[7].ToString() + "\r\n";
                MessageBox.Show("通道号:" + channelid + "档位:" + channelgear);
            }
            else
            {
                MessageBox.Show("读取量程失败!");
            }

        }
        private void 读取上次设置时钟ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeviceRequest req = dev.ReadLastTime();
            req.Callback += new EventHandler(req_ReadTime);
            dev.SendRequest(req);
        }

        private void 设置量程ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte channelid = Convert.ToByte(comboBox1.Text);
            byte gear = Convert.ToByte(comboBox3.Text);
            DeviceRequest req = dev.SetSpan(channelid, gear, 2000);
            req.Callback += new EventHandler(req_Callback);
            dev.SendRequest(req);
        }


        public String CurrRunFileName
        {
            get;
            private set;
        }
        private List<String> cmdList = new List<String>();

        private void button12_Click(object sender, EventArgs e) //读取脚本文件
        {
            if (_runThread == null || (_runThread.IsAlive == false))
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Multiselect = false;


                if (ofd.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                else
                {
                    string path = ofd.FileName;
                    this.button12.Text = "停止脚本";
                    String[] lines = File.ReadAllLines(path, Encoding.Default);
                    cmdList.Clear();
                    for (int i = 0; i < lines.Length; i++)
                    {
                        var cmd = lines[i].Trim();
                        if (String.IsNullOrEmpty(cmd))
                        {
                            continue;
                        }
                        if (cmd.StartsWith(";") || cmd.StartsWith("//")) continue; //去掉注释
                        cmdList.Add(cmd);
                    }
                    _runThread = new Thread(new ThreadStart(Start));
                    _runThread.IsBackground = true;
                    _runThread.Start();
                    IsRunning = true;
                    if (IsRunning == true)
                    {
                        label5.Text = "正在运行脚本文件";
                    }
                }
            }
            else
            {
                _runThread.Abort();
                this.button12.Text = "运行脚本";
                label5.Text = "";
            }

        }




        public void Start()
        {

            DateTime timeStart = DateTime.Now;
            float Magnification = -1;
            byte LastGear = 4;
            byte PresentGear = 0;
            string Lastsigflag = "";
            string Presentsigflag = "";

            float b = 0;

            int channelid = 0;
            byte deviceflag = 0;
            listDataSource.Clear();
            if (cmdList == null)
            {
                return;
            }

            int count = this.cmdList.Count;
            this.Invoke((EventHandler)delegate
            {
                channelid = Convert.ToInt32(comboBox1.Text);
                SampleInterface sa = toolStripComboBox6.SelectedItem as SampleInterface;
                deviceflag = sa.DeviceInfo;
                InitGrid();
            });
            sigSource.SetState(true);


            int mode = -1;

            for (int i = 0; i < count; i++)
            {
                var args = this.cmdList[i].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                Presentsigflag = args[0];
                Int16 frequency = Convert.ToInt16(args[1]);
                sigfreq = frequency;

                switch (Presentsigflag)
                {
                    case "交流电流":
                        if (mode != 0)
                        {
                            MessageBox.Show("快点改到【交流电流】测试模式!再点击我。不要提前点击！！");
                            meter.SetAcCurrent();
                            Thread.Sleep(10000);
                        }
                        mode = 0;
                        paCal.StartCalACA();
                        break;
                    case "交流电压":
                        if (mode != 1)
                        {
                            MessageBox.Show("快点改到【交流电压】测试模式!再点击我。不要提前点击！！");
                            meter.SetAcVolt();

                            Thread.Sleep(10000);
                        }
                        mode = 1;
                        paCal.StartCalACV();
                        break;

                    case "直流电流":
                        //sigSource.SetState(false);
                        if (mode != 2)
                        {
                            MessageBox.Show("快点改到【直流电流】测试模式!再点击我。不要提前点击！！");
                            meter.SetDcCurrent();
                        }
                        //Thread.Sleep(15000);
                        mode = 2;
                        paCal.StartCalDCA();
                        break;
                    case "直流电压":
                        //sigSource.SetState(false);
                        if (mode != 3)
                        {

                            MessageBox.Show("快点改到【直流电压】测试模式!再点击我。不要提前点击！！");
                            meter.SetDcVolt();

                        }
                        //Thread.Sleep(15000);
                        mode = 3;
                        paCal.StartCalDCV();
                        break;
                }




                byte gear = Convert.ToByte(args[2]);
                float ActualValue1 = ToFloat(args[3], float.NaN);
                float Ad1 = ToFloat(args[4], float.NaN);

                Int16 calibrationParameter = Convert.ToInt16(args[5]);
                Int16 offset = Convert.ToInt16(args[6]);
                float deviation = ToFloat(args[7], float.NaN);



                PresentGear = gear;
                if (deviceflag == 7 && PresentGear != LastGear) //设置档位
                {
                    LastGear = PresentGear;
                    DeviceRequest req = dev.SetSpan((byte)channelid, PresentGear, 2000);
                    req.Callback += new EventHandler(req_Callback2);
                    dev.SendRequest(req);
                }


                try
                {
                    Record record = new Record(frequency, gear, ActualValue1, Ad1, calibrationParameter, offset, deviation);
                    if (Presentsigflag == "直流电压" || Presentsigflag == "直流电流")
                    {
                        sigSource.SetState(false);

                        Thread.Sleep(15000);

                        if (channelid == 0)
                        {
                            record.Offset = OffsetValue0;
                            b = OffsetValue0;
                        }
                        else if (channelid == 1)
                        {
                            record.Offset = offsetValue1;
                            b = offsetValue1;
                        }
                        else if (channelid == 2)
                        {
                            record.Offset = OffsetValue2;
                            b = OffsetValue2;
                        }
                        //record.CalibrationParameter = record.ActualValue1 / (record.Ad1 - b);
                        //b = -record.CalibrationParameter * b;
                        //record.Offset = b;
                        sigSource.SetState(true);

                    }

                    if (Presentsigflag == "交流电压" || Presentsigflag == "交流电流" || Presentsigflag == "直流电压")
                    {
                        sigSource.SetFreq(frequency);
                        sigSource.SetAmpli(paCal.CalInput(frequency, ActualValue1));
                    }
                    else if (Presentsigflag == "直流电流")
                    {
                        sigSource.SetFreq(frequency);
                        sigSource.SetAmpli(0.01f);
                        sigSource.SetOffset(0, paCal.CalInput(ActualValue1));
                    }

                    if (Presentsigflag == "直流电压" || Presentsigflag == "直流电流")
                    {
                        record.Frequency = 0;
                    }

                    Thread.Sleep(5000);
                    record.ActualValue1 = Meter.MeterReadingValue;

                    if (Presentsigflag == "交流电压" || Presentsigflag == "交流电流")
                    {
                        if (channelid == 0)
                        {
                            record.Ad1 = AdValue0;
                        }
                        else if (channelid == 1)
                        {
                            record.Ad1 = AdValue1;
                        }
                        else if (channelid == 2)
                        {
                            record.Ad1 = AdValue2;
                        }
                    }
                    else if (Presentsigflag == "直流电压" || Presentsigflag == "直流电流")
                    {
                        if (channelid == 0)
                        {
                            record.Ad1 = OffsetValue0;
                        }
                        else if (channelid == 1)
                        {
                            record.Ad1 = offsetValue1;
                        }
                        else if (channelid == 2)
                        {
                            record.Ad1 = OffsetValue2;
                        }
                    }


                    float CalibrationParametervalue1 = record.ActualValue1 / record.Ad1;
                    record.CalibrationParameter = CalibrationParametervalue1;

                    if (Presentsigflag == "交流电压" || Presentsigflag == "交流电流")
                    {
                        b = 0;
                        record.Offset = 0;
                    }
                    else if (Presentsigflag == "直流电压" || Presentsigflag == "直流电流")
                    {

                        record.CalibrationParameter = record.ActualValue1 / (record.Ad1 - b);
                        b = -record.CalibrationParameter * b;
                        record.Offset = b;

                    }


                    if (Presentsigflag == "交流电压" || Presentsigflag == "交流电流" || Presentsigflag == "直流电压")
                    {
                        sigSource.SetFreq(frequency);
                        sigSource.SetAmpli(paCal.CalInput(frequency, ActualValue1 * 4 / 3));
                    }
                    else if (Presentsigflag == "直流电流")
                    {
                        sigSource.SetFreq(frequency);
                        sigSource.SetAmpli(0.01f);
                        sigSource.SetOffset(0, paCal.CalInput(ActualValue1 * 4 / 3));
                    }
                    Thread.Sleep(5000);

                    float x = Meter.MeterReadingValue;

                    float z = 0;
                    if (Presentsigflag == "交流电压" || Presentsigflag == "交流电流")
                    {
                        if (channelid == 0)
                        {
                            z = AdValue0;
                        }
                        else if (channelid == 1)
                        {
                            z = AdValue1;
                        }
                        else if (channelid == 2)
                        {
                            z = AdValue2;
                        }
                    }
                    else if (Presentsigflag == "直流电压" || Presentsigflag == "直流电流")
                    {
                        if (channelid == 0)
                        {
                            z = OffsetValue0;
                        }
                        else if (channelid == 1)
                        {
                            z = offsetValue1;
                        }
                        else if (channelid == 2)
                        {
                            z = OffsetValue2;
                        }
                    }
                    float y = record.CalibrationParameter * z + b;
                    float dy = Math.Abs(x - y);
                    record.Deviation = dy * 100 / x;

                    if (record.Deviation > 10)
                    {
                        i--;
                        continue;
                    }

                    listDataSource.Add(record);

                }
                catch (Exception ex)
                {
                    NetDebugConsole.WriteLine(ex.ToString());
                }
                this.Invoke((EventHandler)delegate
                {
                    InitGrid();
                });

            }
            this.Invoke((EventHandler)delegate
            {
                label5.Text = "";
                this.button12.Text = "运行脚本";
            });

            IsRunning = false;

            DateTime timeStop = DateTime.Now;


            MessageBox.Show("校准完成!所需时间(Min):" + (timeStop - timeStart).TotalMinutes);
        }

        public static float ToFloat(string val, float defVal)
        {
            if (val == "非数字")
            {
                return float.NaN;
            }
            try
            {
                return Convert.ToSingle(val);
            }
            catch (Exception e)
            {
                Trace.TraceWarning(string.Format("数据转换格式错误：val: {0}  {1}", val, e.Message));
                return defVal;
            }
        }
        private void toolStripButton1_Click_1(object sender, EventArgs e) // 读取通道信息
        {
            byte channeltype;
            var mk = this.toolStripComboBox6.SelectedItem as SampleInterface;
            byte channelid = Convert.ToByte(comboBox1.Text);
            DeviceRequest req = dev.ReadChannelInformation(channelid);

            dev.SendRequest(req);

            if (req.WaitResponse(1000))
            {
                byte[] response = req.Response;
                if (mk.Channels.Count == 2 && channelid == 2)
                {
                    channeltype = mk.Channels[channelid - 1].channeltype.TypeID;
                }
                else
                {
                    channeltype = mk.Channels[channelid].channeltype.TypeID;
                }
                if (channeltype >= 70)
                {
                    ChannelParam param = new ChannelParam(response, 6);
                    channelparamvalue = param;
                    dev.UpdateChannelParam(param);
                    FormCalParam form = new FormCalParam(param);
                    {
                        form.Show();
                    }
                }
                else
                {
                    ChannelParam param = new ChannelParam(response, 6, true);
                    dev.UpdateChannelParam(param);
                    channelparamvalue = param;
                    FormCalParam form = new FormCalParam(param);
                    {
                        form.Show();
                    }
                }
            }

        }

        private void 读取当前量程ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte channelid = Convert.ToByte(comboBox1.Text);
            DeviceRequest req = dev.ReadSpan(channelid);
            req.Callback += new EventHandler(req_ReadSpan);
            dev.SendRequest(req);
        }


        void ShowRawData()
        {
            FrameMonitor monitor = dev.AddMonitor();
            while (true)
            {
                SerialFrame sf = monitor.GetFrame(100);

                StringBuilder sb = new StringBuilder();

                if (sf == null) continue;

                if (sf.RawData[5] == 0x15)
                {
                    continue;
                }
                string txt = sf.ToString();


                this.Invoke(new MethodInvoker(delegate
                {

                    txtSerialRxData.AppendText(txt);

                }));
            }
        }


        void ShowRawData2() //抓数据包
        {
            if (!checkBox1.Checked)
            {
                return;
            }
            byte[] DataBuffer = new byte[] { };
            byte[] ResultData = new byte[] { };
            Stopwatch sw = new Stopwatch();
            FrameMonitor monitor = dev.AddMonitor();
            int sumBytes = 0;
            FileStream fs = null;
            while (true)
            {
                SerialFrame sf = monitor.GetFrame(100);
                if (sf == null) continue;

                byte[] data = sf.RawData;
                int dataLength = data[3] + (data[4] << 8);//数据长度

                if (data[5] != 0x15) continue;
                if (data[6] != 2) continue; //通道0数据


                int adLength = dataLength - 6;

                if (sw.IsRunning == false)
                {
                    sw.Start();
                    sumBytes = adLength;
                    DateTime timeNow = DateTime.Now;
                    string fileName = timeNow.Hour + "-" + timeNow.Minute + "-" + timeNow.Second + ".bin";
                    fs = new FileStream(Path.Combine(Application.StartupPath, fileName), FileMode.OpenOrCreate);

                    for (int i = 11 + 1; i < sumBytes + 11; i += 2)
                    {
                        data[i] = (byte)(data[i] & 0x0f);
                    }

                    fs.Write(data, 11, adLength);
                }
                else
                {
                    sumBytes += adLength;

                    for (int i = 11 + 1; i < adLength + 11; i += 2)
                    {
                        data[i] = (byte)(data[i] & 0x0f);
                    }

                    fs.Write(data, 11, adLength);

                }

                long elapsedTm = sw.ElapsedMilliseconds;

                if (elapsedTm >= 60 * 1000)
                {
                    Console.WriteLine(sumBytes * 1000f / elapsedTm / 2);

                    fs.Close();

                    sw.Stop();
                    sw.Reset();
                }
            }
        }

        float Dft(Int16[] x, float f, int n)
        {
            float w0, w1, w2, xs;
            int i;

            w0 = 0;
            w1 = 0;
            w2 = 0;
            xs = Convert.ToSingle(2 * Math.Cos(2 * Math.PI * f));
            for (i = 0; i < n; i++)
            {
                w0 = w1;
                w1 = w2;
                w2 = xs * w1 - w0 + x[i];
            }

            return Convert.ToSingle(Math.Sqrt(w2 * w2 + w1 * w1 - xs * w2 * w1) * 2.0f / n);
        }

        List<float> amplArray0 = new List<float>();
        float value0;
        float amplvalue0;
        float amplshow0;
        int freqnumber0;
        public void DealData0()
        {
            ADBlock adBlock = dev.GetADBlock(0); //通道0          

            while (true)
            {
                int gear = 0;
                byte channelid = 4;
                float AC = 1f;
                float DC = 1f;
                Int16[] adData0 = adBlock.GetAdData(5000);

                if (adData0 != null)
                {
                    float ampl0 = dsp2.CalAmpl(adData0, (int)sigfreq);
                    OffsetValue0 = dsp2.CalAmpl(adData0, 0);
                    AdValue0 = ampl0;
                    if (checkBox5.Checked)
                    {
                        amplArray0.Add(ampl0);
                        this.Invoke((EventHandler)delegate
                        {
                            gear = Convert.ToByte(textBox3.Text);
                            channelid = Convert.ToByte(comboBox1.Text);
                            var mk = this.toolStripComboBox6.SelectedItem as SampleInterface;

                            if (channelid == 0)
                            {
                                if (mk.Channels[channelid].channeltype.TypeID == 22 || mk.Channels[channelid].channeltype.TypeID == 31 || mk.Channels[channelid].channeltype.TypeID == 32 || mk.Channels[channelid].channeltype.TypeID == 52 || mk.Channels[channelid].channeltype.TypeID == 70 || mk.Channels[channelid].channeltype.TypeID == 71 || mk.Channels[channelid].channeltype.TypeID == 72)
                                {
                                    AC = (float)(Convert.ToInt32(mk.Channels[channelid].gears[gear].Name) / Math.Sqrt(2));
                                }
                                else
                                    AC = Convert.ToSingle(mk.Channels[channelid].gears[gear].Name);
                            }

                        });
                    }
                    else if (checkBox4.Checked)
                    {
                        amplArray0.Add(OffsetValue0);
                        this.Invoke((EventHandler)delegate
                        {
                            gear = Convert.ToByte(textBox3.Text);
                            channelid = Convert.ToByte(comboBox1.Text);
                            var mk = this.toolStripComboBox6.SelectedItem as SampleInterface;
                            if (channelid == 0)
                            {
                                DC = Convert.ToSingle(mk.Channels[channelid].gears[gear].Name);
                            }
                        });
                    }
                    if (amplArray0.Count == 1 && channelid == 0)
                    {
                        for (int k = 0; k < channelparamvalue.CalcList.Count; k++)
                        {
                            if (channelparamvalue.CalcList[k].Freq == sigfreq && channelparamvalue.CalcList[k].Gear == gear)
                            {
                                freqnumber0 = k;
                                break;
                            }
                        }
                        if (channelparamvalue.CalcList.Count != 0)
                        {
                            amplvalue0 = (amplArray0.Sum() * channelparamvalue.CalcList[freqnumber0].CoeffK + channelparamvalue.CalcList[freqnumber0].CoeffB) / 1;

                            if (checkBox5.Checked)
                            {
                                amplshow0 = Math.Abs(Meter.MeterReadingValue - amplvalue0) / AC;
                            }
                            else if (checkBox4.Checked)
                            {
                                amplshow0 = Math.Abs(Meter.MeterReadingValue - amplvalue0) / DC;
                            }
                        }
                        //amplArray0.Clear();
                    }
                    try
                    {
                        this.Invoke((EventHandler)delegate
                        {

                            float realAC = dev.CalRealVal(dev.GetGear(0), 0, sigfreq, AdValue0);
                            float realDC = dev.CalRealVal(dev.GetGear(0), 0, 0, OffsetValue0);

                            textBox1.Text = AdValue0.ToString("0.000") + "\r\n" + realAC.ToString("0.000"); //交流 
                            textBox4.Text = OffsetValue0.ToString("0.000") + "\r\n" + realDC.ToString("0.000"); //直流
                            if (comboBox1.Text == "0")
                            {
                                textBox10.Text = (100 * amplshow0).ToString("0.000") + "%";
                                textBox3.Text = dev.GetGear(0).ToString();
                                textBox9.Text = Meter.MeterReadingValue.ToString();
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                    }
                    finally
                    {
                        amplArray0.Clear();
                    }


                }

                else
                {
                    try
                    {
                        this.Invoke((EventHandler)delegate
                        {
                            textBox1.Text = "超时";
                            textBox4.Text = "超时";
                        });
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }
        List<float> amplArray1 = new List<float>();
        float amplvalue1;
        float amplshow1;
        int freqnumber1;
        public void DealData1()
        {
            ADBlock adBlock = dev.GetADBlock(1); //通道1          

            while (true)
            {
                int gear = 0;
                byte channelid = 4;
                float AC = 1f;
                float DC = 1f;
                Int16[] adData1 = adBlock.GetAdData(5000);

                if (adData1 != null)
                {
                    float ampl1 = dsp2.CalAmpl(adData1, (int)sigfreq);
                    offsetValue1 = dsp2.CalAmpl(adData1, 0);
                    AdValue1 = ampl1;

                    if (checkBox5.Checked)
                    {
                        amplArray1.Add(ampl1);
                        this.Invoke((EventHandler)delegate
                        {
                            gear = Convert.ToByte(textBox3.Text);
                            channelid = Convert.ToByte(comboBox1.Text);
                            if (channelid == 1)
                            {
                                var mk = this.toolStripComboBox6.SelectedItem as SampleInterface;

                                if (mk.Channels[channelid].channeltype.TypeID == 22 || mk.Channels[channelid].channeltype.TypeID == 31 || mk.Channels[channelid].channeltype.TypeID == 32 || mk.Channels[channelid].channeltype.TypeID == 52 || mk.Channels[channelid].channeltype.TypeID == 70 || mk.Channels[channelid].channeltype.TypeID == 71 || mk.Channels[channelid].channeltype.TypeID == 72)
                                {
                                    AC = (float)(Convert.ToInt32(mk.Channels[channelid].gears[gear].Name) / Math.Sqrt(2));
                                }
                                else
                                    AC = Convert.ToSingle(mk.Channels[channelid].gears[gear].Name);
                            }
                        });
                    }
                    else if (checkBox4.Checked)
                    {
                        amplArray1.Add(offsetValue1);
                        this.Invoke((EventHandler)delegate
                        {
                            gear = Convert.ToByte(textBox3.Text);
                            channelid = Convert.ToByte(comboBox1.Text);
                            if (channelid == 1)
                            {
                                var mk = this.toolStripComboBox6.SelectedItem as SampleInterface;
                                DC = Convert.ToSingle(mk.Channels[channelid].gears[gear].Name);
                            }
                        });
                    }

                    if (amplArray1.Count == 1 && channelid == 1)
                    {
                        for (int k = 0; k < channelparamvalue.CalcList.Count; k++)
                        {
                            if (channelparamvalue.CalcList[k].Freq == sigfreq && channelparamvalue.CalcList[k].Gear == gear)
                            {
                                freqnumber1 = k;
                                break;
                            }
                        }
                        if (channelparamvalue.CalcList.Count != 0)
                        {
                            amplvalue1 = (amplArray1.Sum() * channelparamvalue.CalcList[freqnumber1].CoeffK + channelparamvalue.CalcList[freqnumber1].CoeffB) / 1;
                            if (checkBox5.Checked)
                            {
                                amplshow1 = Math.Abs(Meter.MeterReadingValue - amplvalue1) / AC;
                            }
                            else if (checkBox4.Checked)
                            {
                                amplshow1 = Math.Abs(Meter.MeterReadingValue - amplvalue1) / DC;
                            }
                        }
                        //amplArray1.Clear();
                    }

                    try
                    {
                        this.Invoke((EventHandler)delegate
                        {
                            float realAC = dev.CalRealVal(dev.GetGear(1), 1, sigfreq, AdValue1);
                            float realDC = dev.CalRealVal(dev.GetGear(1), 1, 0, offsetValue1);

                            textBox2.Text = AdValue1.ToString("0.000") + "\r\n" + realAC.ToString("0.000"); //交流 
                            textBox5.Text = offsetValue1.ToString("0.000") + "\r\n" + realDC.ToString("0.000"); //直流
                            textBox9.Text = Meter.MeterReadingValue.ToString();

                            if (comboBox1.Text == "1")
                            {
                                textBox10.Text = (100 * amplshow1).ToString("0.000") + "%";
                                textBox3.Text = dev.GetGear(1).ToString();
                            }
                        });
                    }
                    catch (Exception ex)
                    {

                    }
                    finally
                    {
                        amplArray1.Clear();
                    }

                }
                else
                {
                    try
                    {
                        this.Invoke((EventHandler)delegate
                        {
                            textBox2.Text = "超时";
                            textBox5.Text = "超时";
                        });
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }
        List<float> amplArray2 = new List<float>();
        float amplvalue2;
        float amplshow2;
        int freqnumber2;
        public void DealData2()
        {
            int gear = 0;
            ADBlock adBlock = dev.GetADBlock(2); //通道2         

            while (true)
            {
                byte channelid = 4;
                float AC = 1f;
                float DC = 1f;
                Int16[] adData2 = adBlock.GetAdData(5000);
                if (adData2 != null)
                {
                    float ampl2 = dsp2.CalAmpl(adData2, (int)sigfreq);
                    OffsetValue2 = dsp2.CalAmpl(adData2, 0);
                    AdValue2 = ampl2;
                    if (checkBox5.Checked)
                    {
                        amplArray2.Add(ampl2);
                        this.Invoke((EventHandler)delegate
                        {
                            gear = Convert.ToByte(textBox3.Text);
                            channelid = Convert.ToByte(comboBox1.Text);
                            var mk = this.toolStripComboBox6.SelectedItem as SampleInterface;
                            if (mk.Channels.Count == 2 && channelid == 2)
                            {
                                if (mk.Channels[channelid - 1].channeltype.TypeID == 22 || mk.Channels[channelid - 1].channeltype.TypeID == 31 || mk.Channels[channelid - 1].channeltype.TypeID == 32 || mk.Channels[channelid - 1].channeltype.TypeID == 52 || mk.Channels[channelid - 1].channeltype.TypeID == 70 || mk.Channels[channelid - 1].channeltype.TypeID == 71 || mk.Channels[channelid - 1].channeltype.TypeID == 72)
                                {
                                    AC = (float)(Convert.ToInt32(mk.Channels[channelid - 1].gears[gear].Name) / Math.Sqrt(2));
                                }
                                else
                                    AC = Convert.ToSingle(mk.Channels[channelid - 1].gears[gear].Name);
                            }
                            else if (channelid == 2)
                            {
                                if (mk.Channels[channelid].channeltype.TypeID == 22 || mk.Channels[channelid].channeltype.TypeID == 31 || mk.Channels[channelid].channeltype.TypeID == 32 || mk.Channels[channelid].channeltype.TypeID == 52 || mk.Channels[channelid].channeltype.TypeID == 70 || mk.Channels[channelid].channeltype.TypeID == 71 || mk.Channels[channelid].channeltype.TypeID == 72)
                                {
                                    AC = (float)(Convert.ToInt32(mk.Channels[channelid].gears[gear].Name) / Math.Sqrt(2));
                                }
                                else
                                    AC = Convert.ToSingle(mk.Channels[channelid].gears[gear].Name);
                            }
                        });
                    }
                    else if (checkBox4.Checked)
                    {
                        amplArray2.Add(OffsetValue2);
                        this.Invoke((EventHandler)delegate
                        {
                            gear = Convert.ToByte(textBox3.Text);
                            channelid = Convert.ToByte(comboBox1.Text);
                            var mk = this.toolStripComboBox6.SelectedItem as SampleInterface;
                            if (mk.Channels.Count == 2 && channelid == 2)
                            {
                                DC = Convert.ToSingle(mk.Channels[channelid - 1].gears[gear].Name);
                            }
                            else if (channelid == 2)
                            {
                                DC = Convert.ToSingle(mk.Channels[channelid].gears[gear].Name);
                            }
                        });
                    }

                    if (amplArray2.Count == 1 && channelid == 2)
                    {
                        if (checkBox5.Checked)
                        {
                            for (int k = 0; k < channelparamvalue.CalcList.Count; k++)
                            {
                                if (channelparamvalue.CalcList[k].Freq == sigfreq && channelparamvalue.CalcList[k].Gear == gear)
                                {
                                    freqnumber2 = k;
                                    break;
                                }
                            }
                        }
                        else if (checkBox4.Checked)
                        {
                            for (int k = 0; k < channelparamvalue.CalcList.Count; k++)
                            {
                                if (channelparamvalue.CalcList[k].Freq == 0 && channelparamvalue.CalcList[k].Gear == gear)
                                {
                                    freqnumber2 = k;
                                    break;
                                }
                            }
                        }
                        if (channelparamvalue.CalcList.Count != 0)
                        {
                            amplvalue2 = (amplArray2.Sum() * channelparamvalue.CalcList[freqnumber2].CoeffK + 1 * channelparamvalue.CalcList[freqnumber2].CoeffB) / 1;
                            if (checkBox5.Checked)
                            {
                                amplshow2 = Math.Abs(Meter.MeterReadingValue - amplvalue2) / AC;
                            }
                            else if (checkBox4.Checked)
                            {
                                amplshow2 = Math.Abs(Meter.MeterReadingValue - amplvalue2) / DC;
                            }
                        }
                        //amplArray2.Clear();
                    }
                    #region
                    //double adSum = 0;
                    //for (int i = 0; i < adData2.Length; i++)
                    //{
                    //    adSum += adData2[i];
                    //}
                    //Int16 avg = (Int16)(adSum / adData2.Length);


                    //for (int i = 0; i < adData2.Length; i++)
                    //{
                    //    adSum = adSum + (adData2[i] - avg) * (adData2[i] - avg);
                    //}

                    //float rms1 = (float)Math.Sqrt(adSum / adData2.Length);
                    ////林总

                    //float vv = (float)Math.Sqrt(2.0);

                    //float AdValue2x = Dft(adData2, sigfreq / 8000, adData2.Length) / vv;
                    #endregion

                    try
                    {
                        this.Invoke((EventHandler)delegate
                        {

                            float realAC = dev.CalRealVal(dev.GetGear(2), 2, sigfreq, ampl2);
                            float realDC = dev.CalRealVal(dev.GetGear(2), 2, 0, OffsetValue2);

                            textBox6.Text = AdValue2.ToString("0.000") + "\r\n" + realAC.ToString("0.000"); //交流 
                            textBox8.Text = OffsetValue2.ToString("0.000") + "\r\n" + realDC.ToString("0.000"); //直流
                            textBox9.Text = Meter.MeterReadingValue.ToString();


                            if (comboBox1.Text == "2")
                            {
                                textBox10.Text = (100 * amplshow2).ToString("0.000") + "%";
                                textBox3.Text = dev.GetGear(2).ToString();
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                    }
                    finally
                    {
                        amplArray2.Clear();
                    }
                }
                else
                {
                    try
                    {
                        this.Invoke((EventHandler)delegate
                        {
                            textBox8.Text = "超时";
                            textBox6.Text = "超时";
                        });
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }



        #region IRawDataShow 成员
        Queue<byte[]> rxQueue = new Queue<byte[]>();
        public void ShowData(byte[] buf)
        {
            byte[] tempBuf;
            if (buf[0] == 0x15)
            {
                tempBuf = new byte[6];
                return;
            }
            else
            {
                tempBuf = new byte[buf.Length];
            }
            Array.Copy(buf, tempBuf, tempBuf.Length);
            rxQueue.Enqueue(tempBuf);

            if (rxQueue.Count > 10)
            {
                rxQueue.Dequeue();
            }

        }

        #endregion


        internal void Meter_AcVoltSet()
        {
            meter.SetAcVolt();
        }
        internal void Meter_DcVoltSet()
        {
            meter.SetDcVolt();
        }
        internal void Meter_AcCurrentSet()
        {
            meter.SetAcCurrent();
        }
        internal void Meter_DcCurrentSet()
        {
            meter.SetDcCurrent();
        }

        private void 复位_Click(object sender, EventArgs e)//停止执行脚本文件
        {

        }


        public string ReadDeviceInfo(byte[] response)
        {
            if (response == null)
            {
                return "";
            }
            return devinfo.InitDeviceInformation(response);
        }

        private void toolStripButton2_Click(object sender, EventArgs e) //写入通道信息
        {
            ChannelParam channelparam = new ChannelParam();
            byte channelid = Convert.ToByte(comboBox1.Text);
            channelparam.ChannnelNum = channelid;

            var mk = this.toolStripComboBox6.SelectedItem as SampleInterface;
            channelparam.SampleRate = mk.SampleRate;

            if (comboBox2.Text == "打开")
            {
                channelparam.State = 1;
            }
            else if (comboBox2.Text == "关闭")
            {
                channelparam.State = 0;
            }

            List<byte> channeltypelist = new List<byte>();
            for (int jj = 0; jj < mk.Channels.Count; jj++)
            {
                channeltypelist.Add(mk.Channels[jj].ChannelID);
            }
            if (!channeltypelist.Contains(channelid))
            {
                MessageBox.Show("通道" + channelid + "应为关闭状态！");
                return;
            }
            if (mk.Channels.Count == 2 && channelid == 2)
            {
                channelparam.ChannelType = mk.Channels[channelid - 1].channeltype.TypeID;
            }
            else
            {
                channelparam.ChannelType = mk.Channels[channelid].channeltype.TypeID;
            }

            channelparam.Time = DateTime.Now;
            channelparam.Person = user.Name;


            DeviceRequest req1 = dev.ReadChannelInformation(channelid);

            dev.SendRequest(req1);

            if (req1.WaitResponse(1000))
            {
                ChannelParam param;
                byte[] response = req1.Response;
                if (channelparam.ChannelType >= 70)
                {
                    param = new ChannelParam(response, 6);
                }
                else
                {
                    param = new ChannelParam(response, 6, true);
                }


                if (param.CalcList.Count > 0)
                {
                    for (int h = 0; h < param.CalcList.Count; h++)
                    {
                        channelparam.AddCalcItem(param.CalcList[h]);
                    }

                    bool addNewParam = false;

                    for (int i = 0; i < listDataSource.Count; i++)
                    {
                        addNewParam = true;

                        for (int m = 0; m < param.CalcList.Count; m++)
                        {
                            if (listDataSource[i].Frequency == channelparam.CalcList[m].Freq && listDataSource[i].Gear == channelparam.CalcList[m].Gear)
                            {
                                CalcItem item = new CalcItem(listDataSource[i].Frequency, listDataSource[i].Gear, listDataSource[i].CalibrationParameter, listDataSource[i].Offset);
                                channelparam.ReplaceCalcItem(m, item);
                                addNewParam = false;

                                break;
                            }
                        }
                        if (addNewParam)
                        {
                            CalcItem item = new CalcItem(listDataSource[i].Frequency, listDataSource[i].Gear, listDataSource[i].CalibrationParameter, listDataSource[i].Offset);
                            channelparam.AddCalcItem(item);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < listDataSource.Count; i++)
                    {
                        channelparam.AddCalcItem(new CalcItem(listDataSource[i].Frequency, listDataSource[i].Gear, listDataSource[i].CalibrationParameter, listDataSource[i].Offset));
                    }
                }
            }
            byte[] param2;
            if (channelparam.ChannelType >= 70)
            {
                param2 = channelparam.ToArray();
            }
            else
            {
                param2 = channelparam.ToArray2();
            }
            DeviceRequest req = dev.WriteChannelInfo(param2);
            req.Callback += new EventHandler(req_Callback);
            dev.SendRequest(req);
        }

        private void button1_Click(object sender, EventArgs e)//重置
        {
            listDataSource.Clear();
            InitGrid();
            List<byte> data = new List<byte>();
            byte channelid = Convert.ToByte(comboBox1.Text);
            data.Add(channelid);
            var mk = this.toolStripComboBox6.SelectedItem as SampleInterface;
            data.Add((byte)(mk.SampleRate & 0xff));
            data.Add((byte)(((0 & 0x01) << 7) | ((mk.SampleRate >> 8) & 0xff)));

            List<byte> channeltypelist = new List<byte>();
            for (int jj = 0; jj < mk.Channels.Count; jj++)
            {
                channeltypelist.Add(mk.Channels[jj].ChannelID);
            }
            if (!channeltypelist.Contains(channelid))
            {
                MessageBox.Show("通道" + channelid + "应为关闭状态！");
                return;
            }
            if (mk.Channels.Count == 2 && channelid == 2)
            {
                data.Add(mk.Channels[channelid - 1].channeltype.TypeID);
            }
            else
            {
                data.Add(mk.Channels[channelid].channeltype.TypeID);
            }

            //时间            
            data.Add((byte)(DateTime.Now.Year - 2000));
            data.Add((byte)(DateTime.Now.Month));
            data.Add((byte)(DateTime.Now.Day));

            //名字
            byte nameLen = 0;
            byte[] name = null;
            if (string.IsNullOrEmpty(user.Name) == false)
            {
                name = ASCIIEncoding.UTF8.GetBytes(user.Name);
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
                data.AddRange(CalcItem.NULLData);
            }

            for (int i = 0; i < 2; i++)
            {
                data.Add(0);
            }


            byte[] dat = data.ToArray();

            UInt16 crc = CRC16.ComputeCRC16(dat, 0, dat.Length);


            data.Add((byte)(crc & 0xff));
            data.Add((byte)(crc >> 8));

            DeviceRequest req = dev.WriteChannelInfo(data.ToArray());

            req.Callback += new EventHandler(req_Callback);
            dev.SendRequest(req);

        }

        private void button2_Click(object sender, EventArgs e)
        {
            byte channeltype;
            ChannelParam param;
            var mk = this.toolStripComboBox6.SelectedItem as SampleInterface;
            byte channelid = Convert.ToByte(comboBox1.Text);

            if (mk.Channels.Count == 2 && channelid == 2)
            {
                channeltype = mk.Channels[channelid - 1].channeltype.TypeID;
            }
            else
            {
                channeltype = mk.Channels[channelid].channeltype.TypeID;
            }

            DeviceRequest req = dev.ReadChannelInformation(channelid);

            dev.SendRequest(req);

            if (req.WaitResponse(1000))
            {
                byte[] response = req.Response;

                if (channeltype >= 70)
                {
                    param = new ChannelParam(response, 6);

                }
                else
                {
                    param = new ChannelParam(response, 6, true);
                }

                using (SaveFileDialog ofd = new SaveFileDialog())
                {
                    ofd.Filter = "文本文件(*.txt)|*.txt";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllText(ofd.FileName, param.ToString());

                        MessageBox.Show("写入完成!");
                    }
                }

            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "文本文件(*.txt)|*.txt";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    List<byte> channeltypelist = new List<byte>();

                    ChannelParam channelparam = new ChannelParam();
                    byte channelid = Convert.ToByte(comboBox1.Text);
                    channelparam.ChannnelNum = channelid;

                    var mk = this.toolStripComboBox6.SelectedItem as SampleInterface;
                    channelparam.SampleRate = mk.SampleRate;

                    if (comboBox2.Text == "打开")
                    {
                        channelparam.State = 1;
                    }
                    else if (comboBox2.Text == "关闭")
                    {
                        channelparam.State = 0;
                    }

                    for (int jj = 0; jj < mk.Channels.Count; jj++)
                    {
                        channeltypelist.Add(mk.Channels[jj].ChannelID);
                    }
                    if (!channeltypelist.Contains(channelid))
                    {
                        MessageBox.Show("通道" + channelid + "应为关闭状态！");
                        return;
                    }
                    if (mk.Channels.Count == 2 && channelid == 2)
                    {
                        channelparam.ChannelType = mk.Channels[channelid - 1].channeltype.TypeID;
                    }
                    else
                    {
                        channelparam.ChannelType = mk.Channels[channelid].channeltype.TypeID;
                    }
                    channelparam.Time = DateTime.Now;
                    channelparam.Person = user.Name;
                    using (FileStream fs = new FileStream(ofd.FileName, FileMode.Open))
                    using (StreamReader sr = new StreamReader(fs))
                    {


                        while (true)
                        {
                            string line = sr.ReadLine();
                            if (string.IsNullOrEmpty(line))
                            {
                                break;
                            }
                            CalcItem calitem = new CalcItem(line);
                            channelparam.AddCalcItem(calitem);

                            Record record = new Record((short)calitem.Freq, calitem.Gear, 0, 0, calitem.CoeffK, calitem.CoeffB, 0);
                            listDataSource.Add(record);





                        }
                        InitGrid();

                    }


                    byte[] param2;
                    if (channelparam.ChannelType >= 70)
                    {
                        param2 = channelparam.ToArray();
                    }
                    else
                    {
                        param2 = channelparam.ToArray2();
                    }
                    DeviceRequest req = dev.WriteChannelInfo(param2);
                    req.Callback += new EventHandler(req_Callback);
                    dev.SendRequest(req);

                }

            }
        }
        float sigfreq = 25;
        private void button4_Click(object sender, EventArgs e)
        {
            sigfreq = float.Parse(textBox7.Text);

        }

        private void button5_Click(object sender, EventArgs e)
        {
            byte channelid = Convert.ToByte(comboBox1.Text);
            string content = "";
            for (int kk = 0; kk < listDataSource.Count; kk++)
            {
                content += listDataSource[kk].Frequency + " " + listDataSource[kk].ActualValue1 + " " + listDataSource[kk].Ad1 + " " + listDataSource[kk].ActualValue2 + " " + listDataSource[kk].Ad2 + "\r\n";

            }
            using (SaveFileDialog ofd = new SaveFileDialog())
            {
                ofd.Filter = "文本文件(*.txt)|*.txt";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(ofd.FileName, content);

                    MessageBox.Show("写入完成!");
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                Meter_AcCurrentSet();
            }
            else if (checkBox3.Checked)
            {
                Meter_DcCurrentSet();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                Meter_AcVoltSet();
            }
            else if (checkBox3.Checked)
            {
                Meter_DcVoltSet();
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            DeviceRequest req = dev.GetVersionInformation(AckTimeOut);
            req.Callback += new EventHandler(req_GetVersionInformation);
            dev.SendRequest(req);

        }

        private void button8_Click_1(object sender, EventArgs e)
        {
            FormWav wav = new FormWav(dev);
            wav.Show();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            byte channelid = Convert.ToByte(comboBox1.Text);
            byte gear = Convert.ToByte(comboBox3.Text);
            DeviceRequest req = dev.SetSpan(channelid, gear, 2000);
            req.Callback += new EventHandler(req_Callback);
            dev.SendRequest(req);
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            byte channelid = Convert.ToByte(comboBox1.Text);
            DeviceRequest req = dev.ReadSpan(channelid);
            req.Callback += new EventHandler(req_ReadSpan);
            dev.SendRequest(req);
        }
    }

}