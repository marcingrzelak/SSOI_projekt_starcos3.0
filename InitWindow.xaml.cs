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
    /// Interaction logic for InitWindow.xaml
    /// </summary>
    public partial class InitWindow : Window
    {
        public PCSCReader Reader { get; set; } = new PCSCReader();
        public ConsoleTraceListener ConsoleTraceListener { get; set; } = new ConsoleTraceListener();
        public string[] Readers { get; set; }
        public bool IsSelectedType { get; set; }
        public bool IsSelectedInitAmount { get; set; }

        private string _selectedReader;
        RadioButton[] Types = new RadioButton[2];
        RadioButton[] InitAmounts = new RadioButton[5];

        BrushConverter converter = new BrushConverter();
        Brush ErrorBrush, AccentBrush;


        public InitWindow()
        {
            InitializeComponent();
            Trace.Listeners.Add(ConsoleTraceListener);
            Readers = Reader.SCard.ListReaders();
            ReadersList.ItemsSource = Readers;

            ErrorBrush = (Brush)converter.ConvertFromString("#FFD0284C");
            AccentBrush = (Brush)converter.ConvertFromString("#FF0086AF");            

            Types[0] = ClientRadioButton;
            Types[1] = VendorRadioButton;

            InitAmounts[0] = InitAmount1RadioButton;
            InitAmounts[1] = InitAmount2RadioButton;
            InitAmounts[2] = InitAmount3RadioButton;
            InitAmounts[3] = InitAmount4RadioButton;
            InitAmounts[4] = InitAmount5RadioButton;
        }

        private void Connect()
        {
            Reader.Connect(_selectedReader);
            Reader.ActivateCard(GS.SCard.Const.SCARD_SHARE_MODE.Shared, GS.SCard.Const.SCARD_PROTOCOL.T1);
        }

        private void CheckSelection()
        {
            foreach (var item in Types)
            {
                if (item.IsChecked == true)
                {
                    IsSelectedType = true;
                }
            }

            foreach (var item in InitAmounts)
            {
                if (item.IsChecked == true)
                {
                    IsSelectedInitAmount = true;
                }
            }
        }

        private void InitButton_Click(object sender, RoutedEventArgs e)
        {
            CheckSelection();
            if (IsSelectedType == true && IsSelectedInitAmount == true)
            {
                //#todo
                //wysylanie ramek APDU do karty - inicjalizacja
                InitStatusTextBox.Foreground = AccentBrush;
                InitStatusTextBox.Text = "Karta zainicjalizowana!";
            }

            else
            {
                InitStatusTextBox.Foreground = ErrorBrush;

                if(IsSelectedType==false && IsSelectedInitAmount==true)
                {
                    InitStatusTextBox.Text = "Wybierz typ!";
                }

                else if (IsSelectedType == true && IsSelectedInitAmount == false)
                {
                    InitStatusTextBox.Text = "Wybierz kwotę początkową!";
                }

                else
                {
                    InitStatusTextBox.Text = "Wybierz typ i kwotę początkową!";
                }

            }
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

                //#todo
                //wysylanie ramek APDU do karty - sprawdzenie czy karta jest już zainicjalizowana
                SelectedReaderTextBox.Text = _selectedReader;
                SelectedReaderLabel.Foreground = AccentBrush;
                InitButton.IsEnabled = true;
                TypeGroupBox.IsEnabled = true;
                InitAmountGroupBox.IsEnabled = true;
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
