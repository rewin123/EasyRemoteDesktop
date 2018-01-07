using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ERDTransport;

namespace EasyRemoteDesktop
{
    public partial class Login : Form
    {
        ERDClientBase clientBase;
        public Login(ERDClientBase clientBase)
        {
            this.clientBase = clientBase;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            clientBase.Register(textBox1.Text);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
