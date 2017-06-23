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

namespace Eportmonetka
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitButton_Click(object sender, RoutedEventArgs e)
        {
            InitWindow window = new InitWindow();
            window.ShowDialog();
        }

        private void TransactionButton_Click(object sender, RoutedEventArgs e)
        {
            TransactionWindow window = new TransactionWindow();
            window.ShowDialog();
        }

        private void RechargeButton_Click(object sender, RoutedEventArgs e)
        {
            RechargeWindow window = new RechargeWindow();
            window.ShowDialog();
        }
    }
}
