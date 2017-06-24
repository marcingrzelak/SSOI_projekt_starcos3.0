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
        public double CurrentBalance { get; set; }

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

            ForegroundBrush = (Brush)converter.ConvertFromString("#BABABA");
            ErrorBrush = (Brush)converter.ConvertFromString("#FFD0284C");
            AccentBrush = (Brush)converter.ConvertFromString("#FF0086AF");

            Items.Add(new Product() { IsChecked = false, Name = "Towar1", Price = 4.00, Quantity = 0 });
            Items.Add(new Product() { IsChecked = false, Name = "Towar2", Price = 20.00, Quantity = 0 });
            Items.Add(new Product() { IsChecked = false, Name = "Towar3", Price = 10.00, Quantity = 0 });
            Items.Add(new Product() { IsChecked = false, Name = "Towar4", Price = 7.99, Quantity = 0 });
            Items.Add(new Product() { IsChecked = false, Name = "Towar5", Price = 12.99, Quantity = 0 });
            Items.Add(new Product() { IsChecked = false, Name = "Towar6", Price = 3.59, Quantity = 0 });

            ProductsListView.ItemsSource = Items;

            //CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ProductsListView.ItemsSource);
            //view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
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
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                SelectedClientReaderTextBox.Foreground = AccentBrush;
                SelectedClientReaderTextBox.Text = _selectedClientReader;
                IsSelectedClientReader = true;
                if(IsSelectedVendorReader==true)
                {
                    ProductsListView.IsEnabled = true;
                }
                //#todo
                //wysylanie ramek APDU do karty - odczyt stanu konta kienta
                CurrentBalance = 10;
                CurrentBalanceTextBox.Content = CurrentBalance + " PLN";
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
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                SelectedVendorReaderTextBox.Foreground = AccentBrush;
                SelectedVendorReaderTextBox.Text = _selectedVendorReader;
                IsSelectedVendorReader = true;
                if (IsSelectedClientReader==true)
                {
                    ProductsListView.IsEnabled = true;
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
            double sum = 0.0;
            foreach (var item in Items)
            {
                if (item.IsChecked)
                {
                    if (item.Quantity != 0)
                    {
                        sum += item.Quantity * item.Price;
                    }
                }
            }
            if (sum>CurrentBalance)
            {
                SumTextBox.Foreground = Brushes.Red;
                (sender as TextBox).GetBindingExpression(TextBox.TextProperty).UpdateSource();
            }
            else
            {
                SumTextBox.Foreground = ForegroundBrush;
                (sender as TextBox).GetBindingExpression(TextBox.TextProperty).UpdateSource();
            }
            SumTextBox.Text = sum.ToString() + " PLN";
        }

        private void CheckBox_Change(object sender, RoutedEventArgs e)
        {
            double sum = 0.0;
            foreach (var item in Items)
            {
                if (item.IsChecked)
                {
                    if (item.Quantity != 0)
                    {
                        sum += item.Quantity * item.Price;
                    }
                }
            }
            if (sum > CurrentBalance)
            {
                SumTextBox.Foreground = Brushes.Red;
            }
            else
            {
                SumTextBox.Foreground = ForegroundBrush;
            }

            SumTextBox.Text = sum.ToString() + " PLN";
        }

        private void SummaryButton_Click(object sender, RoutedEventArgs e)
        {
            string shopList = "";
            foreach (var item in Items)
            {
                if (item.IsChecked)
                {
                    shopList += item.Name + " " + item.Quantity.ToString() + "x" + item.Price.ToString() + " ";
                }
            }
            shopList += "SUMA: " + SumTextBox.Text;
            TransactionSummaryWindow window = new TransactionSummaryWindow();
            window.ShowDialog();
        }

        private void ClientReadersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedClientReader = ClientReadersList.SelectedItem.ToString();
        }

        private void VendorReadersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedVendorReader = VendorReadersList.SelectedItem.ToString();
        }
    }
}