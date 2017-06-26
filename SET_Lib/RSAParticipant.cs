using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Eportmonetka.SET_Lib
{
    public class RSAParticipant
    {
        private RSACryptoServiceProvider RSACSP;
        private SHA1 sha1;

        public string PrivateKey { get; private set; }
        public string PublicKey { get; private set; }
        public List<AESKey> AESKeysList { get; private set; }

        public string[] SafeParams { get; private set; }

        public RSAParticipant()
        {
            AESKeysList = new List<AESKey>();
            sha1 = new SHA1CryptoServiceProvider();

            SafeParams = new string[8];
        }
        public bool KeysGeneration(int keySize)
        {
            try
            {
                RNGCryptoServiceProvider RNG = new RNGCryptoServiceProvider();
                byte[] seed = new byte[ConstRSA.SafeLengthKeyGen];
                RNG.GetBytes(seed);

                CspParameters CSP = new CspParameters(1);
                CSP.KeyContainerName = ConstRSA.SecureByteToString(seed);
                CSP.Flags = CspProviderFlags.UseMachineKeyStore;
                CSP.ProviderName = "Microsoft Strong Cryptographic Provider";

                RSACSP = new RSACryptoServiceProvider(keySize, CSP);

                PrivateKey = RSACSP.ToXmlString(true); //public + private key
                PublicKey = RSACSP.ToXmlString(false); //only public key

                RSAParameters param = new RSAParameters();
                param = RSACSP.ExportParameters(true);

                SafeParams[0] = ConstRSA.SecureByteToString(param.D);
                SafeParams[1] = ConstRSA.SecureByteToString(param.DP);
                SafeParams[2] = ConstRSA.SecureByteToString(param.DQ);
                SafeParams[3] = ConstRSA.SecureByteToString(param.Exponent);
                SafeParams[4] = ConstRSA.SecureByteToString(param.InverseQ);
                SafeParams[5] = ConstRSA.SecureByteToString(param.Modulus);
                SafeParams[6] = ConstRSA.SecureByteToString(param.P);
                SafeParams[7] = ConstRSA.SecureByteToString(param.Q);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        public bool LoadKey(string[] SafeParams)
        {
            RSAParameters Param = new RSAParameters();

            Param.D = ConstRSA.SecureStringToByte(SafeParams[0]);
            Param.DP = ConstRSA.SecureStringToByte(SafeParams[1]);
            Param.DQ = ConstRSA.SecureStringToByte(SafeParams[2]);
            Param.Exponent = ConstRSA.SecureStringToByte(SafeParams[3]);
            Param.InverseQ = ConstRSA.SecureStringToByte(SafeParams[4]);
            Param.Modulus = ConstRSA.SecureStringToByte(SafeParams[5]);
            Param.P = ConstRSA.SecureStringToByte(SafeParams[6]);
            Param.Q = ConstRSA.SecureStringToByte(SafeParams[7]);

            RSACSP.ImportParameters(Param);

            return false;
        }
        public void AddAESKey(AESKey Key)
        {
            AESKeysList.Add(Key);
        }
        public byte[] SignDataBySHA1(byte[] Data)
        {
            return RSACSP.SignData(Data, sha1);
        }
        public bool VerifyDataSignedBySHA1(byte[] Data, byte[] SignedData)
        {
            return RSACSP.VerifyData(Data, sha1, SignedData);
        }
        public byte[] HashCompBySHA1(byte[] Data)
        {
            return sha1.ComputeHash(Data);
        }
        public byte[] EncryptRSA(byte[] Data, bool Padding)
        {
            return RSACSP.Encrypt(Data, Padding);
        }
        public byte[] DecryptRSA(byte[] Data, bool Padding)
        {
            return RSACSP.Decrypt(Data, Padding);
        }
    }

}
