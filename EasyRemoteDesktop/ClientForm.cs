﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ERDTransport;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Net;

namespace EasyRemoteDesktop
{
    public partial class ClientForm : Form
    {
        ERDClientBase clientBase;
        public ClientForm()
        {
            AddressForm addressForm = new AddressForm();
            if (addressForm.ShowDialog() != DialogResult.OK)
                Close();

            clientBase = new ERDClientBase(addressForm.address);
            if ((new Login(clientBase).ShowDialog()) != DialogResult.OK)
                Close();

            clientBase.UpdateUsers();
            InitializeComponent();


            listBox1.Items.Clear();
            for (int i = 0; i < clientBase.users.Length; i++)
                listBox1.Items.Add(clientBase.users[i].ToString());

            listBox1.MouseDoubleClick += ListBox1_MouseDoubleClick;

            timer1.Start();
        }

        private void ListBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
                clientBase.RunRDP(clientBase.users[listBox1.SelectedIndex]);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            clientBase.UpdateUsers();
            listBox1.Items.Clear();
            for (int i = 0; i < clientBase.users.Length; i++)
                listBox1.Items.Add(clientBase.users[i].ToString());
        }
    }
}
