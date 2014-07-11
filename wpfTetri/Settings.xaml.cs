using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace wpfTetri
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Settings : Window
    {
        MainWindow M = new MainWindow();

        bool SafeSave = true;

        public event CloseEvent Closed;
        public delegate void CloseEvent(bool xTrue);

        public Settings()
        {
            InitializeComponent();
        }

        public Settings(MainWindow Main)
        {
            M = Main;
            InitializeComponent();
            Menu.Visibility = Visibility.Hidden;
            this.Activate();
            ChanceI.Text = M._procI.ToString();
            ChanceT.Text = M._procT.ToString();
            ChanceS.Text = M._procS.ToString();
            ChanceZ.Text = M._procZ.ToString();
            ChanceJ.Text = M._procJ.ToString();
            ChanceL.Text = M._procL.ToString();
            ChanceO.Text = M._procO.ToString();
            TotalChance.Text = (M._procI + M._procT + M._procS + M._procZ + M._procJ + M._procL + M._procO).ToString();
            sldSpeed.Value = M._SpeedInit;
            sldSpeed_ValueChanged(null, null);

            MPtxtNick.Text = M.MPNick;
            MPtxtServer.Text = M.MPServer;
            MPtxtPort.Text = M.MPPort.ToString();
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            Button knop = (Button)sender;
            if (knop.Name == "btnPage0")
            {
                Page0.Width = new GridLength(1,GridUnitType.Star);
                Page1.Width = new GridLength(0);

            }
            if (knop.Name == "btnPage1")
            {
                Page1.Width = new GridLength(1, GridUnitType.Star);
                Page0.Width = new GridLength(0);
            }
            if (knop.Name == "btnSPage0")
            {
                SPage0.Width = new GridLength(1, GridUnitType.Star);
                SPage1.Width = new GridLength(0);

            }
            if (knop.Name == "btnSPage1")
            {
                SPage1.Width = new GridLength(1, GridUnitType.Star);
                SPage0.Width = new GridLength(0);
            }
            if (knop.Name == "btnDiscard")
            {
                this.Close();
                Closed(true);
            }
            if (knop.Name == "btnClient")
            {
                Local.Visibility = Visibility.Visible;
                Server.Visibility = Visibility.Collapsed;
                Menu.Visibility = Visibility.Hidden;
            }
            if (knop.Name == "btnServer")
            {
                Local.Visibility = Visibility.Collapsed;
                Server.Visibility = Visibility.Visible;
                Menu.Visibility = Visibility.Hidden;
            }
            if (knop.Name == "btnSave")
            {
                if (SafeSave)
                {
                    Saving();
                    this.Close();
                    Closed(true);
                }
                else
                { lblSave.Content = "Wrong Values"; }
            }
        }

        /// <summary>
        /// Scrijf alles naar het mainwindow
        /// </summary>
        private void Saving()
        {
            M._procI = Convert.ToInt32(ChanceI.Text.ToString());
            M._procT = Convert.ToInt32(ChanceT.Text.ToString());
            M._procS = Convert.ToInt32(ChanceS.Text.ToString());
            M._procZ = Convert.ToInt32(ChanceZ.Text.ToString());
            M._procJ = Convert.ToInt32(ChanceJ.Text.ToString());
            M._procL = Convert.ToInt32(ChanceL.Text.ToString());
            M._procO = Convert.ToInt32(ChanceO.Text.ToString());
            //M._Speed = Convert.ToInt32(sldSpeed.Value);
            M._SpeedInit = Convert.ToInt32(sldSpeed.Value);
            M.dispatcherTimer.Interval = new TimeSpan(Convert.ToInt32(sldSpeed.Value));
            M.MPNick = MPtxtNick.Text.ToString();
            M.MPServer = MPtxtServer.Text.ToString();
            M.MPPort = Convert.ToInt32(MPtxtPort.Text.ToString());
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
                //Application currentApp = Application.Current;
                //currentApp.Shutdown();
                this.Close();
                Closed(true);
            }
            if (knop.Name == "ellMin")
            {
                WindowState = WindowState.Minimized;
            }
            if (knop.Name == "ellShift")
            {
                if (Menu.Visibility == Visibility.Visible)
                    Menu.Visibility = Visibility.Hidden;
                else
                    Menu.Visibility = Visibility.Visible;
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
            if (knop.Name == "ellShift")
                ellShiftBack.Fill = new SolidColorBrush(Color.FromArgb(100, 200, 0, 0));
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
            if (knop.Name == "ellShift")
                ellShiftBack.Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
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
        /// KeyUpEvent of percentages
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PercentUp(object sender, KeyEventArgs e)
        {
            TextBox number = (TextBox)sender;
            if (number.Name == "ChanceI")
            { }
            if (number.Name == "ChanceT")
            { }
            if (number.Name == "ChanceS")
            { }
            if (number.Name == "ChanceZ")
            { }
            if (number.Name == "ChanceJ")
            { }
            if (number.Name == "ChanceL")
            { }
            if (number.Name == "ChanceO")
            {  }
            try
            {
                TotalChance.Text = (Convert.ToInt16(ChanceI.Text) + Convert.ToInt16(ChanceT.Text) + Convert.ToInt16(ChanceS.Text) + Convert.ToInt16(ChanceZ.Text) + Convert.ToInt16(ChanceJ.Text) + Convert.ToInt16(ChanceL.Text) + Convert.ToInt16(ChanceO.Text)).ToString();
                if (Convert.ToInt16(TotalChance.Text) > 100)
                    throw new Exception();
                TotalChance.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                SafeSave = true;
            }
            catch
            {
                TotalChance.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                SafeSave = false;
            }
        }

        /// <summary>
        /// Give advice over what vaule you have selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sldSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (lblSpeed != null)
            {
                if (sldSpeed.Value <= 5000000)
                {
                    lblSpeed.Content = "Super Fast";
                }
                if (sldSpeed.Value > 5000000 && sldSpeed.Value <= 9000000)
                {
                    lblSpeed.Content = "Fast";
                }
                if (sldSpeed.Value > 9000000 && sldSpeed.Value <= 11000000)
                {
                    lblSpeed.Content = "Normal";
                }
                if (sldSpeed.Value > 11000000 && sldSpeed.Value <= 13000000)
                {
                    lblSpeed.Content = "Low";
                }
                if (sldSpeed.Value > 13000000 && sldSpeed.Value <= 15000000)
                {
                    lblSpeed.Content = "Super Slow";
                }
            }
        }
    }
}
