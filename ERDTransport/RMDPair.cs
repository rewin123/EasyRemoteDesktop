using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ERDTransport
{
    public class RMDPair
    {
        public TcpClient client;
        public TcpClient server;
        public int hash = -1;
    }
}
