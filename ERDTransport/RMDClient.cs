using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing;
using System.Drawing.Imaging;
using Newtonsoft.Json;
using System.Windows.Forms;

namespace ERDTransport
{
    class RMDClient
    {
        TcpClient tcpClient;
        NetworkStream networkStream;
        BinaryFormatter formatter = new BinaryFormatter();
        System.Timers.Timer timer = new System.Timers.Timer(20);
        public Bitmap screenImg;

        public int width = 300;
        public int height = 200;

        public delegate void FrameEvent(Bitmap map);
        public event FrameEvent NewFrame;

        public RMDClient(string address, int hash)
        {
            screenImg = new Bitmap(1, 1);
            
            if(SetupConnect(address,hash))
            {
                timer.Elapsed += Timer_Elapsed;
                timer.Start();
            }
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if(networkStream.DataAvailable)
            {
                screenImg = new Bitmap(networkStream);
                NewFrame.Invoke(screenImg);
            }
            else
            {
                ClientCommand clientCommand = new ClientCommand
                {
                    needFrame = true,
                    needHeight = height,
                    needWidth = width
                };
                formatter.Serialize(networkStream, JsonConvert.SerializeObject(clientCommand));
            }
        }

        bool SetupConnect(string address, int hash)
        {
            tcpClient = new TcpClient(address, RMDServer.port);
            networkStream = tcpClient.GetStream();

            formatter.Serialize(networkStream, 0); //говорим, что мы клиент
            formatter.Serialize(networkStream, hash);

            int result = (int)formatter.Deserialize(networkStream);
            return result == 1;
        }
    }
}
