using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fast_Fourier_Transform;

namespace ImageTransferTest
{
    class FurieCompressor
    {
        public byte[,] raw;
        public byte[,] cos;
        public byte[,] sin;
        int size;
        float[] coses = new float[256];
        float[] sines = new float[256];

        float[,] loc_cos;
        float[,] loc_sin;
        public FurieCompressor(int size)
        {
            this.size = size;
            raw = new byte[size, size];
            cos = new byte[size, size];
            sin = new byte[size, size];
            loc_cos = new float[size, size];
            loc_sin = new float[size, size];

            for(int i = 0;i < 256;i++)
            {
                coses[i] = (float)Math.Cos(i);
                sines[i] = (float)Math.Sin(i);
            }
        }

        public void FT()
        {
            
        }
    }
}
