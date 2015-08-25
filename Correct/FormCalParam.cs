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
    public partial class FormCalParam : Form
    {
        public FormCalParam()
        {
            InitializeComponent();
        }

        public FormCalParam(ChannelParam param)
            : this()
        {
            textBox1.Text = param.ChannnelNum.ToString();
            textBox2.Text = param.State.ToString();
            textBox3.Text = param.SampleRate.ToString();
            textBox4.Text = param.ChannelType.ToString();
            textBox5.Text = param.Person;
            textBox6.Text = param.Time.ToString("yyyy-MM-dd");



            gridControl1.DataSource = param.CalcList;

        }
    }
}
