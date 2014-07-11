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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace wpfTetri
{
    /// <summary>
    /// Interaction logic for Game_Over.xaml
    /// </summary>
    public partial class Game_Over : UserControl
    {

        public event RestartGameEvent Restart;
        public delegate void RestartGameEvent(bool xTrue);

        public Game_Over()
        {
            InitializeComponent();
            this.Height = Game_Over_Window.Height+4;
            this.Width = Game_Over_Window.Width+4;
            Canvas.SetTop(Game_Over_Text, ((Game_Over_Window.Height / 4) * 1) - (Game_Over_Text.Height / 2));
            Canvas.SetLeft(Game_Over_Text, ((Game_Over_Window.Width / 2) - (Game_Over_Text.Width / 2)));
            Canvas.SetTop(ellRetry, ((Game_Over_Window.Height / 4)*3)-(ellRetry.Height/2));
            Canvas.SetLeft(ellRetry, ((Game_Over_Window.Width / 2) - (ellRetry.Width/2)));
            Canvas.SetTop(ellRetryBack, ((Game_Over_Window.Height / 4) * 3) - (ellRetry.Height / 2));
            Canvas.SetLeft(ellRetryBack, ((Game_Over_Window.Width / 2) - (ellRetry.Width / 2)));
        }

        /// <summary>
        /// FunctionButtons
        /// </summary>
        /// <param name="sender">MinMaxClose</param>
        /// <param name="e"></param>
        private void App_Click(object sender, RoutedEventArgs e)
        {
            Ellipse knop = (Ellipse)sender;
            if (knop.Name == "ellRetry")
            {
                this.Visibility = Visibility.Collapsed;
                Restart(true);
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
            if (knop.Name == "ellRetry")
                ellRetryBack.Fill = new SolidColorBrush(Color.FromArgb(100, 200, 0, 0));
        }

        /// <summary>
        /// change back Background behind FunctionButton
        /// </summary>
        /// <param name="sender">MinMaxClose</param>
        /// <param name="e"></param>
        private void App_MouseLeave(object sender, MouseEventArgs e)
        {
            Ellipse knop = (Ellipse)sender;
            if (knop.Name == "ellRetry")
                ellRetryBack.Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        }
    }
}
