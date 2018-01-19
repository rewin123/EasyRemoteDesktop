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
using NReco.VideoConverter;

namespace ImageTransferTest
{
    public partial class Form1 : Form
    {
        ScreenCaptureStream screenCapture;
        DateTime lastFrame = DateTime.Now;
        FCompress compress = new FCompress(8,8);
        bool save = false;
        public Form1()
        {
            screenCapture = new ScreenCaptureStream(new Rectangle(0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height), 60);
            screenCapture.NewFrame += ScreenCapture_NewFrame;
            screenCapture.Start();
            
            InitializeComponent();
            SizeChanged += Form1_SizeChanged;

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
        

        private void Button1_Click(object sender, EventArgs e)
        {
            save = true;
        }

        void ImageLoad(Bitmap screen_img)
        {
            var str = compress.Compress(screen_img);
            str.Position = 0;
            var result = compress.Decompress(str);
            pictureBox1.Image = result;
            Text = str.Length.ToString();
            if(save)
            {
                save = false;
                result.Save("result.png", ImageFormat.Png);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            
        }
        
    }

    class EndlessStream : Stream
    {
        MemoryStream stream = new MemoryStream();
        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override long Length => stream.Length;

        public override long Position { get => stream.Position; set => stream.Position = value; }

        public override void Flush()
        {
            stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            stream.Write(buffer, offset, count);
        }
    }
}
