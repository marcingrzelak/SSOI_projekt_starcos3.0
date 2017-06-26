using System;
using System.Linq;

namespace Eportmonetka.SET_Lib
{
    public static class ConstRSA
    {
        public const int SafeLengthKeyGen = 128; // size seed for RSA key gen
        public const int RSAKeyLength = 1024; // 1024 is our option
        public const int AESKeyLength = 256; // 256 is our option

        public static string SecureByteToString(byte[] Array)
        {
            return BitConverter.ToString(Array).Replace("-", string.Empty).ToLower();
        }
        public static byte[] SecureStringToByte(string text)
        {
            return Enumerable.Range(0, text.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(text.Substring(x, 2), 16))
                     .ToArray();
        }
    }
}

