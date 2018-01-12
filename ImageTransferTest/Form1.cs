using System;
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
using ERDTransport;
using AForge.Video;


namespace ImageTransferTest
{
    public partial class Form1 : Form
    {
        Bitmap screen_img;
        Graphics screen_gr;
        MyEncoder server;
        MyEncoder reciever;
        ScreenCaptureStream screenCapture;
        DateTime lastFrame = DateTime.Now;
        public Form1()
        {
            
            screenCapture = new ScreenCaptureStream(new Rectangle(0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height), 60);
            screenCapture.NewFrame += ScreenCapture_NewFrame;
            screenCapture.Start();

            screen_img = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format16bppRgb565);
            screen_gr = Graphics.FromImage(screen_img);
            InitializeComponent();

            server = new MyEncoder(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            reciever = new MyEncoder(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

            this.SizeChanged += Form1_SizeChanged;

            timer1.Start();
        }

        private void ScreenCapture_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Invoke((BitmapAction)ImageLoad, eventArgs.Frame);
        }

        delegate void BitmapAction(Bitmap map);

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
        }

        long max_byterange = 0;

        private void Button1_Click(object sender, EventArgs e)
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

        void ImageLoad(Bitmap screen_img)
        {
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            
        }
        
    }

    
}
