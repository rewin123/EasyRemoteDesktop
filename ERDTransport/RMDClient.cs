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
using System.IO;

namespace ERDTransport
{
    class RMDClient
    {
        TcpClient tcpClient;
        NetworkStream networkStream;
        BinaryFormatter formatter = new BinaryFormatter();
        Timer timer = new Timer();
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
                timer.Interval = 20;
                timer.Tick += Timer_Elapsed;
                timer.Start();
            }
        }

        private void Timer_Elapsed(object sender, EventArgs e)
        {
            if(networkStream.DataAvailable)
            {
                int length = (int)formatter.Deserialize(networkStream);
                byte[] buffer = new byte[length];
                int position = 0;
                while(position < length)
                {
                    position += networkStream.Read(buffer, position, length - position);
                }
                
                //networkStream.Read(buffer, 0, (int)length);
                MemoryStream mem = new MemoryStream(buffer);
                mem.Position = 0;
                screenImg = new Bitmap(mem);
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

        public void MouseClick(bool isLeft, float relativeX, float relativeY)
        {
            ClientCommand clientCommand = new ClientCommand
            {
                leftMouseClick = isLeft,
                rightMouseClick = !isLeft,
                mouseRelativeX = relativeX,
                mouseRelativeY = relativeY
            };

            formatter.Serialize(networkStream, JsonConvert.SerializeObject(clientCommand));
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
