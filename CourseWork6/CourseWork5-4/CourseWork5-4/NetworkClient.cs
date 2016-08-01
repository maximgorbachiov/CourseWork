using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsersDataClasses;
using System.Net;
using System.Net.Sockets;
using BSONSerialization;

namespace CourseWork5_4
{
    class NetworkClient
    {
        static private NetworkClient netWorkClient = new NetworkClient();
        static public NetworkClient NetWorkClient { get { return netWorkClient; } }

        private BSONSerializer<UserStep> userStepSerializer = new BSONSerializer<UserStep>();
        private BSONSerializer<UsersTable> usersTableSerializer = new BSONSerializer<UsersTable>();
        private BSONSerializer<UserFieldData> userFieldDataSerializer = new BSONSerializer<UserFieldData>();
        private BSONSerializer<UsersConnectionData> userConnectionDataSerializer = new BSONSerializer<UsersConnectionData>();
        private TcpClient user;
        private const int serverUdpPort = 11100;

        public ServerConnectionData ServerInfo { get; private set; }

        public UsersTable DNSTable { get; set; }

        public UsersConnectionData Opponent { get; set; }

        public UsersConnectionData User { get; set; }


        private NetworkClient(){ }

        public void UdpConnect(UsersConnectionData userData)
        {
            byte[] userDataFromServer = new byte[10000];
            byte[] newUserDataFromServer;
            BSONSerializer<ServerConnectionData> serverConnectionDataSerializer = new BSONSerializer<ServerConnectionData>();
            UdpClient udpClient = new UdpClient(serverUdpPort);
            IPEndPoint remoteIpEndPoint = null;

            byte[] serverInfo = udpClient.Receive(ref remoteIpEndPoint);
            udpClient.Close();
            ServerInfo = (ServerConnectionData)serverConnectionDataSerializer.DeserializeMessage(serverInfo);

            IPAddress ipAddr = IPAddress.Parse(ServerInfo.IP);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, ServerInfo.Port);
            TcpClient client = new TcpClient();
            client.Connect(ipEndPoint);
            NetworkStream stream = client.GetStream();
            byte[] data = userConnectionDataSerializer.SerializeMessage(userData);
            stream.Write(data, 0, data.Length);
            int length = stream.Read(userDataFromServer, 0, userDataFromServer.Length);
            newUserDataFromServer = new byte[length];
            for (int i = 0; i < length; i++)
            {
                newUserDataFromServer[i] = userDataFromServer[i];
            }
            User = (UsersConnectionData)userConnectionDataSerializer.DeserializeMessage(newUserDataFromServer);
            client.Close();
        }

        public void SendStepToOpponent(UserStep step)
        {
            byte[] data = userStepSerializer.SerializeMessage(step);
            IPAddress ipAddr = IPAddress.Parse(Opponent.IP);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, Opponent.Port);
            user = new TcpClient();
            user.Connect(ipEndPoint);
            NetworkStream stream = user.GetStream();
            stream.Write(data, 0, data.Length);
        }

        public void GetUsersTableFromServer()
        {
            IPAddress ipAddr = IPAddress.Parse(ServerInfo.IP);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, ServerInfo.Port);
            TcpClient client = new TcpClient();
            byte[] message = Encoding.UTF8.GetBytes("GET_USERS_TABLE");
            client.Connect(ipEndPoint);
            NetworkStream stream = client.GetStream();
            stream.Write(message, 0, message.Length);
            DNSTable = ReceiveUsersTableMessage(stream);
            client.Close();
        }

        public void ConnectToOpponent(string opponentName)
        {
            Opponent = DNSTable.Table.Single(opponent => opponent.Name == opponentName);
            IPAddress ipAddr = IPAddress.Parse(Opponent.IP);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, Opponent.Port);
            user = new TcpClient();
            user.Connect(ipEndPoint);
            BSONSerializer<UserFieldData> fieldDataSerializer = new BSONSerializer<UserFieldData>();
            NetworkStream stream = user.GetStream();
            UserFieldData fieldData = new UserFieldData();
            fieldData.UsersShipsLocation = GameFieldCellsHandler.CellsHandler.UserBattleField;
            fieldData.UsersShipsRotation = GameFieldCellsHandler.CellsHandler.UserBattleShipsRotation;
            byte[] info = fieldDataSerializer.SerializeMessage(fieldData);
            stream.Write(info, 0, info.Length);
            //GameStream = user.GetStream();
        }

        public void LeaveGame()
        {
            if (user != null)
            {
                NetworkStream stream = user.GetStream();
                UsersConnectionData dataForClose = new UsersConnectionData();
                dataForClose.Port = -1;
                dataForClose.Name = User.Name;
                byte[] message = userConnectionDataSerializer.SerializeMessage(dataForClose);
                stream.Write(message, 0, message.Length);
                user.Close();
            }
        }

        private UsersTable ReceiveUsersTableMessage(NetworkStream stream)
        {
            byte[] usersTable = ReceiveMessage(stream);
            return (UsersTable)usersTableSerializer.DeserializeMessage(usersTable);
        }

        public UserStep ReceiveOpponentStep(NetworkStream stream)
        {
            byte[] opponentStep = ReceiveMessage(stream);
            return (UserStep)userStepSerializer.DeserializeMessage(opponentStep);
        }

        public UserFieldData ReceiveOpponentTables(NetworkStream stream)
        {
            byte[] opponentFieldData = ReceiveMessage(stream);
            return (UserFieldData)userFieldDataSerializer.DeserializeMessage(opponentFieldData);
        }

        public void CleanOpponentData()
        {
            Opponent = null;
        }

        private byte[] ReceiveMessage(NetworkStream stream)
        {
            byte[] fullData = new byte[10000];
            int size;
            size = stream.Read(fullData, 0, fullData.Length);
            byte[] data = new byte[size];
            for (int i = 0; i < size; i++)
            {
                data[i] = fullData[i];
            }
            return data;
        }
    }
}
