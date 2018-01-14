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
using System.Runtime.InteropServices;
using WindowsInput.Native;
using AForge.Video;

namespace ERDTransport
{
    class RMDServer
    {
        public static int port = 2009;
        TcpClient tcpClient;
        NetworkStream networkStream;
        BinaryFormatter formatter = new BinaryFormatter();
        Timer timer = new Timer();
        ScreenCaptureStream screenCapture;
        MyEncoder encoder;

        Task sendFrameTask = null;
        
        public RMDServer(string addressServer, int hash)
        {
            screenCapture = new ScreenCaptureStream(new Rectangle(Screen.PrimaryScreen.Bounds.Location, Screen.PrimaryScreen.Bounds.Size),30);
            screenCapture.NewFrame += ScreenCapture_NewFrame;
            screenCapture.Start();

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

        ClientCommand sendFrameCommand = null;

        private void ScreenCapture_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if(sendFrameCommand != null)
            {
                SendFrame(sendFrameCommand, eventArgs.Frame);
                sendFrameCommand = null;
            }
        }

        private void Timer_Elapsed(object sender, EventArgs e)
        {
            while (networkStream.DataAvailable)
            {
                string data = (string)formatter.Deserialize(networkStream);
                ClientCommand command = JsonConvert.DeserializeObject<ClientCommand>(data);

                if (command.needFrame)
                {
                    sendFrameCommand = command;
                }

                if(command.mouseEvent != 0)
                {
                    MouseEvent(command);
                }

                if(command.moveCursor)
                {
                    MoveCursor(command);
                }

                if(command.pressKey != 0)
                {
                    PressKey(command);
                }
            }
        }

        WindowsInput.InputSimulator simulator = new WindowsInput.InputSimulator();
        void PressKey(ClientCommand command)
        {
            if (command.pressKey == 1)
            {
                simulator.Keyboard.KeyDown(command.key);
            }
            else simulator.Keyboard.KeyUp(command.key);
        }

        void MoveCursor(ClientCommand command)
        {
            int X = (int)(command.mouseRelativeX * Screen.PrimaryScreen.Bounds.Width) + (int)Screen.PrimaryScreen.Bounds.X;
            int Y = (int)(command.mouseRelativeY * Screen.PrimaryScreen.Bounds.Height) + (int)Screen.PrimaryScreen.Bounds.Y;
            Cursor.Position = new Point(X, Y);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        void MouseEvent(ClientCommand command)
        {
            uint X = (uint)(command.mouseRelativeX * Screen.PrimaryScreen.Bounds.Width) + (uint)Screen.PrimaryScreen.Bounds.X;
            uint Y = (uint)(command.mouseRelativeY * Screen.PrimaryScreen.Bounds.Height) + (uint)Screen.PrimaryScreen.Bounds.Y;
            mouse_event((uint)command.mouseEvent, X, Y, 0, 0);
        }

        object tcpLock = new object();

        void SendFrame(ClientCommand command, Bitmap screenImg)
        {
            if(encoder == null)
            {
                encoder = new MyEncoder(command.needWidth, command.needHeight);
            }
            
            Bitmap small_map = new Bitmap(screenImg, command.needWidth, command.needHeight);
            MemoryStream str = new MemoryStream();
            encoder.WriteToStr(small_map, str);
            

            MemoryStream compressed = new MemoryStream();
            str.Position = 0;
            ShortEncoder.Encode(str, compressed);
            compressed.Position = 0;

            lock (tcpLock)
            {
                formatter.Serialize(networkStream, (int)compressed.Length);
                compressed.CopyTo(networkStream);
            }
        }

        public void Stop()
        {
            timer.Stop();
            tcpClient.Close();
        }
    }
}
