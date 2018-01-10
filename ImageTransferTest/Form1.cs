﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;

namespace ImageTransferTest
{
    public partial class Form1 : Form
    {
        Bitmap screen_img;
        Graphics screen_gr;
        MyEncoder server;
        MyEncoder reciever;
        public Form1()
        {
            screen_img = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format16bppRgb565);
            screen_gr = Graphics.FromImage(screen_img);
            InitializeComponent();

            server = new MyEncoder(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            reciever = new MyEncoder(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

            timer1.Start();
        }

        long max_byterange = 0;

        private void button1_Click(object sender, EventArgs e)
        {
            screen_gr.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size);

            MemoryStream str = new MemoryStream();
            server.WriteToStr(screen_img, str);
            MemoryStream compressed = new MemoryStream();
            ShortEncoder.Encode(str, compressed);

            max_byterange = Math.Max(max_byterange, compressed.Length);
            Text = (max_byterange).ToString();

            MemoryStream decompressed = new MemoryStream();
            compressed.Position = 0;
            ShortEncoder.Decode(compressed, decompressed);

            decompressed.Position = 0;
            str.Position = 0;
            pictureBox1.Image = reciever.LoadFromStr(decompressed);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            string timesheet = "";
            DateTime start = DateTime.Now;
            DateTime local_start = start;

            screen_gr.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size);

            timesheet += "Copy screen:" + (DateTime.Now - local_start).TotalMilliseconds.ToString();
            local_start = DateTime.Now;

            MemoryStream str = new MemoryStream();
            server.WriteToStr(screen_img, str);

            timesheet += "\nWrite:" + (DateTime.Now - local_start).TotalMilliseconds.ToString();
            local_start = DateTime.Now;

            MemoryStream compressed = new MemoryStream();
            str.Position = 0;
            ShortEncoder.Encode(str, compressed);

            timesheet += "\nCompress:" + (DateTime.Now - local_start).TotalMilliseconds.ToString();
            local_start = DateTime.Now;

            max_byterange = Math.Max(max_byterange, compressed.Length);
            Text = (compressed.Length).ToString();

            MemoryStream decompressed = new MemoryStream();
            compressed.Position = 0;
            ShortEncoder.Decode(compressed, decompressed);

            timesheet += "\nDecompress:" + (DateTime.Now - local_start).TotalMilliseconds.ToString();
            local_start = DateTime.Now;


            decompressed.Position = 0;
            str.Position = 0;
            pictureBox1.Image = reciever.LoadFromStr(decompressed);

            timesheet += "\nLoad:" + (DateTime.Now - local_start).TotalMilliseconds.ToString();
            local_start = DateTime.Now;

            timesheet += "\nAll:" + (DateTime.Now - start).TotalMilliseconds.ToString();

            label1.Text = timesheet;
        }
    }

    class MyEncoder
    {
        int rect_size = 16;
        public int width = 0;
        public int height = 0;
        Bitmap map;
        BitmapData data;
        Bitmap block;
        BitmapData block_data;

        List<Metablock> blocks = new List<Metablock>();
        BinaryFormatter formatter = new BinaryFormatter();

        public MyEncoder(int width, int height)
        {
            this.width = width;
            this.height = height;
            map = new Bitmap(width, height, PixelFormat.Format16bppRgb565);
            data = map.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format16bppRgb565);

            block = new Bitmap(rect_size, rect_size, PixelFormat.Format16bppRgb565);
            block_data = block.LockBits(new Rectangle(0, 0, rect_size, rect_size), ImageLockMode.WriteOnly, PixelFormat.Format16bppRgb565);
            CreateMetaclocks();
        }

        public void WriteToStr(Bitmap next, Stream str)
        {
            BitmapData next_data = next.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format16bppRgb565);
            unsafe
            {
                MemoryStream block_stream = new MemoryStream();
                short* next_point = (short*)next_data.Scan0.ToPointer();
                short* point = (short*)data.Scan0.ToPointer();
                byte* b_point = (byte*)point;
                short* block_img = (short*)block_data.Scan0.ToPointer();
                Metablock block;
                for(int i = 0;i < blocks.Count;i++)
                {
                    block = blocks[i];
                    if (!CheckBlock(i, next_point, point))
                    {
                        #region Отправлеем сообщение о изменении блока
                        Message m = new Message
                        {
                            blockIndex = i,
                            bytes = block.width * block.height * 2
                        };
                        formatter.Serialize(str, m);

                        int endX = block.x + block.width;
                        int endY = block.y + block.height;
                        for (int y = block.y; y < endY; y++)
                        {
                            for (int x = block.x; x < endX; x++)
                            {
                                point[y * width + x] = next_point[y * width + x];
                                str.WriteByte(b_point[(y * width + x) * 2]);
                                str.WriteByte(b_point[(y * width + x) * 2 + 1]);
                            }
                        }
                        #endregion
                    }
                }
            }

            next.UnlockBits(next_data);
        }
        
        unsafe bool CheckBlock(int i, short* next_point, short* point)
        {
            Metablock block = blocks[i];
            int endX = block.x + block.width;
            int endY = block.y + block.height;
            int dist = 0;
            for(int x = block.x;x < endX;x++)
            {
                for(int y = block.y;y < endY;y++)
                {
                    dist += Math.Abs(point[y * width + x] - next_point[y * width + x]);
                }
            }

            return dist == 0;
        }

        public Bitmap LoadFromStr(Stream str)
        {
            Bitmap next = new Bitmap(width, height);
            BitmapData next_data = next.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format16bppRgb565);
            unsafe
            {
                short* next_point = (short*)next_data.Scan0.ToPointer();
                short* point = (short*)data.Scan0.ToPointer();
                byte* b_point = (byte*)point;
                while (str.Position < str.Length)
                {
                    Message m = (Message)formatter.Deserialize(str);
                    Metablock block = blocks[m.blockIndex];

                    int endX = block.x + block.width;
                    int endY = block.y + block.height;
                    for (int y = block.y; y < endY; y++)
                    {
                        for (int x = block.x; x < endX; x++)
                        {
                            b_point[(y * width + x) * 2] = (byte)str.ReadByte();
                            b_point[(y * width + x) * 2 + 1] = (byte)str.ReadByte();
                        }
                    }
                }

                for(int y = 0;y < height;y++)
                {
                    for(int x = 0;x < width;x++)
                    {
                        next_point[y * width + x] = point[y * width + x];
                    }
                }
            }
            next.UnlockBits(next_data);
            return next;
        }

        void CreateMetaclocks()
        {
            for(int x = 0;x < width;x += rect_size)
            {
                for(int y = 0;y < height;y += rect_size)
                {
                    Metablock block = new Metablock
                    {
                        x = x,
                        y = y,
                        width = Math.Min(x + rect_size, width) - x,
                        height = Math.Min(y + rect_size, height) - y
                    };

                    blocks.Add(block);
                }
            }
        }
    }

    [Serializable]
    class Metablock
    {
        public int x = 0;
        public int y = 0;
        public int width = 0;
        public int height = 0;
    }

    [Serializable]
    class Message
    {
        public int blockIndex = 0;
        public int bytes = 0;
    }
}