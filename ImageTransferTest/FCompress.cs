using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Serialization.Formatters.Binary;

namespace ImageTransferTest
{
    class FCompress
    {
         int size;
        FurieByte furieByte;
        BinaryFormatter formatter = new BinaryFormatter();
        public FCompress(int block_size = 8)
        {
            size = block_size;
            furieByte = new FurieByte(size);
        }

        public MemoryStream Compress(Bitmap map)
        {
            MemoryStream str = new MemoryStream();
            formatter.Serialize(str, map.Width);
            formatter.Serialize(str, map.Height);
            BinaryWriter wr = new BinaryWriter(str);

            int width = map.Width;
            int height = map.Height;
            BitmapData bitmapData = map.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var loc_arr = furieByte.raw;
            unsafe
            {
                byte* data = (byte*)bitmapData.Scan0.ToPointer();
                for (int y0 = 0; y0 < height; y0 += size)
                {
                    for (int x0 = 0; x0 < width; x0 += size)
                    {
                        #region Сжатие синего канала
                        for (int y = 0; y < size; y++)
                        {
                            for (int x = 0; x < size; x++)
                            {
                                loc_arr[x, y] = data[((y0 + y) * width + x + x0) * 3];
                            }
                        }
                        furieByte.Compress();
                        for (int y = 0; y < size; y++)
                        {
                            for (int x = 0; x < size; x++)
                            {
                                wr.Write(furieByte.cos[x, y]);
                            }
                        }
                        for (int y = 0; y < size; y++)
                        {
                            for (int x = 0; x < size; x++)
                            {
                                wr.Write(furieByte.sin[x, y]);
                            }
                        }
                        #endregion

                        #region Сжатие зеленого канала
                        for (int y = 0; y < size; y++)
                        {
                            for (int x = 0; x < size; x++)
                            {
                                loc_arr[x, y] = data[((y0 + y) * width + x + x0) * 3 + 1];
                            }
                        }
                        furieByte.Compress();
                        for (int y = 0; y < size; y++)
                        {
                            for (int x = 0; x < size; x++)
                            {
                                wr.Write(furieByte.cos[x, y]);
                            }
                        }
                        for (int y = 0; y < size; y++)
                        {
                            for (int x = 0; x < size; x++)
                            {
                                wr.Write(furieByte.sin[x, y]);
                            }
                        }
                        #endregion

                        #region Сжатие красного канала
                        for (int y = 0; y < size; y++)
                        {
                            for (int x = 0; x < size; x++)
                            {
                                loc_arr[x, y] = data[((y0 + y) * width + x + x0) * 3 + 2];
                            }
                        }
                        furieByte.Compress();
                        for (int y = 0; y < size; y++)
                        {
                            for (int x = 0; x < size; x++)
                            {
                                wr.Write(furieByte.cos[x, y]);
                            }
                        }
                        for (int y = 0; y < size; y++)
                        {
                            for (int x = 0; x < size; x++)
                            {
                                wr.Write(furieByte.sin[x, y]);
                            }
                        }
                        #endregion
                    }
                }

            }

            map.UnlockBits(bitmapData);

            return str;
        }

        public Bitmap Decompress(Stream str)
        {
            int width = (int)formatter.Deserialize(str);
            int height = (int)formatter.Deserialize(str);
            BinaryReader reader = new BinaryReader(str);
            Bitmap map = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData bitmapData = map.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            var loc_arr = furieByte.raw;
            var loc_cos = furieByte.cos;
            var loc_sin = furieByte.sin;
            unsafe
            {
                byte* data = (byte*)bitmapData.Scan0.ToPointer();
                for (int y0 = 0; y0 < height; y0 += size)
                {
                    for (int x0 = 0; x0 < width; x0 += size)
                    {
                        #region Расшифровка синего канала
                        for (int y = 0; y < size; y++)
                        {
                            for (int x = 0; x < size; x++)
                            {
                                loc_cos[x, y] = reader.ReadSByte();
                            }
                        }
                        for (int y = 0; y < size; y++)
                        {
                            for (int x = 0; x < size; x++)
                            {
                                loc_sin[x, y] = reader.ReadSByte();
                            }
                        }
                        furieByte.Decompress();
                        for (int y = 0; y < size; y++)
                        {
                            for (int x = 0; x < size; x++)
                            {
                                 data[((y0 + y) * width + x + x0) * 3] = loc_arr[x, y];
                            }
                        }

                        #endregion

                        #region Расшифровка зеленого канала
                        for (int y = 0; y < size; y++)
                        {
                            for (int x = 0; x < size; x++)
                            {
                                loc_cos[x, y] = reader.ReadSByte();
                            }
                        }
                        for (int y = 0; y < size; y++)
                        {
                            for (int x = 0; x < size; x++)
                            {
                                loc_sin[x, y] = reader.ReadSByte();
                            }
                        }
                        furieByte.Decompress();
                        for (int y = 0; y < size; y++)
                        {
                            for (int x = 0; x < size; x++)
                            {
                                data[((y0 + y) * width + x + x0) * 3 + 1] = loc_arr[x, y];
                            }
                        }

                        #endregion

                        #region Расшифровка красного канала
                        for (int y = 0; y < size; y++)
                        {
                            for (int x = 0; x < size; x++)
                            {
                                loc_cos[x, y] = reader.ReadSByte();
                            }
                        }
                        for (int y = 0; y < size; y++)
                        {
                            for (int x = 0; x < size; x++)
                            {
                                loc_sin[x, y] = reader.ReadSByte();
                            }
                        }
                        furieByte.Decompress();
                        for (int y = 0; y < size; y++)
                        {
                            for (int x = 0; x < size; x++)
                            {
                                data[((y0 + y) * width + x + x0) * 3 + 2] = loc_arr[x, y];
                            }
                        }

                        #endregion

                    }
                }

            }

            map.UnlockBits(bitmapData);

            return map;
        }
    }
}
