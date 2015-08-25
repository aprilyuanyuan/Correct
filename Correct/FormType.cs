using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Correct
{
    public partial class FormType : Form
    {

        public string[] DeviceName = new string[] { "轨道电路监测模块", "交流道岔监测模块", "直流道岔监测模块", "交流信号机监测模块", "直流信号机监测模块", "综合信号板电流采集模块", "综合信号板电压采集模块" };

        public FormType()
        {
            InitializeComponent();
            comboBox1.DataSource = DeviceName;
        }

        public int DevIndex
        {
            get
            {
                return comboBox1.SelectedIndex;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
    }
}
