using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Eportmonetka.SET_Lib
{
    class Program
    {
        static string ExtractString(string s, string tag)
        {
            // You should check for errors in real-world code, omitted for brevity
            var startTag = "<" + tag + ">";
            int startIndex = s.IndexOf(startTag) + startTag.Length;
            int endIndex = s.IndexOf("</" + tag + ">", startIndex);
            return s.Substring(startIndex, endIndex - startIndex);
        }
        static int Tosia()
        {
            // Stworzenie obiektów wykonujace operacje RSA
            RSAParticipant User = new RSAParticipant();
            RSAParticipant Sklep = new RSAParticipant();
            RSAParticipant Bank = new RSAParticipant();

            if (Bank.KeysGeneration(ConstRSA.RSAKeyLength) && User.KeysGeneration(ConstRSA.RSAKeyLength) && Sklep.KeysGeneration(ConstRSA.RSAKeyLength))
            {
                Console.WriteLine("Wygenerowano klucze");
            }
            else
            {
                Console.WriteLine("Błąd generowania kluczy.");
                Console.ReadLine();
                return -1;
            }

            // Tworzenie paragonu:
            Transaction Paragon = new Transaction("Samuel 'Bizon' Lewandowski", "Monopolowy u Wuja");
            Paragon.AddToTransac(new TransacItem("Piwo Żubr 0,5L Puszka", 10, 1.99f));
            Paragon.AddToTransac(new TransacItem("Wódka Starogardzka, 0,7", 3, 14.99f));

            bool[] Rules = { true, true, true, true };
            string FullParagon = Paragon.Print(Rules, 0);

            // Obliczanie hashy:
            Rules[2] = false; // ukrycie listy zakupów
            string BankParagon = Paragon.Print(Rules, 0);
            byte[] H1 = User.HashCompBySHA1(Encoding.ASCII.GetBytes(BankParagon));
            Console.WriteLine("H1:\t" + ConstRSA.SecureByteToString(H1));

            Rules[0] = false; // ukrycie kupca
            Rules[2] = true; // pokazanie listy zakupów
            string SklepParagon = Paragon.Print(Rules, 0);
            byte[] H2 = User.HashCompBySHA1(Encoding.ASCII.GetBytes(SklepParagon));
            Console.WriteLine("H2:\t" + ConstRSA.SecureByteToString(H2));

            string hash = ConstRSA.SecureByteToString(H1) + ConstRSA.SecureByteToString(H2);
            byte[] H3 = User.HashCompBySHA1(Encoding.ASCII.GetBytes(hash));
            Console.WriteLine("H1H2:\t" + hash);
            Console.WriteLine("H3:\t" + ConstRSA.SecureByteToString(H3));

            byte[] SignedH3 = User.SignDataBySHA1(H3);
            Console.WriteLine("SignH3:\t" + ConstRSA.SecureByteToString(SignedH3));

            // MSG1:
            Rules[0] = Rules[1] = Rules[3] = true;
            Rules[2] = false;
            Paragon.Hash = ConstRSA.SecureByteToString(H2);
            Paragon.SignedHash = ConstRSA.SecureByteToString(SignedH3);
            string MSG1 = Paragon.Print(Rules, 1);
            Console.WriteLine("MSG 1:\n" + MSG1);

            // Szyfrowanie MSG1:
            User.AddAESKey(new AESKey());
            string sendMSG1 = User.AESKeysList[0].CreateMSG(MSG1, ConstRSA.SecureByteToString(Bank.EncryptRSA(User.AESKeysList[0].GetKey(), false)));
            Console.WriteLine("Wysłana wiadomość MSG1 do banku:\n" + sendMSG1);

            //float Szansa = 0.001f;
            //float wynik = 0f;
            //int i = 0;
            //while(true)
            //{
            //    wynik += (float)Math.Pow(1 - Szansa, i) * Szansa;
            //    if(wynik >0.5)
            //    {
            //        break;
            //    }
            //    ++i;
            //}
            
            //Zdeszyfrowanie Klucza
            Console.WriteLine();
            byte[] AESKey = Bank.DecryptRSA(ConstRSA.SecureStringToByte(ExtractString(sendMSG1, "Key")), false);

            Bank.AddAESKey(new AESKey());
            Bank.AESKeysList.Last().SetKey(AESKey);

            string OdkodowanaWiadomosc = Bank.AESKeysList.Last().Decrypt(ConstRSA.SecureStringToByte(ExtractString(sendMSG1, "EncryptedData")));
            Console.WriteLine("Odkodowana wiadomosc:\n" + OdkodowanaWiadomosc);

            string ClearMSG1toCompHash = Regex.Replace(OdkodowanaWiadomosc, @"\t<H2>(.|\s)*<\/SignH3>\n", string.Empty);
            Console.WriteLine("Oczyszczona wiadomosc:\n" + ClearMSG1toCompHash);

            byte[] ObliczonyH1 = Bank.HashCompBySHA1(Encoding.ASCII.GetBytes(ClearMSG1toCompHash));
            Console.WriteLine("Obliczony H1:" + ConstRSA.SecureByteToString(ObliczonyH1));

            byte[] WyciagnietyH2 = ConstRSA.SecureStringToByte(ExtractString(OdkodowanaWiadomosc, "H2"));

            string ObliczonyHash3 = ConstRSA.SecureByteToString(ObliczonyH1) + ConstRSA.SecureByteToString(WyciagnietyH2);
            byte[] ObliczonyH3 = Bank.HashCompBySHA1(Encoding.ASCII.GetBytes(ObliczonyHash3));

            // TESTY:
            string[] OldParams = User.SafeParams;
            for (int i = 0; i < OldParams.Length; ++i)
            {
                byte[] temp = ConstRSA.SecureStringToByte(OldParams[i]);
                byte XOR = 0xAA;
                for (int j = 0; j < temp.Length; ++j)
                {
                    temp[j] = (byte)(temp[j] ^ XOR);
                }
                OldParams[i] = ConstRSA.SecureByteToString(temp);
            }
            string[] Params = Sklep.SafeParams;
            User.LoadKey(Params);
            Console.WriteLine("Zweryfikowane?: " + User.VerifyDataSignedBySHA1(ObliczonyH3,ConstRSA.SecureStringToByte(ExtractString(OdkodowanaWiadomosc, "SignH3"))));
            for (int i = 0; i < OldParams.Length; ++i)
            {
                byte[] temp = ConstRSA.SecureStringToByte(OldParams[i]);
                byte XOR = 0xAA;
                for (int j = 0; j < temp.Length; ++j)
                {
                    temp[j] = (byte)(temp[j] ^ XOR);
                }
                OldParams[i] = ConstRSA.SecureByteToString(temp);
            }
            User.LoadKey(OldParams);
            Console.WriteLine("Zweryfikowane?: " + User.VerifyDataSignedBySHA1(ObliczonyH3, ConstRSA.SecureStringToByte(ExtractString(OdkodowanaWiadomosc, "SignH3"))));

            Console.ReadLine();

            return 0;
        }
    }
}
