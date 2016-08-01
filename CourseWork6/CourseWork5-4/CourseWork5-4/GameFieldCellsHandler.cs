using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using UsersDataClasses;

namespace CourseWork5_4
{
    class GameFieldCellsHandler
    {
        private static GameFieldCellsHandler handler = new GameFieldCellsHandler();
        private BitmapImage mouseEnterImage = new BitmapImage();
        private Image enteredImage;
        private BitmapImage imageContent;
        private int[,] userBattleField;
        private int[,] opponentBattleField;
        private bool[,] userBattleShipsRotation;
        private bool[,] opponentBattleShipsRotation;
        private bool isShipCanPlace = true;
        private bool isShipsRotate = false;
        private int isWhetherTheUserCanAttackAgain = 0;

        int sideLength = 10, liveDeck = 2, dieDeck = 1, sea = 0;
        string liveDeckSourceHorizontal = "/Resources/ship6.jpg", dieDeckSourceHorizontal = "/Resources/ship66.jpg", seaSource = "/Resources/water.jpg", cross = "/Resources/cross_medium.jpg";
        string liveDeckSourceVertical = "/Resources/ship6rotate.jpg", dieDeckSourceVertical = "/Resources/ship66rotate.jpg", point = "/Resources/dot_small.jpg";
        List<Image> currentShipDecks = new List<Image>();
        List<BitmapImage> enteredImagesContents = new List<BitmapImage>();

        static public GameFieldCellsHandler CellsHandler { get { return handler; } }
        public int[,] UserBattleField { get { return userBattleField; } }
        public bool[,] UserBattleShipsRotation { get { return userBattleShipsRotation; } }
        public int[,] OpponentBattleField { set { opponentBattleField = value; } }
        public bool[,] OpponentBattleShipsRotation { set { opponentBattleShipsRotation = value; } }
        public Ship CurrentShip { get; set; }
        public bool IsShipCanPlace { get { return isShipCanPlace; } set { isShipCanPlace = value; } }
        public int IsWhetherTheUserCanAttackAgain { get { return isWhetherTheUserCanAttackAgain; } }

        private GameFieldCellsHandler()
        {
            mouseEnterImage = CreateBitmap(cross);
            userBattleField = new int[sideLength, sideLength];
            opponentBattleField = new int[sideLength, sideLength];
            userBattleShipsRotation = new bool[sideLength, sideLength];
            opponentBattleShipsRotation = new bool[sideLength, sideLength];

            for (int i = 0; i < sideLength; i++)
            {
                for (int j = 0; j < sideLength; j++)
                {
                    userBattleField[i, j] = 0;
                    opponentBattleField[i, j] = 0;
                    userBattleShipsRotation[i, j] = false;
                    opponentBattleShipsRotation[i, j] = false;
                }
            }
        }

        public void SetGameFone(Grid grid)
        {
            string fone = "Resources/5.jpg";
            ImageBrush myBrush = new ImageBrush();
            myBrush.ImageSource = new BitmapImage(new Uri(fone, UriKind.RelativeOrAbsolute)) { CreateOptions = BitmapCreateOptions.IgnoreImageCache };
            grid.Background = myBrush;
        }

        public void InitializeGrid(Grid grid, bool isGridBelongsToUser)
        {
            string liveDeckSource = "", localSeaSource = seaSource;
            int[,] battleArea = userBattleField;
            bool[,] shipsRotation = userBattleShipsRotation;

            // если заполняем поле пользователя, то используем его матрицу расстановки и рисуем его корабли,
            // иначе матрицу противника и рисуем только море
            if (!isGridBelongsToUser)
            {
                battleArea = opponentBattleField;
                shipsRotation = opponentBattleShipsRotation;
                localSeaSource = seaSource; 
            }

            for (int i = 0; i < sideLength; i++)
            {
                for (int j = 0; j < sideLength; j++)
                {
                    Image image = new Image();
                    liveDeckSource = (shipsRotation[i, j]) ? liveDeckSourceVertical : liveDeckSourceHorizontal ;
                    if (!isGridBelongsToUser)
                    {
                        liveDeckSource = seaSource;
                    }
                    image.Source = (battleArea[i, j] == liveDeck) ? CreateBitmap(liveDeckSource) : CreateBitmap(localSeaSource);
                    image.Stretch = Stretch.Fill;
                    image.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                    image.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    image.MaxWidth = 40;
                    image.MaxHeight = 40;
                    Grid.SetRow(image, i);
                    Grid.SetColumn(image, j);
                    grid.Children.Add(image);
                }
            }
        }

        public void ImageMouseEnter(object sender, MouseEventArgs e)
        {
            enteredImage = (Image)sender;
            imageContent = (BitmapImage)enteredImage.Source;
            enteredImage.Source = mouseEnterImage;
        }

        public void ImageMouseLeave(object sender, MouseEventArgs e)
        {
            enteredImage.Source = imageContent;
        }

        public UserStep ImageMouseDown(object sender, MouseEventArgs e)
        {
            UserStep step = new UserStep();
            string dieDeckSource = "", source = "";
            int position = ((Grid)enteredImage.Parent).Children.IndexOf(enteredImage);
            int number = opponentBattleField[position / sideLength, position % sideLength];

            dieDeckSource = (isShipsRotate) ? dieDeckSourceVertical : dieDeckSourceHorizontal;
            if (number == liveDeck)
            {
                source = dieDeckSource;
                opponentBattleField[position / sideLength, position % sideLength] = dieDeck;
                step.AdditionalInfo = "KILLED";
                isWhetherTheUserCanAttackAgain = liveDeck;
            }
            else
            {
                if (number == dieDeck)
                {
                    source = dieDeckSource;
                    isWhetherTheUserCanAttackAgain = dieDeck;
                }
                else
                {
                    source = point;
                    isWhetherTheUserCanAttackAgain = sea;
                }
            }

            BitmapImage imageOfKilledShip = CreateBitmap(source);
            enteredImage.Source = imageOfKilledShip;
            imageContent = imageOfKilledShip; // чтобы при событии ImageMouseLeave не изменила картинку на предыдущую
            
            if (!CheckGameContinued()) // если нет у противника живых кораблей мы победили
            {
                MessageBox.Show("You Win");
                step.AdditionalInfo = "YOU_LOOSE";
            }
            step.TheIndexOfAttackCell = position;
            return step;
        }

        // Отображение корабля при наведении на клетку на макете будущей карты боя
        public bool ImageMouseEnterAndChecked(object sender, MouseEventArgs e)
        {
            Image image = (Image)sender;
            int position = ((Grid)image.Parent).Children.IndexOf(image);
            int i = position / sideLength, j = position % sideLength;
            bool rotation = isShipsRotate;
            string liveDeckSourceLocal = "", dieDeckSourceLocal = ""; 

            if (rotation) // если корабль пытается расположится вертикально
            {
                for (int k = 0; k < CurrentShip.CountOfDecks; k++)
                {
                    // если корабль не выходит за пределы поля по оси Y
                    isShipCanPlace = (i + k != sideLength) ? CheckShipPlacement(i + k, j) : false;
                    if (!isShipCanPlace) 
                        break;
                }
                liveDeckSourceLocal = liveDeckSourceVertical;
                dieDeckSourceLocal = dieDeckSourceVertical;
            }
            else // если горизонтально
            {
                for (int k = 0; k < CurrentShip.CountOfDecks; k++)
                {
                    // если корабль не выходит за пределы поля по оси X
                    isShipCanPlace = (j + k != sideLength) ? CheckShipPlacement(i, j + k) : false;
                    if (!isShipCanPlace)
                        break;
                }
                liveDeckSourceLocal = liveDeckSourceHorizontal;
                dieDeckSourceLocal = dieDeckSourceHorizontal;
            }

            // если корабль можно разместить
            return (isShipCanPlace) ? ViewShip(image, position, liveDeckSourceLocal, rotation) : ViewShip(image, position, dieDeckSourceLocal, rotation);
        }

        public void ImageMouseLeaveAndChecked(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < currentShipDecks.Count; i++)
            {
                currentShipDecks[i].Source = enteredImagesContents[i];
            }
            enteredImagesContents.Clear();
            currentShipDecks.Clear();
        }

        public void ImageMouseDownAndChecked(object sender, MouseEventArgs e)
        {
            int i, j, position; 
            if (isShipCanPlace)
            {
                for (int k = 0; k < currentShipDecks.Count; k++)
                {
                    position = ((Grid)currentShipDecks[k].Parent).Children.IndexOf(currentShipDecks[k]);
                    i = position / sideLength;
                    j = position % sideLength;
                    userBattleField[i, j] = liveDeck;
                    userBattleShipsRotation[i, j] = isShipsRotate;
                }
                enteredImagesContents.Clear();
                currentShipDecks.Clear();
            }
        }

        public void MakeOpponentStep(UserStep opponentStep, Grid userGrid)
        {
            int number = 0;
            foreach(Image image in userGrid.Children)
            {
                if (number == opponentStep.TheIndexOfAttackCell)
                {
                    int i = number / sideLength, j = number % sideLength;
                    if ((userBattleField[i, j] == liveDeck) || (userBattleField[i, j] == dieDeck))
                    {
                        userBattleField[i, j] = dieDeck;
                        if (userBattleShipsRotation[i, j])
                        {
                            image.Source = CreateBitmap(dieDeckSourceVertical);
                        }
                        else
                        {
                            image.Source = CreateBitmap(dieDeckSourceHorizontal);
                        }
                    }
                    else
                    {
                        image.Source = CreateBitmap(point);
                    }
                    break;
                }
                number++;
            }
        }

        public void RotateShips(List<Image> images)
        {
            isShipsRotate = !isShipsRotate;
            string liveDeckSource = (isShipsRotate) ? liveDeckSourceVertical : liveDeckSourceHorizontal;
            BitmapImage fourDeckShipRotate = CreateBitmap(liveDeckSource);
            BitmapImage threeDeckShipRotate = CreateBitmap(liveDeckSource);
            BitmapImage twoDeckShipRotate = CreateBitmap(liveDeckSource);
            BitmapImage oneDeckShipRotate = CreateBitmap(liveDeckSource);
            BitmapImage rotationImage = CreateBitmap(liveDeckSource);
            images[0].Source = fourDeckShipRotate;
            images[1].Source = threeDeckShipRotate;
            images[2].Source = twoDeckShipRotate;
            images[3].Source = oneDeckShipRotate;
            images[4].Source = rotationImage;
        }

        public void CleanData()
        {
            userBattleField = new int[sideLength, sideLength];
            opponentBattleField = new int[sideLength, sideLength];
            userBattleShipsRotation = new bool[sideLength, sideLength];
            opponentBattleShipsRotation = new bool[sideLength, sideLength];

            for (int i = 0; i < sideLength; i++)
            {
                for (int j = 0; j < sideLength; j++)
                {
                    userBattleField[i, j] = 0;
                    opponentBattleField[i, j] = 0;
                    userBattleShipsRotation[i, j] = false;
                    opponentBattleShipsRotation[i, j] = false;
                }
            }
            isWhetherTheUserCanAttackAgain = 0;
            currentShipDecks = new List<Image>();
            enteredImagesContents = new List<BitmapImage>();
            enteredImage = null;
            imageContent = null;
            isShipCanPlace = true;
            isShipsRotate = false;
        }

        private bool CheckShipPlacement(int i, int j)
        {
            int tempX, tempY;

            // двумя циклами проходимся вокруг места куда хотим поставить корабль
            for (tempY = -1; tempY < 2; tempY++)
            {
                for (tempX = -1; tempX < 2; tempX++)
                {
                    if ((i + tempY >= 0) && (j + tempX >= 0) && (i + tempY < sideLength) && (j + tempX < sideLength)) // чтобы не попасть при проверке за границы массива
                    {
                        if (userBattleField[i + tempY, j + tempX] == liveDeck) // если на клетке рядом есть корабль
                        {
                            return false;
                        }
                    }
                }
            }
            return true;    
        }

        // Показывает корабль
        private bool ViewShip(Image sender, int position, string source, bool rotation)
        {
            int k = 0, pos = 0, tempPosition = position;

            foreach (Image image in ((Grid)sender.Parent).Children)
            {
                if (pos == tempPosition)
                {
                    enteredImagesContents.Add((BitmapImage)image.Source);
                    image.Source = new BitmapImage(new Uri(source, UriKind.Relative));
                    currentShipDecks.Add(image);
                    k++;
                    tempPosition = (rotation) ? position + k * sideLength : position + k;
                    if (k == CurrentShip.CountOfDecks)
                        break;
                }
                pos++;
            }
            return (rotation) ? (source == liveDeckSourceVertical) : (source == liveDeckSourceHorizontal);
        }

        private BitmapImage CreateBitmap(string source)
        {
            return new BitmapImage(new Uri(source, UriKind.Relative)) { CreateOptions = BitmapCreateOptions.IgnoreImageCache };
        }

        private bool CheckGameContinued()
        {
            bool isGameContinue = false;
            for (int i = 0; i < sideLength; i++)
            {
                for (int j = 0; j < sideLength; j++)
                {
                    if (opponentBattleField[i, j] == liveDeck)
                    {
                        isGameContinue = true;
                        break;
                    }
                }
            }
            return isGameContinue;
        }

        public static void CreateMessageBox(string message)
        {
            MessageBox.Show(message);
        }
    }
}
