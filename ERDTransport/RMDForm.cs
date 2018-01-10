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
            client = new RMDClient(address, hash, pictureBox1.Width, pictureBox1.Height);
            client.NewFrame += Client_NewFrame;

            pictureBox1.MouseClick += PictureBox1_MouseClick;
            pictureBox1.MouseMove += PictureBox1_MouseMove;
            KeyDown += RMDForm_KeyDown;
        }

        private void RMDForm_KeyDown(object sender, KeyEventArgs e)
        {
            client.PressKey(e.KeyCode);
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            client.MouseMove((float)e.X / pictureBox1.Width, (float)e.Y / pictureBox1.Height);
        }

        private void PictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            client.MouseClick(e.Button == MouseButtons.Left,
                (float)e.X / pictureBox1.Width, (float)e.Y / pictureBox1.Height);
        }

        private void Client_NewFrame(Bitmap map)
        {
            pictureBox1.Image = map;
        }
    }
}
