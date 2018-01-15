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
        public sbyte[,] cos;
        public sbyte[,] sin;
        int size;
        float[] coses = new float[256];

        float[,] loc_cos;
        COMPLEX[,] loc_comp;
        public FurieByte(int size)
        {
            this.size = size;
            raw = new byte[size, size];
            cos = new sbyte[size, size];
            sin = new sbyte[size, size];
            loc_cos = new float[size, size];
            loc_comp = new COMPLEX[size, size];

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    loc_comp[x, y] = new COMPLEX();
                }
            }

            for (int i = 0; i < 256; i++)
            {
                coses[i] = (float)Math.Cos(i);
            }
        }
        
        public void Compress()
        {
            for(int x = 0;x < size; x++)
            {
                for(int y = 0;y < size;y++)
                {
                    loc_comp[x, y].real = raw[x, y];
                    loc_comp[x, y].imag = 0;
                }
            }

            loc_comp = FFT.FFT2D(loc_comp, size, size, 1);
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    cos[x, y] = (sbyte)(loc_comp[x, y].real / 2);
                    sin[x, y] = (sbyte)(loc_comp[x, y].imag / 2);
                }
            }
        }

        public void Decompress()
        {
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    loc_comp[x, y].real = cos[x, y] * 2;
                    loc_comp[x, y].imag = sin[x,y] * 2;
                }
            }

            loc_comp = FFT.FFT2D(loc_comp, size, size, -1);
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    raw[x, y] = (byte)loc_comp[x, y].real;
                }
            }
        }
    }
}
