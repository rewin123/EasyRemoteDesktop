using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ERDTransport;
using System.Net.Sockets;

namespace ERDServer
{
    public partial class Form1 : Form
    {
        
        ERDOnlineBase onlineBase;
        public Form1()
        {
            InitializeComponent();

            listBox1.MouseDoubleClick += ListBox1_MouseDoubleClick;

            onlineBase = new ERDOnlineBase();
            onlineBase.NewClient += OnlineBase_NewClient;
            
            timer1.Start();
        }

        private void ListBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
                onlineBase.RunRDP(new SimpleUser(onlineBase.users[listBox1.SelectedIndex]));
        }

        private void OnlineBase_NewClient(User client)
        {
            listBox1.Invoke(new Action(UpdateUser));
        }

        void UpdateUser()
        {
            listBox1.Items.Add(onlineBase.users.Last().ToString());
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

        }
    }
}
