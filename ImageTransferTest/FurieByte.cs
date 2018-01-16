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
        float[] coses;
        float[] sines;

        float[,] loc_cos;
        COMPLEX[,] loc_comp;
        public FurieByte(int size)
        {
            this.size = size;
            raw = new byte[size, size];
            cos = new float[size, size];
            sin = new float[size, size];
            loc_cos = new float[size, size];
            loc_comp = new COMPLEX[size, size];

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    loc_comp[x, y] = new COMPLEX();
                }
            }

            coses = new float[size];
            sines = new float[size];

            for (int i = 0; i < 256; i++)
            {
                coses[i] = (float)Math.Cos(i*2*Math.PI / size);
                sines[i] = (float)Math.Cos(i * 2 * Math.PI / size);
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
                    cos[x, y] = (float)(loc_comp[x, y].real);
                    sin[x, y] = (float)(loc_comp[x, y].imag);
                }
            }
        }

        public void Decompress()
        {
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    loc_comp[x, y].real = cos[x, y];
                    loc_comp[x, y].imag = sin[x,y];
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
