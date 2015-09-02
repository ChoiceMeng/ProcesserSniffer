using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProcesserSniffer
{
    public partial class ReName : Form
    {
        public ReName()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                Form1.ins.mCurPro.szShowName = textBox1.Text;
                Form1.ins.SaveInfoXML("Config\\Info.xml");

                Close();
            }
        }
    }
}
