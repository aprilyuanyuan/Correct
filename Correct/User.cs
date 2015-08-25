using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Correct
{
    public partial class User : Form
    {
        
        string[] str = new string[] { };
        public string Name = "";
        public string Password = "";
             

        public User()
        {
            InitializeComponent();
           
        }

        private void button1_Click(object sender, EventArgs e)//登陆
        {
            if(textBox1.Text == "")
            {
                MessageBox.Show("用户名不能为空！");
                return;
            }
            if (textBox2.Text == "")
            {
                MessageBox.Show("密码不能为空！");
                return;
            }
          
            Dictionary<string, string> user = new Dictionary<string, string>();
            string line;
            string address = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "\\UserName.txt";
            FileInfo usernameFile = new FileInfo(address);
            if (!usernameFile.Exists)
            {
                FileStream UserNameFile = new FileStream(address, FileMode.OpenOrCreate);              
            }
           
            StreamReader sr = new StreamReader(address);

            while ((line = sr.ReadLine()) != null) {
                str = line.Split(',');
                user.Add(str[0],str[1]);
            }
            
            if (user.Keys.Contains(textBox1.Text) && user[textBox1.Text] == textBox2.Text)
            {
                Name = textBox1.Text;
                Password = textBox2.Text;
                //Close();
                Hide();
            } 
            else if (user.Keys.Contains(textBox1.Text) && textBox2.Text != user[textBox1.Text])
            {              
               MessageBox.Show("密码错误！"); 
                           
            }
            else 
            {
                MessageBox.Show("该用户名不存在！");
            }
            sr.Close();
        }
              
        private void button3_Click(object sender, EventArgs e)//注册
        {
            string line;
            Dictionary<string, string> user = new Dictionary<string, string>();
            string address = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "\\UserName.txt";
            FileInfo usernameFile = new FileInfo(address);
            if (textBox1.Text == "")
            {
                MessageBox.Show("用户名不能为空！");
                return;
            }
            if (textBox2.Text == "")
            {
                MessageBox.Show("密码不能为空！");
                return;
            }

            if (!usernameFile.Exists)
            {
                FileStream UserNameFile = new FileStream(address, FileMode.CreateNew);
               
            }
           
            StreamReader sr = new StreamReader(address);
            while ((line = sr.ReadLine()) != null)
            {
                str = line.Split(',');
                user.Add(str[0], str[1]);
            }
            sr.Close();
            StreamWriter sw = new StreamWriter(address,true);
            if (user.Keys.Contains(textBox1.Text))
            {
                MessageBox.Show("该用户名已存在！");
            }
            else
            {
                sw.WriteLine();
                sw.Write(textBox1.Text + "," + textBox2.Text);
                MessageBox.Show("注册成功！");
            }
            sw.Close();

        }

        private void User_FormClosed(object sender, FormClosedEventArgs e)
        {
            //Application.Exit();
            System.Environment.Exit(0);
        }

      
    }
}
