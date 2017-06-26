using System.Windows;
using System.Security.Cryptography;
using Eportmonetka.SET_Lib;
using static Eportmonetka.Constants.Keys;

namespace Eportmonetka
{
    public partial class MainWindow : Window
    {
        public static RSAParticipant Bank { get; set; } = new RSAParticipant();

        public MainWindow()
        {
            InitializeComponent();
            Bank.KeysGeneration(ConstRSA.RSAKeyLength);
            Bank.LoadKey(BankKeyParams);
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
