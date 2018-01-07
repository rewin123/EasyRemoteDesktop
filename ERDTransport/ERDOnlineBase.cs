using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using Newtonsoft.Json;

namespace ERDTransport
{
    public class ERDOnlineBase
    {
        public static int port = 1999;
        public List<User> users = new List<User>();
        public List<RMDPair> pairs = new List<RMDPair>();

        TcpListener listener;
        TcpListener rmd_listener;
        List<TcpClient> unnamed_clients = new List<TcpClient>();
        System.Windows.Forms.Timer list_timer = new System.Windows.Forms.Timer();
        Timer rmd_timer = new Timer(10);

        int has_indexer = 1;

        public delegate void NewClientEvent(User client);
        public delegate void SomeAction();
        public event NewClientEvent NewClient;
        public event SomeAction UpdateClients;

        BinaryFormatter formatter = new BinaryFormatter();

        ERDClientBase serversClient;

        public ERDOnlineBase()
        {
            listener = new TcpListener(port);
            listener.Start();

            rmd_listener = new TcpListener(RMDServer.port);
            rmd_listener.Start();

            list_timer.Interval = 100;
            list_timer.Tick += Timer_Elapsed;
            list_timer.Start();

            rmd_timer.Elapsed += Rmd_timer_Elapsed;
            rmd_timer.Start();

            serversClient = new ERDClientBase("localhost");
            serversClient.Register("Server");
        }
        

        object lock_rmd = new object();
        private void Rmd_timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (lock_rmd)
            {
                if(rmd_listener.Pending())
                {
                    TcpClient client = rmd_listener.AcceptTcpClient();
                    NetworkStream str = client.GetStream();
                    int position = (int)formatter.Deserialize(str);
                    int hash = (int)formatter.Deserialize(str);

                    bool found = false;
                    for(int i = 0;i < pairs.Count;i++)
                    {
                        if(pairs[i].hash == hash)
                        {
                            found = true;
                            if (position == 0)
                                pairs[i].client = client;
                            else pairs[i].server = client;
                            break;
                        }
                    }
                    if(!found)
                    {
                        RMDPair pair = new RMDPair();
                        if (position == 0)
                            pair.client = client;
                        else pair.server = client;
                        pair.hash = hash;
                        pairs.Add(pair);
                    }
                    formatter.Serialize(str, 1);//говорим, что запрос принят
                }
                for (int i = 0; i < pairs.Count; i++)
                {
                    RMDPair pair = pairs[i];
                    if (pair.server != null && pair.client != null)
                    {
                        if (pair.server.GetStream().DataAvailable)
                        {
                            pair.server.GetStream().CopyTo(pair.client.GetStream());
                        }
                        if (pair.client.GetStream().DataAvailable)
                        {
                            pair.client.GetStream().CopyTo(pair.server.GetStream());
                        }
                    }
                }
            }
        }

        object lock_timer = new object();
        [STAThread]
        private void Timer_Elapsed(object sender, EventArgs e)
        {
            lock (lock_timer)
            {
                #region Принимаем новых пользователей
                if (listener.Pending())
                {
                    TcpClient client = listener.AcceptTcpClient();
                    unnamed_clients.Add(client);
                }
                #endregion

                MessageRead();
            }
        }

        void MessageRead()
        {
            #region Регистрируем новых пользователей
            for (int i = 0; i < unnamed_clients.Count; i++)
            {
                if (unnamed_clients[i].GetStream().DataAvailable)
                {
                    NetworkStream str = unnamed_clients[i].GetStream();
                    var reg_data = (RegistryMessage)formatter.Deserialize(str);
                    TcpClient client = unnamed_clients[i];
                    users.Add(new User(reg_data.name, ref client));
                    NewClient.Invoke(users.Last());

                    unnamed_clients.RemoveAt(i);
                    i--;
                }
            }
            #endregion

            for(int i = 0;i < users.Count;i++)
            {
                if(users[i].stream.DataAvailable)
                {
                    CallData data = JsonConvert.DeserializeObject<CallData>((string)formatter.Deserialize(users[i].tcpClient.GetStream()));
                    switch (data.methodName)
                    {
                        case "SendUsers":
                            SendUsers(i, data.data);
                            break;
                        case "Disconect":
                            Disconect(i, data.data);
                            break;
                        case "ActivateRMD":
                            ActivateRMD(i, data.data);
                            break;
                    }
                }
            }
        }
        

        void SendUsers(int id, string data)
        {
            SimpleUser[] s_users = new SimpleUser[users.Count];
            for (int i = 0; i < s_users.Length; i++)
                s_users[i] = new SimpleUser(users[i]);

            string send = Newtonsoft.Json.JsonConvert.SerializeObject(s_users);

            formatter.Serialize(users[id].tcpClient.GetStream(), send);
        }

        void Disconect(int id, string data)
        {
            users.RemoveAt(id);
            UpdateClients.Invoke();
        }

        void ActivateRMD(int id, string data)
        {
            has_indexer++;
            CallData clientCall = new CallData
            {
                methodName = "AllowRMDClient",
                data = has_indexer.ToString()
            };
            formatter.Serialize(users[id].tcpClient.GetStream(), Newtonsoft.Json.JsonConvert.SerializeObject(clientCall));

            clientCall.methodName = "StartRMDServer";
            formatter.Serialize(users[users.FindIndex((user) => user.name == data)].tcpClient.GetStream(), 
                Newtonsoft.Json.JsonConvert.SerializeObject(clientCall));
        }

        ~ERDOnlineBase()
        {
            list_timer.Stop();
        }

        public void RunRDP(SimpleUser user)
        {
            //Process.Start("Cmd.exe", @"/C mstsc.exe  /v:" + user.addres.Split(':')[0]);
        }
    }
}
