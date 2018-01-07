using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EasyRemoteDesktop
{
    public partial class AddressForm : Form
    {
        public string address = "";
        public AddressForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            address = textBox1.Text;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
