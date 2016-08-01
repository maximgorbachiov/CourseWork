using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UsersDataClasses;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.ObjectModel;

namespace CourseWork5_4
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int serverUdpPort = 11100;
        private bool isHandlersWereSetted = false;
        public delegate void VoidFunctionsDelegate();
        public delegate void CreateMessageboxDelegate(string message);
        public delegate void DrawOpponentStepDelegate(UserStep opponentStep);
        public delegate void ButtonEnabledDelegate(bool enabled);
        
        private bool isGameContinue = false;
        private bool isUserCanGo = false;
        private bool isServerConnected = false;
        private bool isOpponentConnected = false;

        int countOfFourDecksShips = 1, countOfThreeDecksShips = 2, countOfTwoDecksShips = 3, countOfOneDecksShips = 4;

        public MainWindow()
        {
            InitializeComponent();
            InitializeGrid(GameFieldPrototype, true);
            GameFieldCellsHandler.CellsHandler.SetGameFone(MainMenuGrid);
            GameFieldCellsHandler.CellsHandler.SetGameFone(GameField);
            GameFieldCellsHandler.CellsHandler.SetGameFone(MainEditGrid);
        }

        private void ImageMouseEnter(object sender, MouseEventArgs e)
        {
            GameFieldCellsHandler.CellsHandler.ImageMouseEnter(sender, e);
        }

        private void ImageMouseLeave(object sender, MouseEventArgs e)
        {
            GameFieldCellsHandler.CellsHandler.ImageMouseLeave(sender, e);
        }

        private void ImageMouseDown(object sender, MouseEventArgs e)
        {
            if (isUserCanGo)
            {
                UserStep step = GameFieldCellsHandler.CellsHandler.ImageMouseDown(sender, e);
                if (GameFieldCellsHandler.CellsHandler.IsWhetherTheUserCanAttackAgain == 2) // если нажали на корабль
                {
                    SendDataToOpponent(step);
                }
                else
                {
                    if (GameFieldCellsHandler.CellsHandler.IsWhetherTheUserCanAttackAgain == 0) // если нажали на корабль
                    {
                        SendDataToOpponent(step);
                        isUserCanGo = false;
                    }
                }
                if (step.AdditionalInfo == "YOU_LOOSE")
                {
                    //CleanData();
                    //Return();
                    //NetworkClient.NetWorkClient.CleanOpponentData();
                    //listenerThread.Abort();
                    //StartListenerThread();
                }
            }
        }

        private void ImageMouseEnterAndChecked(object sender, MouseEventArgs e)
        {
            GameFieldCellsHandler.CellsHandler.ImageMouseEnterAndChecked(sender, e);
        }

        private void ImageMouseLeaveAndChecked(object sender, MouseEventArgs e)
        {
            GameFieldCellsHandler.CellsHandler.ImageMouseLeaveAndChecked(sender, e);
        }

        private void ImageMouseDownAndChecked(object sender, MouseEventArgs e)
        {
            GameFieldCellsHandler.CellsHandler.ImageMouseDownAndChecked(sender, e);
            if (GameFieldCellsHandler.CellsHandler.IsShipCanPlace)
            {
                ResetEvents();
            }
        }
        
        private void InitializeGrid(Grid grid, bool isGridBelongsToUser)
        {
            GameFieldCellsHandler.CellsHandler.InitializeGrid(grid, isGridBelongsToUser);
        }

        public void GetUsersTable()
        {
            VoidFunctionsDelegate updateDelegate = UpdateOpponentsContent;
            while (true)
            {
                NetworkClient.NetWorkClient.GetUsersTableFromServer();
                Dispatcher.BeginInvoke(updateDelegate);
                Thread.Sleep(10000);
            }
        }

        private void SendDataToOpponent(UserStep step)
        {
            NetworkClient.NetWorkClient.SendStepToOpponent(step);
        }

        private void ButtonConnectionToServer_Click(object sender, RoutedEventArgs e)
        {
            UsersConnectionData connectionData = new UsersConnectionData();
            if ((UserPort.Text != "") && (UserLogin.Text != ""))
            {
                connectionData.Port = Convert.ToInt32(UserPort.Text);
                connectionData.Name = UserLogin.Text;
                NetworkClient client = NetworkClient.NetWorkClient;
                client.UdpConnect(connectionData);
                isGameContinue = true;
                Thread tableGetThread = new Thread(new ThreadStart(GetUsersTable));
                tableGetThread.Start();
                isServerConnected = true;
                StartListenerThread();
            }
            else
            {
                GameFieldCellsHandler.CreateMessageBox("Check your port or login");
            }
        }

        private void ButtonStartGame_Click(object sender, RoutedEventArgs e)
        {
            if (isServerConnected && isOpponentConnected)
            {
                TabGame.Visibility = Visibility.Visible;
                TabGame.Focus();
                TabEdit.Visibility = Visibility.Collapsed;
                TabMenu.Visibility = Visibility.Collapsed;
                InitializeGrid(UserField, true);
                InitializeGrid(OpponentField, false);
                isUserCanGo = (bool)IsUserFirst.IsChecked;
                foreach (Image image in OpponentField.Children)
                {
                    image.MouseEnter += ImageMouseEnter;
                    image.MouseLeave += ImageMouseLeave;
                    image.MouseDown += ImageMouseDown;
                }
            }
            else
            {
                GameFieldCellsHandler.CreateMessageBox("Check your connection to server or opponent");
            }
        }

        private void ButtonFourDeck_Click(object sender, RoutedEventArgs e)
        {
            if (countOfFourDecksShips > 0)
            {
                GameFieldCellsHandler.CellsHandler.CurrentShip = new Ship(4);
                GameFieldCellsHandler.CellsHandler.IsShipCanPlace = true;
                SetEvents();
                countOfFourDecksShips--;
                LabelLeftFourDeckShips.Content = "-" + countOfFourDecksShips.ToString();
            }
        }

        private void ButtonThreeDeck_Click(object sender, RoutedEventArgs e)
        {
            if (countOfThreeDecksShips > 0)
            {
                GameFieldCellsHandler.CellsHandler.CurrentShip = new Ship(3);
                GameFieldCellsHandler.CellsHandler.IsShipCanPlace = true;
                SetEvents();
                countOfThreeDecksShips--;
                LabelLeftThreeDeckShips.Content = "-" + countOfThreeDecksShips.ToString();
            }
        }

        private void ButtonTwoDeck_Click(object sender, RoutedEventArgs e)
        {
            if (countOfTwoDecksShips > 0)
            {
                GameFieldCellsHandler.CellsHandler.CurrentShip = new Ship(2);
                GameFieldCellsHandler.CellsHandler.IsShipCanPlace = true;
                SetEvents();
                countOfTwoDecksShips--;
                LabelLeftTwoDeckShips.Content = "-" + countOfTwoDecksShips.ToString();
            }
        }

        private void ButtonOneDeck_Click(object sender, RoutedEventArgs e)
        {
            if (countOfOneDecksShips > 0)
            {
                GameFieldCellsHandler.CellsHandler.CurrentShip = new Ship(1);
                GameFieldCellsHandler.CellsHandler.IsShipCanPlace = true;
                SetEvents();
                countOfOneDecksShips--;
                LabelLeftOneDeckShips.Content = "-" + countOfOneDecksShips.ToString();
            }
        }

        private void SetEvents()
        {
            if (!isHandlersWereSetted)
            {
                foreach (Image image in GameFieldPrototype.Children)
                {
                    image.MouseEnter += ImageMouseEnterAndChecked;
                    image.MouseLeave += ImageMouseLeaveAndChecked;
                    image.MouseDown += ImageMouseDownAndChecked;
                }
                isHandlersWereSetted = true;
            }
        }

        private void ResetEvents()
        {
            if (isHandlersWereSetted)
            {
                foreach (Image image in GameFieldPrototype.Children)
                {
                    image.MouseEnter -= ImageMouseEnterAndChecked;
                    image.MouseLeave -= ImageMouseLeaveAndChecked;
                    image.MouseDown -= ImageMouseDownAndChecked;
                }
                isHandlersWereSetted = false;
            }
        }

        private void ButtonRotation_Click(object sender, RoutedEventArgs e)
        {
            List<Button> buttons = new List<Button>();
            List<Image> images = new List<Image>();
            buttons.Add(ButtonFourDeck);
            buttons.Add(ButtonThreeDeck);
            buttons.Add(ButtonTwoDeck);
            buttons.Add(ButtonOneDeck);
            for (int i = 0; i < buttons.Count; i++)
            {
                Grid grid = (Grid)buttons[i].Content;
                foreach(var child in grid.Children)
                {
                    if (child is Image)
                    {
                        images.Add((Image)child);
                    }
                }
            }
            images.Add((Image)ButtonRotation.Content);
            GameFieldCellsHandler.CellsHandler.RotateShips(images);
        }

        public void UpdateOpponentsContent()
        {
            ObservableCollection<string> opponents = new ObservableCollection<string>();
            for (int i = 0; i < NetworkClient.NetWorkClient.DNSTable.Table.Count; i++)
            {
                if (NetworkClient.NetWorkClient.DNSTable.Table[i].Name != NetworkClient.NetWorkClient.User.Name)
                {
                    opponents.Add(NetworkClient.NetWorkClient.DNSTable.Table[i].Name);
                }
            }
            ListboxOpponents.ItemsSource = opponents;
        }

        public void StartListenerThread()
        {
            Thread listenerThread = new Thread(new ThreadStart(ListenerThreadWork));
            listenerThread.Start();
        }

        public void ListenerThreadWork()
        {
            IPAddress ipAddr = IPAddress.Parse(NetworkClient.NetWorkClient.User.IP);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, (NetworkClient.NetWorkClient.User.Port));
            TcpListener listener = new TcpListener(ipEndPoint);
            ButtonEnabledDelegate buttonEnabledDelegate = ButtonChangeEnabled;
            Dispatcher.BeginInvoke(buttonEnabledDelegate, new object[] { false });
            bool isUserGetTables = false;

            listener.Start();
            // Начинаем слушать соединения
            while (isGameContinue)
            {
                TcpClient client = listener.AcceptTcpClient();
                NetworkStream tcpStream = client.GetStream();

                if (isUserGetTables)
                {
                    string endPoint = client.Client.RemoteEndPoint.ToString(), ip = "";
                    int i = 0;
                    while (endPoint[i] != ':')
                    {
                        ip += endPoint[i];
                        i++;
                    }
                    if (ip == NetworkClient.NetWorkClient.Opponent.IP)
                    {
                        UserStep opponentStep = NetworkClient.NetWorkClient.ReceiveOpponentStep(tcpStream);
                        if (opponentStep.AdditionalInfo == null)
                        {
                            isGameContinue = MakeOpponentStep(opponentStep);
                            isUserCanGo = true;
                        }
                        else
                        {
                            if (opponentStep.AdditionalInfo == "KILLED")
                            {
                                isGameContinue = MakeOpponentStep(opponentStep);
                                isUserCanGo = false;
                            }
                            else
                            {
                                CreateMessageboxDelegate opponentWin = GameFieldCellsHandler.CreateMessageBox;
                                VoidFunctionsDelegate returnDelegate = Return;
                                VoidFunctionsDelegate cleanDataDelegate = CleanData;
                                if (opponentStep.AdditionalInfo == "YOU_LOOSE")
                                {
                                    MakeOpponentStep(opponentStep);
                                    NetworkClient.NetWorkClient.SendStepToOpponent(new UserStep { AdditionalInfo = "OK" });
                                    Dispatcher.BeginInvoke(opponentWin, new object[] { "You loose" });
                                }                                
                                //else
                                //{
                                    //NetworkClient.NetWorkClient.SendStepToOpponent(new UserStep { AdditionalInfo = "OK"});
                                  //  Dispatcher.BeginInvoke(opponentWin, new object[] { "Opponent leave game" });
                                //}
                                //if (opponentStep.AdditionalInfo != "OK")
                                //{
                                //    NetworkClient.NetWorkClient.SendStepToOpponent(new UserStep { AdditionalInfo = "OK" });
                                //}
                                Dispatcher.BeginInvoke(buttonEnabledDelegate, new object[] { true });
                                isGameContinue = false;
                                //NetworkClient.NetWorkClient.CleanOpponentData();
                                //Dispatcher.BeginInvoke(cleanDataDelegate);
                                //Dispatcher.BeginInvoke(returnDelegate);
                            }
                        }
                    }
                    client.Close();
                }
                else
                {
                    UserFieldData opponentFieldData = NetworkClient.NetWorkClient.ReceiveOpponentTables(tcpStream);
                    GameFieldCellsHandler.CellsHandler.OpponentBattleField = opponentFieldData.UsersShipsLocation;
                    GameFieldCellsHandler.CellsHandler.OpponentBattleShipsRotation = opponentFieldData.UsersShipsRotation;
                    isUserGetTables = true;
                }
            }
            listener.Stop();
            isGameContinue = true;
            isUserGetTables = false;
            isOpponentConnected = false;
        }

        public bool MakeOpponentStep(UserStep opponentStep)
        {
            try
            {
                DrawOpponentStepDelegate drawer = DrawOpponentStep;
                Dispatcher.BeginInvoke(drawer, new object[] { opponentStep });
                return true;
            }
            catch(Exception exc)
            {
                MessageBox.Show(exc.Message);
                return false;
            }
        }

        public void DrawOpponentStep(UserStep opponentStep)
        {
            GameFieldCellsHandler.CellsHandler.MakeOpponentStep(opponentStep, UserField);
        }

        private void ButtonChooseOpponent_Click(object sender, RoutedEventArgs e)
        {
            if (((string)ListboxOpponents.SelectedItem != null) && ((string)ListboxOpponents.SelectedItem != ""))
            {
                Thread.Sleep(100); // чтоб прослушивание успело начатся
                NetworkClient.NetWorkClient.ConnectToOpponent((string)ListboxOpponents.SelectedItem);
                isOpponentConnected = true;
            }
            else
            {
                GameFieldCellsHandler.CreateMessageBox("Check your opponent choice");
            }
        }

        private void Return()
        {
            TabGame.Visibility = Visibility.Collapsed;
            TabEdit.Visibility = Visibility.Visible;
            TabMenu.Visibility = Visibility.Visible;
            TabMenu.Focus();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GameFieldCellsHandler.CellsHandler.CleanData();
            NetworkClient.NetWorkClient.LeaveGame();
        }

        private void CleanData()
        {
            GameFieldCellsHandler.CellsHandler.CleanData();
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            Return();
            GameFieldCellsHandler.CellsHandler.CleanData();
            NetworkClient.NetWorkClient.CleanOpponentData();
            Thread.Sleep(100);
            countOfFourDecksShips = 1;
            countOfThreeDecksShips = 2;
            countOfTwoDecksShips = 3;
            countOfOneDecksShips = 4;
            LabelLeftOneDeckShips.Content = "-" + countOfOneDecksShips.ToString();
            LabelLeftTwoDeckShips.Content = "-" + countOfTwoDecksShips.ToString();
            LabelLeftThreeDeckShips.Content = "-" + countOfThreeDecksShips.ToString();
            LabelLeftFourDeckShips.Content = "-" + countOfFourDecksShips.ToString();
            InitializeGrid(GameFieldPrototype, true);
            StartListenerThread();
        }

        private void ButtonChangeEnabled(bool enabled)
        {
            ButtonBack.IsEnabled = enabled;
        }
    }
}
