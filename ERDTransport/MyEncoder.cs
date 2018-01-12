using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ERDTransport
{
    public class MyEncoder
    {
        int rect_size = 64;
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
                for (int i = 0; i < blocks.Count; i++)
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
            for (int x = block.x; x < endX; x += 2)
            {
                for (int y = block.y; y < endY; y += 2)
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
                //pos один расчет положения в циклах
                int pos;

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
                            pos = (y * width + x) * 2;
                            b_point[pos] = (byte)str.ReadByte();
                            b_point[pos + 1] = (byte)str.ReadByte();
                        }
                    }
                }

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
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
            for (int x = 0; x < width; x += rect_size)
            {
                for (int y = 0; y < height; y += rect_size)
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
    public class Metablock
    {
        public int x = 0;
        public int y = 0;
        public int width = 0;
        public int height = 0;
    }

    [Serializable]
    public class Message
    {
        public int blockIndex = 0;
        public int bytes = 0;
    }
}
