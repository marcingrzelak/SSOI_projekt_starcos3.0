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
using System.ComponentModel;

namespace Eportmonetka
{
    /// <summary>
    /// Interaction logic for TransactionWindow.xaml
    /// </summary>
    /// 
    public partial class TransactionWindow : Window
    {
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
            Items.Add(new Product() { IsChecked = false, Name = "Piwo", Price = 4, Quantity = 0 });
            Items.Add(new Product() { IsChecked = false, Name = "Wódka", Price = 20, Quantity = 0 });
            Items.Add(new Product() { IsChecked = false, Name = "Likier", Price = 10, Quantity = 0 });
            ProductsListView.ItemsSource = Items;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ProductsListView.ItemsSource);
            view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
        }

        private void SelectClientReaderButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedClientReaderTextBox.Text = "OMNIKEY READER CL 0 5637";
        }

        private void SelectVendorReaderButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedVendorReaderTextBox.Text = "OMNIKEY READER CL 0 5637";
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

            SumTextBox.Text = sum.ToString() + " PLN";
        }
    }
}
