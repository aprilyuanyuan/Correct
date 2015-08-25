using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Timers;

namespace Correct
{  
    /// <summary>
    /// 定义了一个委托类型
    /// </summary>
    public delegate void WhenGetNew();

    /// <summary>
    /// 串口封装类，Help By Wyz
    /// </summary>
    public class PortDataDisplay
    {
        /// <summary>
        /// 系统串口类
        /// </summary>
        public SerialPort serialPort
        {
            get;
            set;
        }


        /// <summary>
        /// 解析得到数据后触发事件
        /// </summary>
        public event WhenGetNew whenGetNew;
        /// <summary>
        /// 处理线程
        /// </summary>
        private SerialDataReceivedEventHandler threadCallHandler;
        /// <summary>
        /// 对外的数据类型定义
        /// </summary>
        public string dataSrc = "";
        /// <summary>
        /// 准备关闭串口=true
        /// </summary>
        private bool m_IsTryToClosePort = false;
        /// <summary>
        /// true表示正在接收数据
        /// </summary>
        private bool m_IsReceiving = false;


        /// <summary>
        /// 初始化
        /// </summary>
        public PortDataDisplay()
        {
        }

        /// <summary>
        /// 有参数的构造函数
        /// </summary>
        /// <param name="PortName">串口号，如"COM1"</param>
        /// <param name="BaudRate">波特率，如19200</param>
        public void InitSerialPort(string PortName, int BaudRate)
        {
            try
            {
                serialPort = new SerialPort(PortName, BaudRate);
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public void InitSerialPort2(string PortName)
        {
            serialPort = new SerialPort(PortName);
        }
        /// <summary>
        /// 开始工作
        /// </summary>
        public void ConnectDeveice()
        {
            //0.注册事件
            serialPort.DataReceived -= OnSerialPortDataCome;
            serialPort.DataReceived += OnSerialPortDataCome;
            //1.再设置一下串口参数
            if (this.serialPort.IsOpen == false)
            {
                this.serialPort.ReadBufferSize = 1000;
                this.serialPort.ReceivedBytesThreshold = 1;//数据达到120的时候才就要触发事件，不行！！应该是数据来就触发
                //2.打开串口开始工作
                m_IsTryToClosePort = false;
                this.serialPort.Open();
            }
        }

        /// <summary>
        /// 结束工作
        /// </summary>
        public void DisconnectDeveice() // 关键和核心！！！
        {
            m_IsTryToClosePort = true;
            while (m_IsReceiving)
            {
                System.Windows.Forms.Application.DoEvents();
            }
            if(serialPort.IsOpen)
            {
                 serialPort.Close();
            }           
        }

        /// <summary>
        /// 当通知到有数据时处理（读取，与分析）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSerialPortDataCome(object sender, SerialDataReceivedEventArgs e)
        {
            if (m_IsTryToClosePort) // 关键！！！
            {
                return;
            }

            m_IsReceiving = true; // 关键!!!

            try
            {
                if (threadCallHandler == null)
                {
                    threadCallHandler = new SerialDataReceivedEventHandler(OnSerialPortDataCome);
                }

                //read
                dataSrc = serialPort.ReadExisting();//读出缓冲区所有数据
                if (dataSrc != "" && this.whenGetNew != null)
                {
                    this.whenGetNew();
                }
            }
            finally // 放在finally里面比较好。
            {
                m_IsReceiving = false; // 关键!!!
            }
        }
    }
}












