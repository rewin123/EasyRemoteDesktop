using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageTransferTest
{
    class FurieCompressor
    {
        public byte[,] array;
        int size;
        public FurieCompressor(int size)
        {
            this.size = size;
            array = new byte[size, size];
        }

        public void Compress(float lvl)
        {
            int delta = (int)(size / 2 * (1 - lvl));

        }
    }
}
