using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fast_Fourier_Transform;

namespace ImageTransferTest
{
    class FurieByte
    {
        public byte[,] raw;
        public float[,] cos;
        public float[,] sin;
        int size;
        float[,,,] coses;
        float[,,,] sines;

        float[,] loc_cos;
        float[,] loc_sin;
        public FurieByte(int size)
        {
            this.size = size;
            raw = new byte[size, size];
            cos = new float[size, size];
            sin = new float[size, size];
            loc_cos = new float[size, size];
            loc_sin = new float[size, size];

            coses = new float[size, size, size, size];
            sines = new float[size, size, size, size];

            for (int x_w = 0; x_w < size; x_w++)
            {
                for (int y_w = 0; y_w < size; y_w++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        for (int y = 0; y < size; y++)
                        {
                            coses[x_w, y_w, x, y] = (float)Math.Cos((x_w * x + y_w * y) * 2 * Math.PI / size);
                            sines[x_w, y_w, x, y] = (float)Math.Sin((x_w * x + y_w * y) * 2 * Math.PI / size);
                        }
                    }

                }
            }
        }
        
        public void Compress()
        {
            for(int x_w = 0;x_w < size;x_w++)
            {
                for (int y_w = 0; y_w < size; y_w++)
                {
                    float val = 0;
                    #region Вычисляем косинус
                    for(int x = 0;x < size;x++)
                    {
                        for(int y = 0;y < size;y++)
                        {
                            val += coses[x_w, y_w, x, y] * raw[x, y];
                        }
                    }
                    cos[x_w, y_w] = val / size / size;
                    #endregion

                    val = 0;
                    #region Вычисляем синус
                    for (int x = 0; x < size; x++)
                    {
                        for (int y = 0; y < size; y++)
                        {
                            val += sines[x_w, y_w, x, y] * raw[x, y];
                        }
                    }
                    sin[x_w, y_w] = val / size / size;
                    #endregion
                }
            }
        }

        public void Decompress()
        {
            for(int x = 0;x < size;x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float val = 0;
                    for (int x_w = 0; x_w < size; x_w++)
                    {
                        for (int y_w = 0; y_w < size; y_w++)
                        {
                            val += cos[x_w, y_w] * coses[x_w, y_w, x, y] + sin[x_w,y_w] * sines[x_w,y_w,x,y];
                        }
                    }
                    raw[x, y] = (byte)val;
                }
            }
        }
    }
}
