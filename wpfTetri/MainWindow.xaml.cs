using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Interop;
//MP
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Threading;
using client;



namespace wpfTetri
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool _xgame_over = false;            //True if game over
        public bool _xrun = true;                   //True if running, False if pause
        public int[] _fX = new int[4];              //X values for block on screen
        public int[] _fY = new int[4];              //Y values for block on screen
        public int[] _fXtemp = new int[4];          //TestValue for _fX
        public int[] _fYtemp = new int[4];          //TestValue for _fY
        private int[] _fXNext = new int[4];         //X Previeuw Value
        private int[] _fYNext = new int[4];         //Y Previeuw Value
        public int _BlockSize = 20;                 //BlockSize
        public int _SpeedInit = 10000000;           //Starting Speed
        public int _Speed;                         //Speed In game (going faster)
        public Settings set;                        //SettingsWindow

        public System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();   // Ticker for moving event
        public Random rand = new Random(System.DateTime.Now.Millisecond);                                                   //Random function for random blocks
        public Color BlokKleur = Color.FromArgb(100, 100, 100, 100);                                                        //Basic Block Colour

        public byte[,] field = new byte[10, 25];    //Playable field (not seen)

        private char shape;                 //What shape is it?
        private char nextshape = ' ';       //What is the next shape?
        private byte dir;                   //What is his position?

        public int _iScore = 0;     //Game Score
        public int _iLevel = 1;     //Game Level

        //Percentages for every block
        public int _procT, _procI, _procL, _procJ, _procZ, _procS, _procO;

        /// <summary>
        /// Everything for MultiPlayer Here
        /// MultiplayerStuff Starts with MP
        /// </summary>
        Client client;  //client for multiplayer
        public String MPNick = "anonymous";
        public String MPServer = "localhost";
        public int MPPort = 1000;

        public MainWindow()
        {
            InitializeComponent();

            //Settings
            _procI = 20;
            _procJ = 10;
            _procL = 10;
            _procO = 20;
            _procS = 10;
            _procZ = 10;
            _procT = 20;
            _Speed = _SpeedInit;

            //Set doorloop snelheid van het programma
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick); // Event for moving even
            dispatcherTimer.Interval = new TimeSpan(_Speed);
            dispatcherTimer.Start();

            //Multiplayer inklappen
            MultiPlayer.Visibility = Visibility.Collapsed;
            //Game Over Window inklappen
            GOW.Visibility = Visibility.Collapsed;
            //Game Over Window plaatsen in het midden
            Canvas.SetBottom(GOW, (cvsUserField.Height / 2) + (GOW.Height / 2));
            Canvas.SetLeft(GOW, (cvsUserField.Width / 2) - (GOW.Width / 2));
            //Event Restart toewijzen
            GOW.Restart += GOW_Restart;
            //multiplayer socket maken
            Socket sck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); // socket maken
            client = new Client(sck);  // nieuwe client maken met het socket
            client.Received += client_Received; // nieuwe receive methode maken

            initial();
        }

        /// <summary>
        /// When event Restart in Game Over Window, Clear all
        /// </summary>
        /// <param name="xTrue"></param>
        void GOW_Restart(bool xTrue)
        {
            if (xTrue)
            {
                //Reset score, level and speed
                _iScore = 0;
                _iLevel = 1;
                _Speed = _SpeedInit;
                dispatcherTimer.Interval = new TimeSpan(_Speed);

                cvsUserField.Children.Clear();  //Clear blocks on field
                cvsNext.Children.Clear();       //Clear blocks on next field
                //Clear Field
                for (int x = 0; x < 10; x++)
                {
                    for (int y = 0; y < 25; y++)
                    {
                        field[x, y] = 0;
                    }
                }

                initial();  //Rebuild all needed blocks
                _xgame_over = false;    //Reset game
                _xrun = true;
            }
        }

        /// <summary>
        /// initailisatie voor alle blokken
        /// </summary>
        private void initial()
        {
            //Aanmaken Preview
            Rectangle[] preview = new Rectangle[4];
            for (int blok = 0; blok < 4; blok++)
            {
                preview[blok] = new Rectangle();
                preview[blok].Name = "preview" + blok;  //Naam om terug te vinden: preview + bloknummer
                preview[blok].Height = _BlockSize;
                preview[blok].Width = _BlockSize;
                preview[blok].Fill = new SolidColorBrush(Color.FromArgb(100,100,100,100));
                preview[blok].Visibility = Visibility.Visible;
                cvsNext.Children.Add(preview[blok]);
                Canvas.SetLeft(preview[blok], (_fXNext[blok] * _BlockSize) + 10);
                Canvas.SetTop(preview[blok], (_fYNext[blok] * _BlockSize) + 10);
            }
            //pick random a new block(shape)
            RandomShape();
            //Plaats Block
            newShape();
            //Kleurtje kiezen
            GradientStopCollection gradient = new GradientStopCollection();
            gradient.Add(new GradientStop(Color.Add(BlokKleur, Color.FromArgb(100, 100, 100, 100)), 0));
            gradient.Add(new GradientStop(Color.Add(BlokKleur, Color.FromArgb(50, 50, 50, 50)), 0.5));
            gradient.Add(new GradientStop(BlokKleur, 1));
            //Speelblokken plaatsen
            Rectangle[] test = new Rectangle[4];
            for (int blok = 0; blok < 4; blok++)
            {
                test[blok] = new Rectangle();
                test[blok].Name = "test" + blok;
                test[blok].Height = _BlockSize;
                test[blok].Width = _BlockSize;
                test[blok].Fill = new RadialGradientBrush(gradient);
                test[blok].Visibility = Visibility.Visible;
                cvsUserField.Children.Add(test[blok]);
                Canvas.SetLeft(test[blok], (_fX[blok] * _BlockSize));
                Canvas.SetBottom(test[blok], (_fY[blok] * _BlockSize));
            }
            
        }

        /// <summary>
        /// Kies een nieuw block voor volgend block
        /// </summary>
        private void RandomShape()
        {
            int iRandom = rand.Next(0, 100);
            if ((iRandom >= 0) && (iRandom < _procT))
            {
                nextshape = 't';
                _fXNext[0] = 1;
                _fYNext[0] = 1;
                _fXNext[1] = _fXNext[0] - 1;
                _fYNext[1] = _fYNext[0];
                _fXNext[2] = _fXNext[0];
                _fYNext[2] = _fYNext[0] - 1;
                _fXNext[3] = _fXNext[0] + 1;
                _fYNext[3] = _fYNext[0];
            }
            else if ((iRandom >= _procT) && (iRandom < (_procT+_procI)))
            {
                nextshape = 'i';
                _fXNext[0] = 0;
                _fYNext[0] = 1;
                _fXNext[1] = _fXNext[0] + 1;
                _fYNext[1] = _fYNext[0];
                _fXNext[2] = _fXNext[0] + 2;
                _fYNext[2] = _fYNext[0];
                _fXNext[3] = _fXNext[0] + 3;
                _fYNext[3] = _fYNext[0];
            }
            else if ((iRandom >= (_procT + _procI)) && (iRandom < (_procT + _procI+_procL)))
            {
                nextshape = 'l';
                _fXNext[0] = 0;
                _fYNext[0] = 1;
                _fXNext[1] = _fXNext[0];
                _fYNext[1] = _fYNext[0] - 1;
                _fXNext[2] = _fXNext[0] + 1;
                _fYNext[2] = _fYNext[0] - 1;
                _fXNext[3] = _fXNext[0] + 2;
                _fYNext[3] = _fYNext[0] - 1;
            }
            else if ((iRandom >= (_procT + _procI + _procL)) && (iRandom < (_procT + _procI + _procL+_procJ)))
            {
                nextshape = 'j';
                _fXNext[0] = 2;
                _fYNext[0] = 1;
                _fXNext[1] = _fXNext[0];
                _fYNext[1] = _fYNext[0]-1;
                _fXNext[2] = _fXNext[0]-1;
                _fYNext[2] = _fYNext[0] - 1;
                _fXNext[3] = _fXNext[0] - 2;
                _fYNext[3] = _fYNext[0] - 1;
            }
            else if ((iRandom >= (_procT + _procI + _procL + _procJ)) && (iRandom < (_procT + _procI + _procL + _procJ + _procZ)))
            {
                nextshape = 'z';
                _fXNext[0] = 0;
                _fYNext[0] = 0;
                _fXNext[1] = _fXNext[0] + 1;
                _fYNext[1] = _fYNext[0];
                _fXNext[2] = _fXNext[0]+1;
                _fYNext[2] = _fYNext[0] + 1;
                _fXNext[3] = _fXNext[0] + 2;
                _fYNext[3] = _fYNext[0] + 1;
            }
            else if ((iRandom >= (_procT + _procI + _procL + _procJ + _procZ)) && (iRandom < (_procT + _procI + _procL + _procJ + _procZ+_procS)))
            {
                nextshape = 's';
                _fXNext[0] = 0;
                _fYNext[0] = 1;
                _fXNext[1] = _fXNext[0] + 1;
                _fYNext[1] = _fYNext[0];
                _fXNext[2] = _fXNext[0]+1;
                _fYNext[2] = _fYNext[0] - 1;
                _fXNext[3] = _fXNext[0] +2;
                _fYNext[3] = _fYNext[0] - 1;
            }
            else
            {
                nextshape = 'o';
                _fXNext[0] = 1;
                _fYNext[0] = 0;
                _fXNext[1] = _fXNext[0] + 1;
                _fYNext[1] = _fYNext[0];
                _fXNext[2] = _fXNext[0];
                _fYNext[2] = _fYNext[0] + 1;
                _fXNext[3] = _fXNext[0] + 1;
                _fYNext[3] = _fYNext[0] + 1;
            }

            foreach (Rectangle Block in cvsNext.Children)
            {
                if (Block.Name == "preview0")
                {
                    if((nextshape=='i')||(nextshape=='o'))
                        Canvas.SetLeft(Block,(_fXNext[0]*_BlockSize));
                    else
                        Canvas.SetLeft(Block, (_fXNext[0]*_BlockSize)+(_BlockSize/2));
                    if (nextshape == 'i')
                        Canvas.SetTop(Block, (_fYNext[0] * _BlockSize) + (_BlockSize / 2));
                    else
                        Canvas.SetTop(Block, (_fYNext[0] * _BlockSize) + (_BlockSize));
                }
                if (Block.Name == "preview1")
                {
                    if ((nextshape == 'i') || (nextshape == 'o'))
                        Canvas.SetLeft(Block, (_fXNext[1] * _BlockSize));
                    else
                        Canvas.SetLeft(Block, (_fXNext[1] * _BlockSize) + (_BlockSize/2));
                    if (nextshape == 'i')
                        Canvas.SetTop(Block, (_fYNext[1] * _BlockSize) + (_BlockSize / 2));
                    else
                        Canvas.SetTop(Block, (_fYNext[1] * _BlockSize) + (_BlockSize));
                }
                if (Block.Name == "preview2")
                {
                    if ((nextshape == 'i') || (nextshape == 'o'))
                        Canvas.SetLeft(Block, (_fXNext[2] * _BlockSize));
                    else
                        Canvas.SetLeft(Block, (_fXNext[2] * _BlockSize) + (_BlockSize/2));
                    if (nextshape == 'i')
                        Canvas.SetTop(Block, (_fYNext[2] * _BlockSize) + (_BlockSize / 2));
                    else
                        Canvas.SetTop(Block, (_fYNext[2] * _BlockSize) + (_BlockSize));
                }
                if (Block.Name == "preview3")
                {
                    if ((nextshape == 'i') || (nextshape == 'o'))
                        Canvas.SetLeft(Block, (_fXNext[3] * _BlockSize));
                    else
                        Canvas.SetLeft(Block, (_fXNext[3] * _BlockSize) + (_BlockSize/2));
                    if (nextshape == 'i')
                        Canvas.SetTop(Block, (_fYNext[3] * _BlockSize) + (_BlockSize / 2));
                    else
                        Canvas.SetTop(Block, (_fYNext[3] * _BlockSize) + (_BlockSize));
                }
            }
            
        }

        //Huidig block vormen
        private void newShape()
        {
            //int iRandom = rand.Next(0, 100);
            shape = nextshape;
            dir = (byte)rand.Next(0, 3);
            if (shape == 't')
            {
                _fX[0] = 5;
                _fY[0] = 20;
                BlokKleur = Color.FromArgb(255, 200, 0, 200);
            }
            else if (shape == 'i')
            {
                dir = 1;
                _fX[0] = 5;
                _fY[0] = 20;
                _fX[1] = _fX[0];
                _fY[1] = _fY[0] + 1;
                _fX[2] = _fX[0];
                _fY[2] = _fY[0] + 2;
                _fX[3] = _fX[0];
                _fY[3] = _fY[0] + 3;
                BlokKleur = Color.FromArgb(255, 0, 50, 200);
            }
            else if (shape == 'l')
            {
                
                _fX[0] = 4;
                _fY[0] = 20;
                BlokKleur = Color.FromArgb(255, 200, 100, 0);
            }
            else if (shape == 'j')
            {
                
                _fX[0] = 5;
                _fY[0] = 20;
                BlokKleur = Color.FromArgb(255, 0, 0, 150);
            }
            else if (shape == 'z')
            {
                
                _fX[0] = 4;
                _fY[0] = 20;
                BlokKleur = Color.FromArgb(255, 200, 0, 0);
            }
            else if (shape == 's')
            {
                
                _fX[0] = 3;
                _fY[0] = 20;
                BlokKleur = Color.FromArgb(255, 0, 200, 0);
            }
            else if (shape == 'o')
            {
                
                _fX[0] = 3;
                _fY[0] = 20;
                BlokKleur = Color.FromArgb(255, 200, 200, 0);
            }

            Turn();

            foreach (Rectangle test in cvsUserField.Children)
            {
                if (test.Name.Contains("test"))
                {
                    GradientStopCollection gradient = new GradientStopCollection();
                    gradient.Add(new GradientStop(Color.Add(BlokKleur, Color.FromArgb(100, 100, 100, 100)), 0));
                    gradient.Add(new GradientStop(Color.Add(BlokKleur, Color.FromArgb(50, 50, 50, 50)), 0.5));
                    gradient.Add(new GradientStop(BlokKleur, 1));
                    test.Fill = new RadialGradientBrush(gradient);
                }
            }
            RandomShape();
        }

        /// <summary>
        /// Per tick word het volledige programma doorlopen. Dit regelt de snelheid van het spel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            check_game();
            if ((_xrun == true) && (_xgame_over == false))
                Lower();
        }

        /// <summary>
        /// Detecteer ingedrukte toets + setten van bools
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Toets</param>
        private void KeyDownEventHanlder(object sender, KeyEventArgs e)
        {
            Key toets = e.Key;
            if ((_xrun == true) && (_xgame_over == false))
            {
                switch (toets)
                {
                    case Key.Up: { Turn(); break; }
                    case Key.Down: { Lower(); break; }
                    case Key.Right: { MoveLR(true); break; }
                    case Key.Left: { MoveLR(false); break; }
                    case Key.Z: { Turn(); break; }
                    case Key.S: { Lower(); break; }
                    case Key.D: { MoveLR(true); break; }
                    case Key.Q: { MoveLR(false); break; }
                    case Key.Space: { while (!Lower()); break; }
                }
            }
        }

        /// <summary>
        /// Beweeg huidig blok links of rechts
        /// </summary>
        /// <param name="LeftRight">True=Rechts, False=Links</param>
        private void MoveLR(bool LeftRight)
        {
            if (LeftRight == false)
            {
                for (int t = 0; t < 4; t++)
                {
                    _fXtemp[t] = _fX[t] - 1;
                    _fYtemp[t] = _fY[t];
                }
            }
            if (LeftRight == true)
            {
                for (int t = 0; t < 4; t++)
                {
                    _fXtemp[t] = _fX[t] + 1;
                    _fYtemp[t] = _fY[t];
                }
            }
            //Wanneer move geldig, pas plaatsen
            if (TestMove())
            {
                for (int t = 0; t < 4; t++)
                {
                    _fX[t] = _fXtemp[t];
                    _fY[t] = _fYtemp[t];
                }
            }
            Move();
        }

        /// <summary>
        /// Huidig blok zetten op huidige plaats
        /// </summary>
        private void Move()
        {
            foreach (Rectangle Block in cvsUserField.Children)
            {
                if (Block.Name == "test0")
                {
                    Canvas.SetBottom(Block, (_fY[0] * _BlockSize));
                    Canvas.SetLeft(Block, (_fX[0] * _BlockSize));
                }
                if (Block.Name == "test1")
                {
                    Canvas.SetBottom(Block, (_fY[1] * _BlockSize));
                    Canvas.SetLeft(Block, (_fX[1] * _BlockSize));
                }
                if (Block.Name == "test2")
                {
                    Canvas.SetBottom(Block, (_fY[2] * _BlockSize));
                    Canvas.SetLeft(Block, (_fX[2] * _BlockSize));
                }
                if (Block.Name == "test3")
                {
                    Canvas.SetBottom(Block, (_fY[3] * _BlockSize));
                    Canvas.SetLeft(Block, (_fX[3] * _BlockSize));
                }
            }
        }

        /// <summary>
        /// zak huidig blok 1 lijn
        /// </summary>
        /// <returns>Returns True wanneer de blok vast zit</returns>
        private bool Lower()
        {
            bool fix = false;
            for (int t = 0; t < 4; t++)
            {
                _fXtemp[t] = _fX[t];
                _fYtemp[t] = _fY[t] - 1;
            }
            //wanneer geldg beweeg blok
            if (TestMove())
            {
                for (int t = 0; t < 4; t++)
                {
                    _fX[t] = _fXtemp[t];
                    _fY[t] = _fYtemp[t];
                }
            }
            //anders: zet blok vast
            else
            {
                fixate();
                fix = true;
            }
            Move();
            return fix;
        }

        /// <summary>
        /// Controleer huidg blok of volgende zet geldig is
        /// </summary>
        /// <returns>Returns True wanner geldig</returns>
        private bool TestMove()
        {
            bool testing = true;
            //true if good
            for (int t = 0; t < 4; t++)
            {
                if ((_fXtemp[t] >= 0) && (_fXtemp[t] < 10))
                {
                    if ((_fYtemp[t] >= 0) && (_fYtemp[t] < 25))
                    {
                        if (field[_fXtemp[t], _fYtemp[t]] == 1)
                            testing = false;
                    }
                    else
                        testing = false;
                }
                else
                    testing = false;
            }
            return testing;
        }

        /// <summary>
        /// Alle draaibewegingen van alle blokken zit hierin
        /// </summary>
        private void Turn()
        {
            if (shape == 't')
            {
                switch (dir)
                {
                    case 0:
                        {
                            _fXtemp[0] = _fX[0];
                            _fYtemp[0] = _fY[0];
                            _fXtemp[1] = _fXtemp[0];
                            _fYtemp[1] = _fYtemp[0] + 1;
                            _fXtemp[2] = _fXtemp[0] + 1;
                            _fYtemp[2] = _fYtemp[0];
                            _fXtemp[3] = _fXtemp[0];
                            _fYtemp[3] = _fYtemp[0] - 1;
                            break;
                        }
                    case 1:
                        {
                            if (_fX[0] > 0)
                                _fXtemp[0] = _fX[0];
                            else
                                _fXtemp[0] = _fX[0] + 1;
                            _fYtemp[0] = _fY[0];
                            _fXtemp[1] = _fXtemp[0] + 1;
                            _fYtemp[1] = _fYtemp[0];
                            _fXtemp[2] = _fXtemp[0];
                            _fYtemp[2] = _fYtemp[0] - 1;
                            _fXtemp[3] = _fXtemp[0] - 1;
                            _fYtemp[3] = _fYtemp[0];
                            break;
                        }
                    case 2:
                        {
                            _fXtemp[0] = _fX[0];
                            _fYtemp[0] = _fY[0];
                            _fXtemp[1] = _fXtemp[0];
                            _fYtemp[1] = _fYtemp[0] - 1;
                            _fXtemp[2] = _fXtemp[0] - 1;
                            _fYtemp[2] = _fYtemp[0];
                            _fXtemp[3] = _fXtemp[0];
                            _fYtemp[3] = _fYtemp[0] + 1;
                            break;
                        }
                    case 3:
                        {
                            if (_fX[0] < 9)
                                _fXtemp[0] = _fX[0];
                            else
                                _fXtemp[0] = _fX[0] - 1;
                            _fYtemp[0] = _fY[0];
                            _fXtemp[1] = _fXtemp[0] - 1;
                            _fYtemp[1] = _fYtemp[0];
                            _fXtemp[2] = _fXtemp[0];
                            _fYtemp[2] = _fYtemp[0] + 1;
                            _fXtemp[3] = _fXtemp[0] + 1;
                            _fYtemp[3] = _fYtemp[0];
                            break;
                        }
                }
            }
            if (shape == 'i')
            {
                switch (dir)
                {
                    case 0:
                        {
                            _fXtemp[0] = _fX[0] + 1;
                            _fYtemp[0] = _fY[0] + 2;
                            _fXtemp[1] = _fXtemp[0];
                            _fYtemp[1] = _fYtemp[0] - 1;
                            _fXtemp[2] = _fXtemp[0];
                            _fYtemp[2] = _fYtemp[0] - 2;
                            _fXtemp[3] = _fXtemp[0];
                            _fYtemp[3] = _fYtemp[0] - 3;
                            break;
                        }
                    case 1:
                        {
                            _fXtemp[0] = _fX[0] + 2;
                            if (_fXtemp[0] < 3)
                                _fXtemp[0] = 3;
                            if (_fXtemp[0] > 9)
                                _fXtemp[0] = 9;
                            _fYtemp[0] = _fY[0] - 1;
                            _fXtemp[1] = _fXtemp[0] - 1;
                            _fYtemp[1] = _fYtemp[0];
                            _fXtemp[2] = _fXtemp[0] - 2;
                            _fYtemp[2] = _fYtemp[0];
                            _fXtemp[3] = _fXtemp[0] - 3;
                            _fYtemp[3] = _fYtemp[0];
                            break;
                        }
                    case 2:
                        {
                            _fXtemp[0] = _fX[0] - 1;
                            _fYtemp[0] = _fY[0] - 2;
                            _fXtemp[1] = _fXtemp[0];
                            _fYtemp[1] = _fYtemp[0] + 1;
                            _fXtemp[2] = _fXtemp[0];
                            _fYtemp[2] = _fYtemp[0] + 2;
                            _fXtemp[3] = _fXtemp[0];
                            _fYtemp[3] = _fYtemp[0] + 3;
                            break;
                        }
                    case 3:
                        {
                            _fXtemp[0] = _fX[0] - 2;
                            if (_fXtemp[0] < 0)
                                _fXtemp[0] = 0;
                            if (_fXtemp[0] > 6)
                                _fXtemp[0] = 6;
                            _fYtemp[0] = _fY[0] + 1;
                            _fXtemp[1] = _fXtemp[0] + 1;
                            _fYtemp[1] = _fYtemp[0];
                            _fXtemp[2] = _fXtemp[0] + 2;
                            _fYtemp[2] = _fYtemp[0];
                            _fXtemp[3] = _fXtemp[0] + 3;
                            _fYtemp[3] = _fYtemp[0];
                            break;
                        }
                }
            }
            if (shape == 'l')
            {
                switch (dir)
                {
                    case 0:
                        {
                            _fXtemp[0] = _fX[0] + 1;
                            _fYtemp[0] = _fY[0] + 1;
                            _fXtemp[1] = _fXtemp[0] + 1;
                            _fYtemp[1] = _fYtemp[0];
                            _fXtemp[2] = _fXtemp[0] + 1;
                            _fYtemp[2] = _fYtemp[0] - 1;
                            _fXtemp[3] = _fXtemp[0] + 1;
                            _fYtemp[3] = _fYtemp[0] - 2;
                            break;
                        }
                    case 1:
                        {
                            if (_fX[0] > 0)
                                _fXtemp[0] = _fX[0] + 1;
                            else
                                _fXtemp[0] = _fX[0] + 2;
                            _fYtemp[0] = _fY[0] - 1;
                            _fXtemp[1] = _fXtemp[0];
                            _fYtemp[1] = _fYtemp[0] - 1;
                            _fXtemp[2] = _fXtemp[0] - 1;
                            _fYtemp[2] = _fYtemp[0] - 1;
                            _fXtemp[3] = _fXtemp[0] - 2;
                            _fYtemp[3] = _fYtemp[0] - 1;
                            break;
                        }
                    case 2:
                        {
                            _fXtemp[0] = _fX[0] - 1;
                            _fYtemp[0] = _fY[0] - 1;
                            _fXtemp[1] = _fXtemp[0] - 1;
                            _fYtemp[1] = _fYtemp[0];
                            _fXtemp[2] = _fXtemp[0] - 1;
                            _fYtemp[2] = _fYtemp[0] + 1;
                            _fXtemp[3] = _fXtemp[0] - 1;
                            _fYtemp[3] = _fYtemp[0] + 2;
                            break;
                        }
                    case 3:
                        {
                            if (_fX[0] < 9)
                                _fXtemp[0] = _fX[0] - 1;
                            else
                                _fXtemp[0] = _fX[0] - 2;
                            _fYtemp[0] = _fY[0] + 1;
                            _fXtemp[1] = _fXtemp[0];
                            _fYtemp[1] = _fYtemp[0] + 1;
                            _fXtemp[2] = _fXtemp[0] + 1;
                            _fYtemp[2] = _fYtemp[0] + 1;
                            _fXtemp[3] = _fXtemp[0] + 2;
                            _fYtemp[3] = _fYtemp[0] + 1;
                            break;
                        }
                }
            }
            if (shape == 'j')
            {
                switch (dir)
                {
                    case 0:
                        {
                            _fXtemp[0] = _fX[0] - 1;
                            _fYtemp[0] = _fY[0] - 1;
                            _fXtemp[1] = _fXtemp[0] + 1;
                            _fYtemp[1] = _fYtemp[0];
                            _fXtemp[2] = _fXtemp[0] + 1;
                            _fYtemp[2] = _fYtemp[0] + 1;
                            _fXtemp[3] = _fXtemp[0] + 1;
                            _fYtemp[3] = _fYtemp[0] + 2;
                            break;
                        }
                    case 1:
                        {
                            if (_fX[0]>0)
                                _fXtemp[0] = _fX[0] - 1;
                            _fYtemp[0] = _fY[0] + 1;
                            _fXtemp[1] = _fXtemp[0];
                            _fYtemp[1] = _fYtemp[0] - 1;
                            _fXtemp[2] = _fXtemp[0] + 1;
                            _fYtemp[2] = _fYtemp[0] - 1;
                            _fXtemp[3] = _fXtemp[0] + 2;
                            _fYtemp[3] = _fYtemp[0] - 1;
                            break;
                        }
                    case 2:
                        {
                            _fXtemp[0] = _fX[0] + 1;
                            _fYtemp[0] = _fY[0] + 1;
                            _fXtemp[1] = _fXtemp[0] - 1;
                            _fYtemp[1] = _fYtemp[0];
                            _fXtemp[2] = _fXtemp[0] - 1;
                            _fYtemp[2] = _fYtemp[0] - 1;
                            _fXtemp[3] = _fXtemp[0] - 1;
                            _fYtemp[3] = _fYtemp[0] - 2;
                            break;
                        }
                    case 3:
                        {
                            if (_fX[0] <9)
                                _fXtemp[0] = _fX[0] + 1;
                            _fYtemp[0] = _fY[0] - 1;
                            _fXtemp[1] = _fXtemp[0];
                            _fYtemp[1] = _fYtemp[0] + 1;
                            _fXtemp[2] = _fXtemp[0] - 1;
                            _fYtemp[2] = _fYtemp[0] + 1;
                            _fXtemp[3] = _fXtemp[0] - 2;
                            _fYtemp[3] = _fYtemp[0] + 1;

                            break;
                        }
                }
            }
            if (shape == 'z')
            {
                switch (dir)
                {
                    case 0:
                    case 2:
                        {
                            _fXtemp[0] = _fX[0] + 1;
                            _fYtemp[0] = _fY[0] + 1;
                            _fXtemp[1] = _fXtemp[0];
                            _fYtemp[1] = _fYtemp[0] - 1;
                            _fXtemp[2] = _fXtemp[0] - 1;
                            _fYtemp[2] = _fYtemp[0] - 1;
                            _fXtemp[3] = _fXtemp[0] - 1;
                            _fYtemp[3] = _fYtemp[0] - 2;
                            break;
                        }
                    case 1:
                    case 3:
                        {
                            if (_fX[0]==9)
                                _fXtemp[0] = _fX[0] - 2;
                            else
                                _fXtemp[0] = _fX[0] - 1;
                            _fYtemp[0] = _fY[0] - 1;
                            _fXtemp[1] = _fXtemp[0] + 1;
                            _fYtemp[1] = _fYtemp[0];
                            _fXtemp[2] = _fXtemp[0] + 1;
                            _fYtemp[2] = _fYtemp[0] - 1;
                            _fXtemp[3] = _fXtemp[0] + 2;
                            _fYtemp[3] = _fYtemp[0] - 1;
                            break;
                        }
                }
            }
            if (shape == 's')
            {
                switch (dir)
                {
                    case 0:
                    case 2:
                        {
                            _fXtemp[0] = _fX[0] + 1;
                            _fYtemp[0] = _fY[0] + 1;
                            _fXtemp[1] = _fXtemp[0];
                            _fYtemp[1] = _fYtemp[0] - 1;
                            _fXtemp[2] = _fXtemp[0] + 1;
                            _fYtemp[2] = _fYtemp[0] - 1;
                            _fXtemp[3] = _fXtemp[0] + 1;
                            _fYtemp[3] = _fYtemp[0] - 2;
                            break;
                        }
                    case 1:
                    case 3:
                        {
                            if (_fX[0] == 0)
                                _fXtemp[0] = 0;
                            else
                                _fXtemp[0] = _fX[0] - 1;
                            _fYtemp[0] = _fY[0] - 1;
                            _fXtemp[1] = _fXtemp[0] + 1;
                            _fYtemp[1] = _fYtemp[0];
                            _fXtemp[2] = _fXtemp[0] + 1;
                            _fYtemp[2] = _fYtemp[0] + 1;
                            _fXtemp[3] = _fXtemp[0] + 2;
                            _fYtemp[3] = _fYtemp[0] + 1;
                            break;
                        }
                }
            }
            if (shape == 'o')
            {
                _fXtemp[0] = _fX[0];
                _fYtemp[0] = _fY[0];
                _fXtemp[1] = _fXtemp[0] + 1;
                _fYtemp[1] = _fYtemp[0];
                _fXtemp[2] = _fXtemp[0];
                _fYtemp[2] = _fYtemp[0] - 1;
                _fXtemp[3] = _fXtemp[0] + 1;
                _fYtemp[3] = _fYtemp[0] - 1;
            }

            bool test = TestMove();
            if (test)
            {
                for (int x = 0; x < 4; x++)
                {
                    _fX[x] = _fXtemp[x];
                    _fY[x] = _fYtemp[x];
                }
                if (dir < 3)
                    dir++;
                else
                    dir = 0;
            }
            Move();
        }

        /// <summary>
        /// FunctionButtons
        /// </summary>
        /// <param name="sender">MinMaxClose</param>
        /// <param name="e"></param>
        private void App_Click(object sender, RoutedEventArgs e)
        {
            Ellipse knop = (Ellipse)sender;
            if (knop.Name == "ellClose")
            {
                Application currentApp = Application.Current;
                currentApp.Shutdown();
            }
            if (knop.Name == "ellMin")
            {
                WindowState = WindowState.Minimized;
            }
            if (knop.Name == "ellOptions")
            {
                Pause_Game(true);
                set = new Settings(this);
                set.Closed += set_Closed;
                set.Owner = this; 
                set.Show();
            }
            if (knop.Name == "ellShift")
            {
                if (MultiPlayer.Visibility == Visibility.Collapsed)
                {
                    MultiPlayer.Visibility = Visibility.Visible;
                    ellShift.Fill = new ImageBrush(Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.Knop_Pijl_Links_4.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()));
                    btnConnect_Click(null,null);
                }
                else
                {
                    MultiPlayer.Visibility = Visibility.Collapsed;
                    ellShift.Fill = new ImageBrush(Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.Knop_Pijl_Rechts_4.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()));
                    
                }
            }
            if (knop.Name == "ellPause")
            {
                Pause_Game(_xrun);
            }
        }

        /// <summary>
        /// FunctionButtons
        /// </summary>
        /// <param name="sender">MinMaxClose</param>
        /// <param name="e"></param>
        private void App_MouseEnter(object sender, MouseEventArgs e)
        {
            Ellipse knop = (Ellipse)sender;
            if (knop.Name == "ellClose")
                ellCloseBack.Fill = new SolidColorBrush(Color.FromArgb(100, 200, 0, 0));
            if (knop.Name == "ellMin")
                ellMinBack.Fill = new SolidColorBrush(Color.FromArgb(100, 200, 0, 0));
            if(knop.Name=="ellOptions")
                ellOptionsBack.Fill = new SolidColorBrush(Color.FromArgb(100, 200, 0, 0));
            if (knop.Name == "ellShift")
                ellShiftBack.Fill = new SolidColorBrush(Color.FromArgb(100, 200, 0, 0));
            if(knop.Name=="ellPause")
                ellPauseBack.Fill = new SolidColorBrush(Color.FromArgb(100, 200, 0, 0));
        }

        /// <summary>
        /// change back Background behind FunctionButton
        /// </summary>
        /// <param name="sender">MinMaxClose</param>
        /// <param name="e"></param>
        private void App_MouseLeave(object sender, MouseEventArgs e)
        {
            Ellipse knop = (Ellipse)sender;
            if (knop.Name == "ellClose")
                ellCloseBack.Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            if (knop.Name == "ellMin")
                ellMinBack.Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            if (knop.Name == "ellOptions")
                ellOptionsBack.Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            if (knop.Name == "ellShift")
                ellShiftBack.Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            if (knop.Name == "ellPause")
                ellPauseBack.Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        }

        /// <summary>
        /// Makes the window move arround
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TitleBar(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        /// <summary>
        /// fixate current block
        /// </summary>
        private void fixate()
        {
            for (int t = 0; t < 4; t++)
            {
                //Make his place in code 1
                field[_fX[t], _fY[t]] = 1;
                //Make his place on field visible
                Rectangle fix = new Rectangle();
                fix.Name = "fix"+_fX[t]+_fY[t];
                fix.Height = _BlockSize;
                fix.Width = _BlockSize;
                GradientStopCollection gradient = new GradientStopCollection();
                gradient.Add(new GradientStop(Color.Add(BlokKleur, Color.FromArgb(100, 100, 100, 100)), 0));
                gradient.Add(new GradientStop(Color.Add(BlokKleur, Color.FromArgb(50, 50, 50, 50)), 0.5));
                gradient.Add(new GradientStop(BlokKleur, 1));
                fix.Fill = new RadialGradientBrush(gradient);
                fix.Visibility = Visibility.Visible;
                cvsUserField.Children.Add(fix);
                Canvas.SetBottom(fix, (_fY[t] * _BlockSize));
                Canvas.SetLeft(fix, (_fX[t] * _BlockSize));
                //if higher than 20 blocks: Game Over
                if (_fY[t] > 20)
                {
                    GameOver(true);
                }
            }
            //Check fir full lines
            checkRow();
            //Pick New blocks
            newShape();
        }

        /// <summary>
        /// Check for full lines
        /// </summary>
        private void checkRow()
        {
            bool full = true;
            int tempScore = 0;
            for (int tY = 24; tY >= 0; tY--)
            {
                full = true;
                for (int tX = 0; tX < 10; tX++)
                {
                    if (field[tX, tY] == 0)
                        full = false;
                }
                if (full)
                {
                    if (tempScore == 0)
                        tempScore = 100;
                    else
                        tempScore *= 2;
                    for (int delX = 0; delX < 10; delX++)
                    {
                        Rectangle rm = new Rectangle();
                        foreach (Rectangle Block in cvsUserField.Children)
                        {
                            if (Block.Name == ("fix" + delX + tY))
                            {
                                rm = Block;
                            }
                        }
                        cvsUserField.Children.Remove(rm);
                    }
                    for (int delY = tY; delY < 23; delY++)
                    {
                        for (int delX = 0; delX < 10; delX++)
                        {
                            field[delX, delY] = field[delX, delY + 1];
                            Rectangle mv = new Rectangle();
                            foreach (Rectangle Block in cvsUserField.Children)
                            {
                                if (Block.Name == ("fix" + delX + (delY + 1)))
                                {
                                    mv = Block;
                                }
                            }
                            mv.Name = "fix" + delX + delY;
                            mv.Tag = "fix" + delX + delY;
                            Canvas.SetBottom(mv, ((delY) * _BlockSize));
                            Canvas.SetLeft(mv, (delX * _BlockSize));
                        }
                    }
                }
            }
            Score(tempScore);
        }

        /// <summary>
        /// Click event on Options
        /// Opens the option Window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Option_Click(object sender, RoutedEventArgs e)
        {
            Settings set = new Settings(this);
            set.Show();
        }

        /// <summary>
        /// If Options is closed, Continue Game
        /// </summary>
        /// <param name="xTrue"></param>
        void set_Closed(bool xTrue)
        {
            Pause_Game(false);
        }

        /// <summary>
        /// Check for Blocks Higher than 20
        /// </summary>
        private void check_game()
        {
            for (int Y = 20; Y < 25; Y++)
            {
                for (int X = 0; X < 10; X++)
                {
                    if (field[X, Y] == 1)
                    {
                        GameOver(true);
                    }
                }
            }
        }

        /// <summary>
        /// Make Game Over Window visible
        /// </summary>
        /// <param name="STOP">True=Game Over</param>
        public void GameOver(bool STOP)
        {
            if (!_xgame_over && STOP)
            {
                GOW.Visibility = Visibility.Visible;
            }
            _xgame_over = STOP;
        }

        /// <summary>
        /// Pauses game
        /// Changes the pausebutton back and forth
        /// </summary>
        /// <param name="Pause">True=Pause, False=Run</param>
        public void Pause_Game(bool Pause)
        {
            if (Pause)
            {
                ellPause.Fill = new ImageBrush(Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.Knop_Pijl_Rechts_2.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()));
                _xrun = false;
            }
            else
            {
                ellPause.Fill = new ImageBrush(Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.Knop_Pauze.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()));
                _xrun = true;
            }
        }

        /// <summary>
        /// manage scores and speed
        /// </summary>
        /// <param name="AddScore"></param>
        public void Score(int AddScore)
        {
            _iScore += AddScore;
            _iLevel = (_iScore / 1000) + 1;
            _Speed = _SpeedInit -(_iLevel * 1000000);
            lblScore.Content = "Score: " + _iScore;
            lblLevel.Content = "Level: " + _iLevel;
            dispatcherTimer.Interval = new TimeSpan(_Speed);
        }

        //MULTPLAYER STUFF HERE!! //////////////////////////////////////////////////////////////////////////////////////////

        private void btnConnect_Click(object sender, RoutedEventArgs e) //connecteren met de server
        {
            if (MPNick != string.Empty) //enkel connecteren met naam
            {
                try
                {
                    //client.sck.Connect("127.0.0.1", 1000);  //verbinden
                    client.sck.Connect(MPServer, MPPort);
                    int s = client.sck.Send(Encoding.Default.GetBytes("Name " + MPNick));  // naam doorgeven
                    txtInfoBox.Content = "Connected";
                    client.begin_receive();  //beginnen met ontvangen
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                txtInfoBox.Content = "Vul je naam in";
            }
            
        }

        void client_Received(Client sender, byte[] data) // als er iets ontvangen is komt dit hier binnen
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => message_received(Encoding.Default.GetString(data), sender)));
        }
        void message_received(string tekst, Client sender)
        {
            txtchatBox.Text =  tekst + "\r\n" + txtchatBox.Text;
        }

        private void btnVerzenden_Click(object sender, RoutedEventArgs e)// methode om text te verzenden
        {
            int s = client.sck.Send(Encoding.Default.GetBytes("Broadcast " + txtVerzenden.Text));

            if (s > 0)
            {
                txtInfoBox.Content = "bericht verzonden";
                txtchatBox.Text = "Me: " + txtVerzenden.Text + "\r\n" + txtchatBox.Text;
                txtVerzenden.Text = String.Empty;
            }
        }       
    }
}
