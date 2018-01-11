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
using WindowsInput;
using WindowsInput.Native;

namespace ERDTransport
{
    public partial class RMDForm : Form
    {
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        RMDClient client;
        int startWidth;
        int startHeight;
        int deltaX;
        int deltaY;
        public RMDForm(string address, int hash)
        {
            InitializeComponent();
            client = new RMDClient(address, hash, pictureBox1.Width, pictureBox1.Height);
            startWidth = pictureBox1.Width;
            startHeight = pictureBox1.Height;
            client.NewFrame += Client_NewFrame;

            pictureBox1.MouseDown += PictureBox1_MouseDown;
            pictureBox1.MouseUp += PictureBox1_MouseUp;
            pictureBox1.MouseMove += PictureBox1_MouseMove;
            KeyDown += RMDForm_KeyDown;
            SizeChanged += RMDForm_SizeChanged;

            deltaX = Width - pictureBox1.Width;
            deltaY = Height - pictureBox1.Height;
        }

        private void RMDForm_SizeChanged(object sender, EventArgs e)
        {
            pictureBox1.Width = Width - deltaX;
            pictureBox1.Height = Height - deltaY;
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            client.MouseClick(e.Button == MouseButtons.Left ? MOUSEEVENTF_LEFTUP : MOUSEEVENTF_RIGHTUP,
                (float)e.X / pictureBox1.Width, (float)e.Y / pictureBox1.Height);
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            client.MouseClick(e.Button == MouseButtons.Left ? MOUSEEVENTF_LEFTDOWN : MOUSEEVENTF_RIGHTDOWN,
                (float)e.X / pictureBox1.Width, (float)e.Y / pictureBox1.Height);
        }

        InputSimulator simulator = new InputSimulator();

        private void RMDForm_KeyDown(object sender, KeyEventArgs e)
        {
            var max = Enum.GetValues(typeof(VirtualKeyCode)).Cast<VirtualKeyCode>().Max();
            var min = Enum.GetValues(typeof(VirtualKeyCode)).Cast<VirtualKeyCode>().Min();
            for(var index = min; index < max;index++)
            {
                if(simulator.InputDeviceState.IsKeyDown(index))
                {
                    client.PressKey(index, false);
                }
                if (simulator.InputDeviceState.IsKeyUp(index))
                {
                    client.PressKey(index, true);
                }
            }
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            client.MouseMove((float)e.X / pictureBox1.Width, (float)e.Y / pictureBox1.Height);
        }
        

        private void Client_NewFrame(Bitmap map)
        {
            pictureBox1.Image = map;
        }
    }
}
