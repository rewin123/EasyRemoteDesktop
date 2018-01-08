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
    class RMDServer
    {
        public static int port = 2009;
        TcpClient tcpClient;
        NetworkStream networkStream;
        BinaryFormatter formatter = new BinaryFormatter();
        Timer timer = new Timer();
        Bitmap screenImg;
        Graphics screenGr;

        public RMDServer(string addressServer, int hash)
        {
            screenImg = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format24bppRgb);
            screenGr = Graphics.FromImage(screenImg);
            tcpClient = new TcpClient(addressServer, port);
            networkStream = tcpClient.GetStream();

            formatter.Serialize(networkStream, 1); //говорим, что мы сервер
            formatter.Serialize(networkStream, hash);
            int result = (int)formatter.Deserialize(networkStream);
            if(result != 1)
            {
                tcpClient.Dispose();
                return;
            }

            timer.Interval = 10;
            timer.Tick += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, EventArgs e)
        {
            if (networkStream.DataAvailable)
            {
                string data = (string)formatter.Deserialize(networkStream);
                ClientCommand command = JsonConvert.DeserializeObject<ClientCommand>(data);

                if (command.needFrame)
                {
                    SendFrame(command);
                }
            }
        }

        void SendFrame(ClientCommand command)
        {
            screenGr.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size);
            Bitmap small_map = new Bitmap(screenImg, command.needWidth, command.needHeight);
            MemoryStream mem = new MemoryStream();
            small_map.Save(mem, ImageFormat.Jpeg);
            mem.Position = 0;
            byte[] buffer = new byte[(int)mem.Length];
            mem.Read(buffer, 0, (int)mem.Length);

            formatter.Serialize(networkStream, buffer.Length);
            networkStream.Write(buffer, 0, buffer.Length);

            mem.Dispose();
            small_map.Dispose();
        }

        public void Stop()
        {
            timer.Stop();
            tcpClient.Close();
        }
    }
}
