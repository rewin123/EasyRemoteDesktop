using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERDTransport
{
    [Serializable]
    public class SimpleUser
    {
        public string name = "";
        public string addres = "";

        public SimpleUser()
        {

        }

        public SimpleUser(User user)
        {
            name = user.name;
            addres = user.tcpClient.Client.RemoteEndPoint.ToString();
        }

        public override string ToString()
        {
            return name + "|" + addres;
        }
    }
}
