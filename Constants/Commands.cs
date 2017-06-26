namespace Eportmonetka.Constants
{
    public static class Commands
    {
        public const string SelectMf = "00 A4 00 0C 02 3F 00";
        public const string SelectInfocard = "00 A4 02 0C 02 00 01";
        public const string ReadInfocard = "00 B2 01 04 00";
        public const string UpdateInfocardClient = "00 DC 01 04 01 10";
        public const string UpdateInfocardVendor = "00 DC 01 04 01 01";
        public const string SelectDfUser = "00 A4 01 0C 02 00 10";
        public const string SelectUserRsa = "00 A4 02 0C 02 00 01";
        public const string ChangeUserPin = "00 24 00 81 0D 31 32 33 34 35"; //dopisac
        public const string UnlockUserPin = "00 20 00 81 08"; //dopisac
        public const string ReadFirstUserRsaRecord = "00 B2 01 04 00";
        public const string ReadSecondUserRsaRecord = "00 B2 02 04 00";
        public const string ReadThirdUserRsaRecord = "00 B2 03 04 00";
        public const string SelectHash = "00 A4 02 0C 02 00 02";
        public const string ReadFirstHashRecord = "00 B2 01 04 00";
        public const string AddRecordToHash = "00 E2 00 00 14"; //dopisac
        public const string SelectDfBank = "00 A4 01 0C 02 00 11";
        public const string ChangeBankPin = "00 24 00 81 0D 31 32 33 34 35";
        public const string UnlockBankPin = "00 20 00 81 08";
        public const string SelectBankRsa = "00 A4 02 0C 02 00 01";
        public const string ReadFirstBankRsaRecord = "00 B2 01 04 00";
        public const string SelectCash = "00 A4 02 0C 02 00 02";
        public const string ReadCash = "00 B2 01 04 10";
        public const string UpdateCash = "00 DC 01 04 10"; //dopisac
        public const string UpdateCashInit1 = "00 DC 01 04 10 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 F4"; //do testow, powinno byc zaszyfrowane, 5-ty bajt to dlugosc - trzeba zmieniac
        public const string UpdateCashInit2 = "00 DC 01 04 10 00 00 00 00 00 00 00 00 00 00 00 00 00 00 03 E8"; //do testow, powinno byc zaszyfrowane
        public const string UpdateCashInit3 = "00 DC 01 04 10 00 00 00 00 00 00 00 00 00 00 00 00 00 00 07 D0"; //do testow, powinno byc zaszyfrowane
        public const string UpdateCashInit4 = "00 DC 01 04 10 00 00 00 00 00 00 00 00 00 00 00 00 00 00 13 88"; //do testow, powinno byc zaszyfrowane
        public const string UpdateCashInit5 = "00 DC 01 04 10 00 00 00 00 00 00 00 00 00 00 00 00 00 00 27 10"; //do testow, powinno byc zaszyfrowane
    }
}
