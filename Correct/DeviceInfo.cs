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
    public partial class DeviceInfo : Form
    {
        public DeviceInfo()
        {
            InitializeComponent();            
        }

        private void DeviceInfo_Load(object sender, EventArgs e)
        {

        }

        public string ProductionSequence
        {
            set;
            get;
        }

        public DateTime ProductionDate
        {
            set;
            get;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ProductionSequence = textBox1.Text;
            ProductionDate = dateTimePicker1.Value;
            Hide();

        }
    }
}
