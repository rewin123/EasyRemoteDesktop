﻿using System;
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
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        RMDClient client;
        public RMDForm(string address, int hash)
        {
            InitializeComponent();
            client = new RMDClient(address, hash, pictureBox1.Width, pictureBox1.Height);
            client.NewFrame += Client_NewFrame;

            pictureBox1.MouseDown += PictureBox1_MouseDown;
            pictureBox1.MouseUp += PictureBox1_MouseUp;
            pictureBox1.MouseMove += PictureBox1_MouseMove;
            KeyDown += RMDForm_KeyDown;
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

        private void RMDForm_KeyDown(object sender, KeyEventArgs e)
        {
            client.PressKey(e.KeyCode);
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
