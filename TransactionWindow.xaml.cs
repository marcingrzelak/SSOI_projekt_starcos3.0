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
using System.ComponentModel;
using static Eportmonetka.Constants.Commands;
using static Eportmonetka.Constants.ThemeColors;

namespace Eportmonetka
{
    /// <summary>
    /// Interaction logic for TransactionWindow.xaml
    /// </summary>
    /// 
    public partial class TransactionWindow : Window
    {
        public PCSCReader ClientReader { get; set; } = new PCSCReader();
        public PCSCReader VendorReader { get; set; } = new PCSCReader();
        public ConsoleTraceListener ConsoleTraceListener { get; set; } = new ConsoleTraceListener();
        public string[] ClientReaders { get; set; }
        public string[] VendorReaders { get; set; }
        public bool IsSelectedClientReader { get; set; }
        public bool IsSelectedVendorReader { get; set; }
        public int CurrentClientBalance { get; set; }
        public int CurrentVendorBalance { get; set; }
        public int TransactionAmount { get; set; }
        public bool IsClientCardType { get; set; }
        public bool IsVendorCardType { get; set; }
        public int NewClientBalance { get; set; }
        public int NewVendorBalance { get; set; }

        private string _selectedClientReader;
        private string _selectedVendorReader;

        BrushConverter converter = new BrushConverter();
        Brush ForegroundBrush, ErrorBrush, AccentBrush;

        public class Product
        {
            public bool IsChecked { get; set; }
            public string Name { get; set; }
            public double Price { get; set; }
            public int Quantity { get; set; }
        }

        public List<Product> Items { get; set; } = new List<Product>();

        public TransactionWindow()
        {
            InitializeComponent();
            Trace.Listeners.Add(ConsoleTraceListener);

            ClientReaders = ClientReader.SCard.ListReaders();
            ClientReadersList.ItemsSource = ClientReaders;

            VendorReaders = VendorReader.SCard.ListReaders();
            VendorReadersList.ItemsSource = VendorReaders;

            ForegroundBrush = (Brush)converter.ConvertFromString(Text);
            ErrorBrush = (Brush)converter.ConvertFromString(Error);
            AccentBrush = (Brush)converter.ConvertFromString(Accent);

            Items.Add(new Product() { IsChecked = false, Name = "Towar1", Price = 4.00, Quantity = 0 });
            Items.Add(new Product() { IsChecked = false, Name = "Towar2", Price = 20.00, Quantity = 0 });
            Items.Add(new Product() { IsChecked = false, Name = "Towar3", Price = 10.00, Quantity = 0 });
            Items.Add(new Product() { IsChecked = false, Name = "Towar4", Price = 7.99, Quantity = 0 });
            Items.Add(new Product() { IsChecked = false, Name = "Towar5", Price = 12.99, Quantity = 0 });
            Items.Add(new Product() { IsChecked = false, Name = "Towar6", Price = 3.59, Quantity = 0 });

            ProductsListView.ItemsSource = Items;
        }

        private void Connect(PCSCReader pReader, string pSelectedReader)
        {
            pReader.Connect(pSelectedReader);
            pReader.ActivateCard(GS.SCard.Const.SCARD_SHARE_MODE.Shared, GS.SCard.Const.SCARD_PROTOCOL.T1);
        }

        private void SelectClientReaderButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_selectedClientReader))
            {
                try
                {
                    Connect(ClientReader, _selectedClientReader);
                }
                catch (WinSCardException ex)
                {
                    MessageBox.Show(ex.WinSCardFunctionName + " Błąd 0x" + ex.Status.ToString("X08") + ": " + ex.Message);
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }

                CheckCardType(ClientReader);

                if (IsClientCardType)
                {
                    SelectedClientReaderTextBox.Foreground = AccentBrush;
                    SelectedClientReaderTextBox.Text = _selectedClientReader;
                    ReadCurrentBalance(ClientReader);
                    IsSelectedClientReader = true;
                }
                else if (IsVendorCardType)
                {
                    SelectedClientReaderTextBox.Foreground = ErrorBrush;
                    SelectedClientReaderTextBox.Text = "Wymagany typ karty: klient!";
                }

                if (IsSelectedClientReader == true && IsSelectedVendorReader == true)
                {
                    ProductsListView.IsEnabled = true;
                    SummaryButton.IsEnabled = true;
                }
            }
            else
            {
                MessageBox.Show("Zaznacz czytnik klienta!");
            }
        }

        private void SelectVendorReaderButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_selectedVendorReader))
            {
                try
                {
                    Connect(VendorReader, _selectedVendorReader);
                }
                catch (WinSCardException ex)
                {
                    MessageBox.Show(ex.WinSCardFunctionName + " Błąd 0x" + ex.Status.ToString("X08") + ": " + ex.Message);
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }

                CheckCardType(VendorReader);

                if (IsVendorCardType)
                {
                    SelectedVendorReaderTextBox.Foreground = AccentBrush;
                    SelectedVendorReaderTextBox.Text = _selectedVendorReader;
                    ReadCurrentBalance(VendorReader);
                    IsSelectedVendorReader = true;
                }
                else if (IsClientCardType)
                {
                    SelectedVendorReaderTextBox.Foreground = ErrorBrush;
                    SelectedVendorReaderTextBox.Text = "Wymagany typ karty: sprzedawca!";
                }

                if (IsSelectedClientReader == true && IsSelectedVendorReader == true)
                {
                    ProductsListView.IsEnabled = true;
                    SummaryButton.IsEnabled = true;
                }
            }
            else
            {
                MessageBox.Show("Zaznacz czytnik sprzedawcy!");
            }
        }

        private void QuantityTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).GetBindingExpression(TextBox.TextProperty).UpdateSource();
            TransactionAmount = 0;
            foreach (var item in Items)
            {
                if (item.IsChecked)
                {
                    if (item.Quantity != 0)
                    {
                        TransactionAmount += item.Quantity * (int)(item.Price * 100);
                    }
                }
            }
            if (TransactionAmount > CurrentClientBalance)
            {
                SumTextBox.Foreground = ErrorBrush;
                (sender as TextBox).GetBindingExpression(TextBox.TextProperty).UpdateSource();
            }
            else
            {
                SumTextBox.Foreground = ForegroundBrush;
                (sender as TextBox).GetBindingExpression(TextBox.TextProperty).UpdateSource();
            }
            SumTextBox.Text = ((double)TransactionAmount / 100).ToString("F2") + " PLN";
        }

        private void CheckBox_Change(object sender, RoutedEventArgs e)
        {
            TransactionAmount = 0;
            foreach (var item in Items)
            {
                if (item.IsChecked)
                {
                    if (item.Quantity != 0)
                    {
                        TransactionAmount += item.Quantity * (int)(item.Price * 100);
                    }
                }
            }
            if (TransactionAmount > CurrentClientBalance)
            {
                SumTextBox.Foreground = ErrorBrush;
            }
            else
            {
                SumTextBox.Foreground = ForegroundBrush;
            }

            SumTextBox.Text = ((double)TransactionAmount / 100).ToString("F2") + " PLN";
        }

        private void SummaryButton_Click(object sender, RoutedEventArgs e)
        {
            /*string shopList = "";
            foreach (var item in Items)
            {
                if (item.IsChecked)
                {
                    shopList += item.Name + " " + item.Quantity.ToString() + "x" + item.Price.ToString() + " ";
                }
            }
            shopList += "SUMA: " + SumTextBox.Text;*/

            if (TransactionAmount > CurrentClientBalance)
            {
                TransactionStatusTextBox.Foreground = ErrorBrush;
                TransactionStatusTextBox.Text = "Brak wystarczających środków na koncie!";
            }
            else
            {
                Transaction();
                IsClientCardType = true;
                IsVendorCardType = false;
                ReadCurrentBalance(ClientReader);
            }

        }

        private void ClientReadersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedClientReader = ClientReadersList.SelectedItem.ToString();
        }

        private void VendorReadersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedVendorReader = VendorReadersList.SelectedItem.ToString();
        }

        private void SendApdu(string command, PCSCReader Reader)
        {
            try
            {
                RespApdu respApdu = Reader.Exchange(command);

                if (respApdu.SW1SW2 == 0x9000)
                {
                    if (respApdu.Data != null)
                    {
                        if (command == ReadCash)
                        {
                            if(IsClientCardType)
                            {
                                CurrentClientBalance = Convert.ToInt32(HexFormatting.ToHexString(respApdu.Data, true).Replace(" ", ""), 16);
                                CurrentClientBalanceTextLabel.Content = ((double)CurrentClientBalance / 100).ToString("F2") + " PLN";
                            }
                            else if (IsVendorCardType)
                            {
                                CurrentVendorBalance = Convert.ToInt32(HexFormatting.ToHexString(respApdu.Data, true).Replace(" ", ""), 16);
                            }
                        }

                        if (command == ReadInfocard)
                        {
                            string CardResponse = HexFormatting.ToHexString(respApdu.Data, true).Replace(" ", string.Empty);
                            if (CardResponse == "10")
                            {
                                IsClientCardType = true;
                                IsVendorCardType = false;
                            }
                            if (CardResponse == "01")
                            {
                                IsClientCardType = false;
                                IsVendorCardType = true;
                            }
                        }
                        //RechargeStatusTextBox.Text += HexFormatting.ToHexString(respApdu.Data, true);
                    }
                    int response = Convert.ToInt32(respApdu.SW1SW2);
                    //RechargeStatusTextBox.Text += "Response: " + response.ToString("X").Insert(2, " ");
                }
                else
                {
                    int response = Convert.ToInt32(respApdu.SW1SW2);
                    //RechargeStatusTextBox.Text += "Error code: " + response.ToString("X").Insert(2, " ");
                }
            }
            catch (Exception ex)
            {
                //StatusTextBox.Text += ex.Message;
            }
        }

        private void CheckCardType(PCSCReader Reader)
        {
            SendApdu(SelectMf, Reader);
            SendApdu(SelectInfocard, Reader);
            SendApdu(ReadInfocard, Reader);
        }

        private void ReadCurrentBalance(PCSCReader Reader)
        {
            SendApdu(SelectMf, Reader);
            SendApdu(SelectDfBank, Reader);
            SendApdu(UnlockBankPin + "32 32 32 32 32 32 32 32", Reader);
            SendApdu(SelectCash, Reader);
            SendApdu(ReadCash, Reader);
        }

        private void Transaction()
        {
            IsClientCardType = true;
            IsVendorCardType = false;

            SendApdu(SelectMf, ClientReader);
            SendApdu(SelectDfBank, ClientReader);
            SendApdu(UnlockBankPin + "32 32 32 32 32 32 32 32", ClientReader);
            SendApdu(SelectCash, ClientReader);
            NewClientBalance = CurrentClientBalance - TransactionAmount;
            SendApdu(UpdateCash + NewClientBalance.ToString("X32"), ClientReader);

            IsClientCardType = false;
            IsVendorCardType = true;

            SendApdu(SelectMf, VendorReader);
            SendApdu(SelectDfBank, VendorReader);
            SendApdu(UnlockBankPin + "32 32 32 32 32 32 32 32", VendorReader);
            SendApdu(SelectCash, VendorReader);

            NewVendorBalance = CurrentVendorBalance + TransactionAmount;
            SendApdu(UpdateCash + NewVendorBalance.ToString("X32"), VendorReader);
        }
    }
}