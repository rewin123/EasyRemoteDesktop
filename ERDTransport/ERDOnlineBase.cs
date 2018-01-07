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

namespace ERDTransport
{
    public class ERDOnlineBase
    {
        public static int port = 1999;
        public List<User> users = new List<User>();
        

        TcpListener listener;
        List<TcpClient> unnamed_clients = new List<TcpClient>();
        Timer list_timer = new Timer(100);

        public delegate void NewClientEvent(User client);
        public event NewClientEvent NewClient;

        BinaryFormatter formatter = new BinaryFormatter();

        public ERDOnlineBase()
        {
            listener = new TcpListener(port);
            listener.Start();

            list_timer.AutoReset = false;
            list_timer.Elapsed += Timer_Elapsed;
            list_timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            #region Принимаем новых польщователей
            if (listener.Pending())
            {
                TcpClient client = listener.AcceptTcpClient();
                unnamed_clients.Add(client);
            }
            #endregion

            MessageRead();

            list_timer.Start();
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
                }
            }
            #endregion

            for(int i = 0;i < users.Count;i++)
            {
                if(users[i].tcpClient.GetStream().DataAvailable)
                {
                    CallData data = (CallData)formatter.Deserialize(users[i].tcpClient.GetStream());
                    switch (data.methodName)
                    {
                        case "SendUsers":
                            SendUsers(i, data.data);
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

        ~ERDOnlineBase()
        {
            list_timer.Stop();
        }

        public void RunRDP(SimpleUser user)
        {
            Process.Start("Cmd.exe", @"/C mstsc.exe  " + user.addres.Split(':')[0]);
        }
    }
}
