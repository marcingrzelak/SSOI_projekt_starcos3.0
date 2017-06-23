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
using System.Windows.Forms;
using System.Windows.Threading;

namespace Eportmonetka
{
    /// <summary>
    /// Interaction logic for InitWindow.xaml
    /// </summary>
    public partial class InitWindow : Window
    {
        static DispatcherTimer timer = new DispatcherTimer();
        int stop = 0;

        private void timer_Tick(Object myObject, EventArgs myEventArgs)
        {
            Width += 20;
            stop++;

            if (stop == 19)
            {
                timer.Stop();
                stop = 0;
            }
        }

        public InitWindow()
        {
            InitializeComponent();
            timer.Tick += timer_Tick;
            timer.Interval = TimeSpan.FromMilliseconds(10);
            ResizeMode = ResizeMode.NoResize;
        }

        private void SelectReaderButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedReaderTextBox.Text = "OMNIKEY READER CL 0 5637";
            //timer.Start();
        }

        private void InitButton_Click(object sender, RoutedEventArgs e)
        {
            InitStatusTextBox.Text = "Karta zainicjalizowana";
        }
    }
}
