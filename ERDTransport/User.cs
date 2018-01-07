using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace ERDTransport
{
    public class User
    {
        public string name = "Аноним";

        [NonSerialized]
        public TcpClient tcpClient;

        public User()
        {
            
        }

        public User(string name, ref TcpClient tcpClient)
        {
            this.name = name;
            this.tcpClient = tcpClient;
        }

        public override string ToString()
        {
            return name + "|" + tcpClient.Client.LocalEndPoint.ToString();
        }
    }
}
