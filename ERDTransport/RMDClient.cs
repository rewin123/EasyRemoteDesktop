﻿using System;
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
using WindowsInput;
using WindowsInput.Native;

namespace ERDTransport
{
    class RMDClient
    {
        TcpClient tcpClient;
        NetworkStream networkStream;
        BinaryFormatter formatter = new BinaryFormatter();
        Timer timer = new Timer();
        public Bitmap screenImg;
        MyEncoder encoder;

        public int width = 300;
        public int height = 200;

        public delegate void FrameEvent(Bitmap map);
        public event FrameEvent NewFrame;

        public RMDClient(string address, int hash, int width, int height)
        {
            this.width = width;
            this.height = height;
            encoder = new MyEncoder(width, height);
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
                MemoryStream decompressed = new MemoryStream();
                ShortEncoder.Decode(mem, decompressed);
                decompressed.Position = 0;
                screenImg = encoder.LoadFromStr(decompressed);
                //screenImg = new Bitmap(mem);
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

        public void MouseMove(float relativeX, float relativeY)
        {
            ClientCommand clientCommand = new ClientCommand
            {
                moveCursor = true,
                mouseRelativeX = relativeX,
                mouseRelativeY = relativeY
            };

            formatter.Serialize(networkStream, JsonConvert.SerializeObject(clientCommand));
        }

        public void MouseClick(int mouseEvent, float relativeX, float relativeY)
        {
            ClientCommand clientCommand = new ClientCommand
            {
                mouseEvent = mouseEvent,
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

        public void PressKey(VirtualKeyCode key, bool keyUp)
        {
            ClientCommand clientCommand = new ClientCommand
            {
                key = key,
                pressKey = (byte)(keyUp ? 2 : 1)
            };

            formatter.Serialize(networkStream, JsonConvert.SerializeObject(clientCommand));
        }
    }
}
