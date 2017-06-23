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

        public RechargeWindow()
        {
            InitializeComponent();
            Trace.Listeners.Add(ConsoleTraceListener);
            Readers = Reader.SCard.ListReaders();
            ReadersList.ItemsSource = Readers;
        }

        private void RechargeButton_Click(object sender, RoutedEventArgs e)
        {
            RechargeStatusTextBox.Text = "Karta doładowana, " + AmountTextBox.Text + "PLN";
        }

        private void SelectReaderButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedReaderTextBox.Text = "OMNIKEY READER CL 0 5637";
        }
    }
}
