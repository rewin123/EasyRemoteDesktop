using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ERDTransport
{
    public partial class RMDForm : Form
    {
        RMDClient client;
        public RMDForm(string address, int hash)
        {
            InitializeComponent();
            client = new RMDClient(address, hash);
            client.NewFrame += Client_NewFrame;
            client.width = pictureBox1.Width;
            client.height = pictureBox1.Height;
        }

        private void Client_NewFrame(Bitmap map)
        {
            pictureBox1.Image = map;
        }
    }
}
