using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;
using System.Text;
using GS.Apdu;
using GS.PCSC;
using GS.SCard;
using GS.Util.Hex;
using static Eportmonetka.Constants.Commands;
using static Eportmonetka.Constants.ThemeColors;
using Eportmonetka.SET_Lib;

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
        public bool IsCardInitialized { get; set; }

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

            ErrorBrush = (Brush)converter.ConvertFromString(Error);
            AccentBrush = (Brush)converter.ConvertFromString(Accent);

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
                InitCard();
                InitStatusTextBox.Foreground = AccentBrush;
                InitStatusTextBox.Text = "Karta zainicjalizowana!";
            }

            else
            {
                InitStatusTextBox.Foreground = ErrorBrush;

                if (IsSelectedType == false && IsSelectedInitAmount == true)
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
            IsCardInitialized = false;

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

                CheckInit();

                if (IsCardInitialized)
                {
                    InitButton.IsEnabled = false;
                    TypeGroupBox.IsEnabled = false;
                    InitAmountGroupBox.IsEnabled = false;
                    InitStatusTextBox.Foreground = ErrorBrush;
                    InitStatusTextBox.Text = "Karta jest już zainicjalizowana";
                }
                else
                {
                    SelectedReaderTextBox.Text = _selectedReader;
                    SelectedReaderTextBox.Foreground = AccentBrush;
                    InitStatusTextBox.Text = "";
                    InitButton.IsEnabled = true;
                    TypeGroupBox.IsEnabled = true;
                    InitAmountGroupBox.IsEnabled = true;
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
                        if (command == ReadInfocard)
                        {
                            string CardResponse = HexFormatting.ToHexString(respApdu.Data, true).Replace(" ", string.Empty);
                            if (CardResponse == "10" || CardResponse == "01")
                            {
                                IsCardInitialized = true;
                            }
                        }
                        InitStatusTextBox.Text += HexFormatting.ToHexString(respApdu.Data, true);
                    }
                    int response = Convert.ToInt32(respApdu.SW1SW2);
                    InitStatusTextBox.Text += "Response: " + response.ToString("X").Insert(2, " ");
                }
                else
                {
                    int response = Convert.ToInt32(respApdu.SW1SW2);
                    InitStatusTextBox.Text += "Error code: " + response.ToString("X").Insert(2, " ");
                }
            }
            catch (Exception ex)
            {
                InitStatusTextBox.Text += ex.Message;
            }
        }

        private void InitCard()
        {
            SendApdu(SelectMf);
            SendApdu(SelectInfocard);

            if (ClientRadioButton.IsChecked == true)
            {
                SendApdu(UpdateInfocardClient);
            }
            else if (VendorRadioButton.IsChecked == true)
            {
                SendApdu(UpdateInfocardVendor);
            }

            SendApdu(SelectDfUser);
            SendApdu(ChangeUserPin + " 31 31 31 31 31 31 31 31");
            SendApdu(UnlockUserPin + " 31 31 31 31 31 31 31 31");

            RSAParticipant user = new RSAParticipant();
            user.KeysGeneration(ConstRSA.RSAKeyLength);
            string[] parametrs = user.SafeParams;

            SendApdu(SelectUserRsa128);
            SendApdu("00 DC 01 04 80" + parametrs[0]); //write (d)
            SendApdu("00 DC 02 04 80" + parametrs[5]); //write (N)

            SendApdu(SelectUserRsa64);
            SendApdu("00 DC 01 04 40" + parametrs[1]); //write (dp)
            SendApdu("00 DC 02 04 40" + parametrs[2]); //write (dq)
            SendApdu("00 DC 03 04 40" + parametrs[4]); //write (1/q)
            SendApdu("00 DC 04 04 40" + parametrs[6]); //write (p)
            SendApdu("00 DC 05 04 40" + parametrs[7]); //write (q)

            SendApdu(SelectUserRsa3);
            SendApdu("00 DC 01 04 03" + parametrs[3]); //write (e)

            SendApdu(SelectMf);
            SendApdu(SelectDfBank);
            SendApdu(ChangeBankPin + "32 32 32 32 32 32 32 32");
            SendApdu(UnlockBankPin + "32 32 32 32 32 32 32 32");

            if (ClientRadioButton.IsChecked == true)
            {
                SendApdu(SelectCash);

                byte[] EncryptedAmount = new byte[ConstRSA.RSAKeyLength/8];
                int amount;

                for (int i = 0; i < InitAmounts.Length; i++)
                {
                    if (InitAmounts[i].IsChecked == true)
                    {
                        amount = int.Parse(InitAmounts[i].Content.ToString().Replace(" PLN", string.Empty))*100;
                        byte[] porcje = new byte[4];
                        porcje[0] = (byte)((amount >> 24) & 255);
                        porcje[1] = (byte)((amount >> 16) & 255);
                        porcje[2] = (byte)((amount >> 8) & 255);
                        porcje[3] = (byte)((amount >> 0) & 255);
                        EncryptedAmount = MainWindow.Bank.EncryptRSA(porcje, true);
                    }
                }

                SendApdu(UpdateCash + ConstRSA.SecureByteToString(EncryptedAmount));                    
            }
        }

        private void CheckInit()
        {
            SendApdu(SelectMf);
            SendApdu(SelectInfocard);
            SendApdu(ReadInfocard);
        }
    }
}
