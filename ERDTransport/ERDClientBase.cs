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
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        RMDServer server = null;
        string address = "";

        public bool exterCall = true;

        public SimpleUser[] users = new SimpleUser[0];

        public ERDClientBase(string address)
        {
            this.address = address;
            client = new TcpClient(address, ERDOnlineBase.port);
            networkStream = client.GetStream();

            timer.Interval = 100;
            timer.Tick += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, EventArgs e)
        {
            if (networkStream.DataAvailable)
            {
                string data = (string)formatter.Deserialize(networkStream);
                CallData callData = JsonConvert.DeserializeObject<CallData>(data);
                switch (callData.methodName)
                {
                    case "StartRMDServer":
                        StartRMDServer(callData.data);
                        break;
                    case "AllowRMDClient":
                        AllowRMDClient(callData.data);
                        break;
                    case "NotAllomRDM":
                        NotAllomRDM(callData.data);
                        break;
                    case "GetUsers":
                        GetUsers(callData.data);
                        break;
                }
            }
        }

        void GetUsers(string data)
        {
            users = JsonConvert.DeserializeObject<SimpleUser[]>(data);
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

        void NotAllomRDM(string data)
        {
            timer.Start();
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

        public void UpdateUsers()
        {
            CallData call = new CallData
            {
                methodName = "SendUsers"
            };

            formatter.Serialize(networkStream, JsonConvert.SerializeObject(call));
            
        }

        public void RunRDP(SimpleUser user)
        {
            CallData callData = new CallData
            {
                methodName = "ActivateRMD",
                data = user.name
            };
            formatter.Serialize(networkStream, JsonConvert.SerializeObject(callData));
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
