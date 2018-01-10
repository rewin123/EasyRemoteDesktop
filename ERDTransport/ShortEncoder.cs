using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

namespace ERDTransport
{
    /// <summary>
    /// Реализуем запись повторов шотов
    /// </summary>
    class ShortEncoder
    {
        public static void Encode(Stream input, Stream output)
        {
            MemoryStream local = new MemoryStream();
            GZipStream def = new GZipStream(local, CompressionLevel.Optimal);
            input.CopyTo(def);
            def.Close();
            var arr = local.ToArray();
            output.Write(arr, 0, arr.Length);
        }

        public static void Decode(Stream input, Stream output)
        {
            GZipStream def = new GZipStream(input, CompressionMode.Decompress);
            def.CopyTo(output);
            //def.Close();
        }
    }
}
