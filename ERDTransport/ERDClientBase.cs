using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;

namespace ERDTransport
{
    public class ERDClientBase
    {
        TcpClient client;
        NetworkStream networkStream;
        BinaryFormatter formatter = new BinaryFormatter();

        public ERDClientBase(string address)
        {
            client = new TcpClient(address, ERDOnlineBase.port);
            networkStream = client.GetStream();
        }

        public void Register(string name)
        {
            RegistryMessage message = new RegistryMessage
            {
                name = name
            };

            formatter.Serialize(networkStream, message);
        }

        public SimpleUser[] GetUsers()
        {
            CallData call = new CallData
            {
                methodName = "SendUsers"
            };

            formatter.Serialize(networkStream, call);


            string data = (string)formatter.Deserialize(networkStream);
            SimpleUser[] users = Newtonsoft.Json.JsonConvert.DeserializeObject<SimpleUser[]>(data);
            return users;
        }

        public void RunRDP(SimpleUser user)
        {
            Process.Start("Cmd.exe", @"/C mstsc.exe  " + user.addres.Split(':')[0]);
        }
        
        ~ERDClientBase()
        {
            client.GetStream().Close();
        }
    }
}
