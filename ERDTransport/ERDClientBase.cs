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
using Newtonsoft.Json;

namespace ERDTransport
{
    public class ERDClientBase
    {
        TcpClient client;
        NetworkStream networkStream;
        BinaryFormatter formatter = new BinaryFormatter();
        System.Timers.Timer timer = new System.Timers.Timer(100);

        RMDServer server = null;
        string address = "";

        public ERDClientBase(string address)
        {
            this.address = address;
            client = new TcpClient(address, ERDOnlineBase.port);
            networkStream = client.GetStream();

            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (networkStream.DataAvailable)
            {
                string data = (string)formatter.Deserialize(networkStream);
                CallData callData = Newtonsoft.Json.JsonConvert.DeserializeObject<CallData>(data);
                switch (callData.methodName)
                {
                    case "StartRMDServer":
                        StartRMDServer(callData.data);
                        break;
                    case "AllowRMDClient":
                        AllowRMDClient(callData.data);
                        break;
                }
            }
        }

        void StartRMDServer(string data)
        {
            int hash = int.Parse(data);

            server = new RMDServer(address, hash);
        }

        void AllowRMDClient(string data)
        {
            int hash = int.Parse(data);
            RMDForm form = new RMDForm(address, hash);
            form.ShowDialog();
        }

        void StopRMDServer()
        {
            server.Stop();
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

            formatter.Serialize(networkStream, JsonConvert.SerializeObject(call));


            string data = (string)formatter.Deserialize(networkStream);
            SimpleUser[] users = Newtonsoft.Json.JsonConvert.DeserializeObject<SimpleUser[]>(data);
            return users;
        }

        public void RunRDP(SimpleUser user)
        {
            CallData callData = new CallData
            {
                methodName = "ActivateRMD",
                data = user.name
            };
            formatter.Serialize(networkStream, Newtonsoft.Json.JsonConvert.SerializeObject(callData));
            //Process.Start("Cmd.exe", @"/C mstsc.exe  " + user.addres.Split(':')[0]);
        }
        
        ~ERDClientBase()
        {
            try
            {
                client.GetStream().Close();
            }
            catch
            {

            }
        }
    }
}
