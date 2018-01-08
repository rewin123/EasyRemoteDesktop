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
using System.Threading;

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
        System.Windows.Forms.Timer rmd_timer = new System.Windows.Forms.Timer();

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

            rmd_timer.Interval = 20;
            rmd_timer.Tick += Rmd_timer_Elapsed;
            rmd_timer.Start();
            
        }


        byte[] buffer = new byte[20];
        object lock_rmd = new object();
        private void Rmd_timer_Elapsed(object sender, EventArgs e)
        {
            if (rmd_listener.Pending())
            {
                TcpClient client = rmd_listener.AcceptTcpClient();
                NetworkStream str = client.GetStream();
                int position = (int)formatter.Deserialize(str);
                int hash = (int)formatter.Deserialize(str);

                bool found = false;
                for (int i = 0; i < pairs.Count; i++)
                {
                    if (pairs[i].hash == hash)
                    {
                        found = true;
                        if (position == 0)
                            pairs[i].client = client;
                        else pairs[i].server = client;
                        break;
                    }
                }
                if (!found)
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
                    var serverStream = pair.server.GetStream();
                    var clientStream = pair.client.GetStream();
                    while (pair.server.Available >= buffer.Length)
                    {
                        serverStream.Read(buffer, 0, buffer.Length);
                        clientStream.Write(buffer, 0, buffer.Length);
                    }
                    if(pair.server.Available > 0)
                    {
                        int count = pair.server.Available;
                        if (count < buffer.Length)
                        {
                            serverStream.Read(buffer, 0, count);
                            clientStream.Write(buffer, 0, count);
                        }
                        else
                        {
                            ;
                        }
                    }
                    while (pair.client.Available >= buffer.Length)
                    {
                        clientStream.Read(buffer, 0, buffer.Length);
                        serverStream.Write(buffer, 0, buffer.Length);
                    }
                    if (pair.client.Available > 0)
                    {
                        int count = pair.client.Available;
                        if (count < buffer.Length)
                        {
                            clientStream.Read(buffer, 0, count);
                            serverStream.Write(buffer, 0, count);
                        }
                        else
                        {
                            ;
                        }
                    }
                }
            }
        }

        object lock_timer = new object();
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
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].stream.DataAvailable)
                {
                    object str = formatter.Deserialize(users[i].tcpClient.GetStream());
                    CallData data;
                    //if (str is CallData)
                    //    data = (CallData)str;
                     data = JsonConvert.DeserializeObject<CallData>((string)str);
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

            
        }
        

        void SendUsers(int id, string data)
        {
            SimpleUser[] s_users = new SimpleUser[users.Count];
            for (int i = 0; i < s_users.Length; i++)
                s_users[i] = new SimpleUser(users[i]);

            string send = Newtonsoft.Json.JsonConvert.SerializeObject(s_users);

            CallData callData = new CallData
            {
                methodName = "GetUsers",
                data = send
            };

            formatter.Serialize(users[id].tcpClient.GetStream(), JsonConvert.SerializeObject(callData));
        }

        void Disconect(int id, string data)
        {
            users.RemoveAt(id);
            UpdateClients.Invoke();
        }

        void ActivateRMD(int id, string data)
        {
            int index = users.FindIndex((user) => user.name == data);
            if (index != -1)
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
            else
            {
                CallData clientCall = new CallData
                {
                    methodName = "NotAllomRDM"
                };
                formatter.Serialize(users[id].tcpClient.GetStream(), Newtonsoft.Json.JsonConvert.SerializeObject(clientCall));
            }
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
