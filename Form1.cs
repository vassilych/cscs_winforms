using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsCSCS
{
    public partial class Form1 : Form
    {
        public static Action<string, string> OnButtonClick;
        static Form1 Instance;

        public static Control GetWidget(string name)
        {
            if (name == Instance.label1.Text)
            {
                return Instance.label1;
            }
            if (name == Instance.button1.Text)
            {
                return Instance.button1;
            }
            if (name == Instance.button2.Text)
            {
                return Instance.button2;
            }
            if (name == Instance.button3.Text)
            {
                return Instance.button3;
            }
            if (name == Instance.button4.Text)
            {
                return Instance.button4;
            }
            if (name == Instance.button5.Text)
            {
                return Instance.button5;
            }
            if (name == "textBox1")
            {
                return Instance.textBox1;
            }
            return null;
        }

        public Form1()
        {
            InitializeComponent();
            Instance = this;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            OnButtonClick?.Invoke(((System.Windows.Forms.Button)sender).Text, "button");
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            OnButtonClick?.Invoke(((System.Windows.Forms.Button)sender).Text, "button");
        }
        private void Button2_Click_1(object sender, EventArgs e)
        {
            OnButtonClick?.Invoke(((System.Windows.Forms.Button)sender).Text, "button");
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            OnButtonClick?.Invoke(((System.Windows.Forms.Button)sender).Text, "button");
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            OnButtonClick?.Invoke(((System.Windows.Forms.Button)sender).Text, "button");
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            OnButtonClick?.Invoke(((System.Windows.Forms.Button)sender).Text, "button");
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            OnButtonClick?.Invoke(((System.Windows.Forms.TextBox)sender).Text, "textbox");
        }

        private void Label1_Click(object sender, EventArgs e)
        {
            OnButtonClick?.Invoke(((System.Windows.Forms.Label)sender).Text, "label");
        }

    }
}
