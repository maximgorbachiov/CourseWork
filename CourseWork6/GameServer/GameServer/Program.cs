using System;
using System.Text;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using BSONSerialization;
using UsersDataClasses;

namespace GameServer
{
    class Program
    {
        static int port = 11100;
        static ServerConnectionData serverInfo;
        static UsersTable dnsTable = new UsersTable();
        static BSONSerializer<UsersTable> usersTableSerialier = new BSONSerializer<UsersTable>();
        static BSONSerializer<ServerConnectionData> serverConnectionDataSerializer = new BSONSerializer<ServerConnectionData>();
        static BSONSerializer<UsersConnectionData> usersConnectionDataSerializer = new BSONSerializer<UsersConnectionData>();

        [STAThread]
        static private void UdpSender()
        {
            UdpClient udpClient = new UdpClient();
            int i = 0;
            Console.WriteLine(serverInfo.IP);
            Console.WriteLine(serverInfo.Port);
            byte[] message = serverConnectionDataSerializer.SerializeMessage(serverInfo);
            while (true)
            {
                udpClient.Send(message, message.Length, new IPEndPoint(IPAddress.Broadcast, port));
                Console.WriteLine(i);
                Thread.Sleep(1000);
                i++;
            }
        }

        [STAThread]
        static private void SendCollection(NetworkStream stream)
        {
            UsersTable list = new UsersTable();
            list = dnsTable;
            byte[] message = usersTableSerialier.SerializeMessage(list);
            stream.Write(message, 0, message.Length);
        }

        [STAThread]
        static void Main(string[] args)
        {
            IPAddress ipAddr;
            IPEndPoint ipEndPoint;
            TcpListener listener;
            serverInfo = new ServerConnectionData();
            try
            {
                Console.WriteLine("Enter your IP");
                serverInfo.IP = Console.ReadLine(); // Вводим ip сервера
                Console.WriteLine("Enter port on which server start work");
                serverInfo.Port = Convert.ToInt32(Console.ReadLine()); // Вводим порт на котором сервер будет работать
                ipAddr = IPAddress.Parse(serverInfo.IP);
                ipEndPoint = new IPEndPoint(ipAddr, serverInfo.Port); // создаем точку соединения с сервером
                listener = new TcpListener(ipEndPoint);
                dnsTable.Table = new ObservableCollection<UsersConnectionData>();
                Thread sender = new Thread(new ThreadStart(UdpSender));
                sender.Start();

                listener.Start();
                // Начинаем слушать соединения
                while (true)
                {
                    Console.WriteLine("Waiting for connections on port {0}", ipEndPoint);
                    TcpClient client = listener.AcceptTcpClient();
                    NetworkStream tcpStream = client.GetStream();
                    string sendListCommand = "GET_USERS_TABLE";
                    Console.WriteLine(client.Client.RemoteEndPoint);
                    byte[] getMessage = new byte[1024];

                    int receiveMessageLength = tcpStream.Read(getMessage, 0, getMessage.Length);
                    byte[] temp = new byte[receiveMessageLength];
                    for (int j = 0; j < receiveMessageLength; j++)
                    {
                        temp[j] = getMessage[j];
                    }
                    if (Encoding.UTF8.GetString(temp) == sendListCommand) // если пользователь послал сообщение Получить таблицу
                    {
                        SendCollection(tcpStream);
                    }
                    else // В противном случае пользователь еще только подключается
                    {
                        UsersConnectionData userData = (UsersConnectionData)usersConnectionDataSerializer.DeserializeMessage(temp);
                        string endPoint = client.Client.RemoteEndPoint.ToString();
                        if (userData.Port != -1)
                        {
                            int i = 0;
                            while (endPoint[i] != ':')
                            {
                                userData.IP += endPoint[i];
                                i++;
                            }
                            dnsTable.Table.Add(userData);
                            byte[] userInfo = usersConnectionDataSerializer.SerializeMessage(userData);
                            tcpStream.Write(userInfo, 0, userInfo.Length);
                        }
                        else
                        {
                            for (int i = 0; i < dnsTable.Table.Count; i++)
                            {
                                if (dnsTable.Table[i].Name == userData.Name)
                                {
                                    dnsTable.Table.RemoveAt(i);
                                }
                            }
                        }
                    }

                    client.Close();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.ReadLine();
            }
        }
    }
}
