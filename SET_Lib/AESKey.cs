using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace Eportmonetka.SET_Lib
{
    public class AESKey
    {
        private AesManaged Key = null;

        public AESKey()
        {
            Key = new AesManaged();
            Key.KeySize = ConstRSA.AESKeyLength;
            RNGCryptoServiceProvider KeyGen = new RNGCryptoServiceProvider();
            
            byte[] Values = { 6, 44, 244, 231, 33, 124, 69, 94, 34, 3, 24, 95, 140, 135, 160, 241, 90, 201, 122, 178, 2 };
            byte[] IV = new byte[Key.IV.Length];
            for (int i = 0; i < IV.Length; ++i)
            {
                IV[i] = Values[i % Values.Length];
            }

            Key.IV = IV; // z jakiegoś powodu kiedy używałem losowości był problem z odczytaniem pierwszego bloku wiadomości, teraz działa. 

            KeyGen.GetBytes(Key.Key);
        }

        public byte[] Encrypt(string data)
        {
            // Check arguments.
            if (data == null || data.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key.Key == null || Key.Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (Key.IV == null || Key.IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;
            // Create an AesManaged object
            // with the specified key and IV.
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key.Key;
                aesAlg.IV = Key.IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(data);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }
        public string Decrypt(byte[] cipherText)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key.Key == null || Key.Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (Key.IV == null || Key.IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an AesManaged object
            // with the specified key and IV.
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key.Key;
                aesAlg.IV = Key.IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }
            return plaintext;
        }
        public string CreateMSG(string MSG, string Key)
        {
            StringBuilder result = new StringBuilder();

            result.Append("<Message>\n");
            result.Append("\t<EncryptedData>" + ConstRSA.SecureByteToString(this.Encrypt(MSG)) + "</EncryptedData>\n");
            result.Append("\t<Key>" + Key + "</Key>\n");
            result.Append("</Message>");

            return result.ToString();
        }
        public byte[] GetKey()
        {
            return Key.Key;
        }

        public void SetKey(byte[] newKey)
        {
            Key.Key = newKey;
        }
    }

}
