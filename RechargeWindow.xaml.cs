using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;
using GS.Apdu;
using GS.PCSC;
using GS.SCard;
using GS.Util.Hex;
using static Eportmonetka.Constants.Commands;
using static Eportmonetka.Constants.ThemeColors;

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
        public int CurrentBalance { get; set; }
        public int RechargeAmount { get; set; }
        public bool IsClientCardType { get; set; }

        private string _selectedReader;
        BrushConverter converter = new BrushConverter();
        Brush AccentBrush, ErrorBrush;

        public RechargeWindow()
        {
            InitializeComponent();
            Trace.Listeners.Add(ConsoleTraceListener);
            Readers = Reader.SCard.ListReaders();
            ReadersList.ItemsSource = Readers;

            AccentBrush = (Brush)converter.ConvertFromString(Accent);
            ErrorBrush = (Brush)converter.ConvertFromString(Error);
        }

        private void Connect()
        {
            Reader.Connect(_selectedReader);
            Reader.ActivateCard(GS.SCard.Const.SCARD_SHARE_MODE.Shared, GS.SCard.Const.SCARD_PROTOCOL.T1);
        }

        private void RechargeButton_Click(object sender, RoutedEventArgs e)
        {
            RechargeCard();
            RechargeStatusTextBox.Foreground = AccentBrush;
            RechargeStatusTextBox.Text = "Karta doładowana kwotą " + AmountTextBox.Text + " PLN";
            CurrentBalanceTextLabel.Content = ((double)RechargeAmount / 100).ToString("F2") + " PLN";
            ReadCurrentBalance();
        }

        private void SelectReaderButton_Click(object sender, RoutedEventArgs e)
        {
            IsClientCardType = false;

            if (!string.IsNullOrEmpty(_selectedReader))
            {
                try
                {
                    Connect();
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

                CheckCardType();

                if (IsClientCardType)
                {
                    SelectedReaderTextBox.Foreground = AccentBrush;
                    SelectedReaderTextBox.Text = _selectedReader;
                    RechargeStatusTextBox.Text = "";
                    RechargeButton.IsEnabled = true;
                    AmountSlider.IsEnabled = true;
                    AmountTextBox.IsEnabled = true;
                    ReadCurrentBalance();
                }
                else
                {
                    SelectedReaderTextBox.Foreground = AccentBrush;
                    SelectedReaderTextBox.Text = _selectedReader;
                    ReadCurrentBalance();
                    RechargeButton.IsEnabled = false;
                    AmountSlider.IsEnabled = false;
                    AmountTextBox.IsEnabled = false;
                    RechargeStatusTextBox.Foreground = ErrorBrush;
                    RechargeStatusTextBox.Text = "Nie można doładować karty sprzedawcy!";                    
                }
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

        private void SendApdu(string command)
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
                            CurrentBalance = Convert.ToInt32(HexFormatting.ToHexString(respApdu.Data, true).Replace(" ", ""), 16);
                            CurrentBalanceTextLabel.Content = ((double)CurrentBalance / 100).ToString("F2") + " PLN";
                        }

                        if (command == ReadInfocard)
                        {
                            string CardResponse = HexFormatting.ToHexString(respApdu.Data, true).Replace(" ", string.Empty);
                            if (CardResponse == "10")
                            {
                                IsClientCardType = true;
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
                //RechargeStatusTextBox.Text += ex.Message;
            }
        }

        private void ReadCurrentBalance()
        {
            SendApdu(SelectMf);
            SendApdu(SelectDfBank);
            SendApdu(UnlockBankPin + "32 32 32 32 32 32 32 32");
            SendApdu(SelectCash);
            SendApdu(ReadCash);
        }

        private void RechargeCard()
        {
            SendApdu(SelectMf);
            SendApdu(SelectDfBank);
            SendApdu(UnlockBankPin + "32 32 32 32 32 32 32 32");
            SendApdu(SelectCash);

            RechargeAmount = (int.Parse(AmountTextBox.Text) * 100) + CurrentBalance;

            SendApdu(UpdateCash + RechargeAmount.ToString("X32"));
        }

        private void CheckCardType()
        {
            SendApdu(SelectMf);
            SendApdu(SelectInfocard);
            SendApdu(ReadInfocard);
        }
    }
}
