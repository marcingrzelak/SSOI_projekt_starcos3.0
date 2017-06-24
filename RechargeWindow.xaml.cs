using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using GS.Apdu;
using GS.PCSC;
using GS.SCard;
using GS.Util.Hex;

namespace Eportmonetka
{
    /// <summary>
    /// Interaction logic for RechargeWindow.xaml
    /// </summary>
    public partial class RechargeWindow : Window
    {
        public PCSCReader Reader { get; set; } = new PCSCReader();
        public ConsoleTraceListener ConsoleTraceListener { get; set; } = new ConsoleTraceListener();
        public string[] Readers { get; set; }
        public double CurrentBalance { get; set; }

        private string _selectedReader;

        public RechargeWindow()
        {
            InitializeComponent();
            Trace.Listeners.Add(ConsoleTraceListener);
            Readers = Reader.SCard.ListReaders();
            ReadersList.ItemsSource = Readers;
        }

        private void Connect()
        {
            Reader.Connect(_selectedReader);
            Reader.ActivateCard(GS.SCard.Const.SCARD_SHARE_MODE.Shared, GS.SCard.Const.SCARD_PROTOCOL.T1);
        }

        private void RechargeButton_Click(object sender, RoutedEventArgs e)
        {
            //#todo
            //wysylanie ramek APDU do karty - doładowanie
            //zmiana stanu konta (textBox)
            RechargeStatusTextBox.Foreground = Brushes.Green;
            RechargeStatusTextBox.Text = "Karta doładowana kwotą " + AmountTextBox.Text + " PLN";
        }

        private void SelectReaderButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_selectedReader))
            {
                try
                {
                    Connect();
                }
                catch (WinSCardException ex)
                {
                    MessageBox.Show(ex.WinSCardFunctionName + " Błąd 0x" + ex.Status.ToString("X08") + ": " + ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                SelectedReaderTextBox.Foreground = Brushes.Green;
                SelectedReaderTextBox.Text = _selectedReader;
                RechargeButton.IsEnabled = true;
                //#todo
                //wysylanie ramek APDU do karty - odczyt stanu konta
            }
            else
            {
                MessageBox.Show("Zaznacz czytnik!");
            }
        }

        private void ReadersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedReader = ReadersList.SelectedItem.ToString();
        }
    }
}
