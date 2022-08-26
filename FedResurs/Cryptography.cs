using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace FedResurs
{
    public static class Cryptography
    {
        public static string Decrypt(string cipherHex, string keyHex, string IVHex)
        {
            return DecryptStringFromBytes(
                StringToByteArray(cipherHex),
                StringToByteArray(keyHex),
                StringToByteArray(IVHex)
                );
        }
        public static string Decrypt(byte[] cipherText, byte[] key, byte[] IV)
        {
            // Create AesManaged    
            using (AesManaged aes = new AesManaged())
            {
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform decryptor = aes.CreateDecryptor(key, IV);

                using (MemoryStream ms = new MemoryStream(cipherText))
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (StreamReader reader = new StreamReader(cs))
                    return reader.ReadToEnd();
            }
        }

        public static string DecryptAES(byte[] cipher, byte[] aes_key, byte[] aes_iv)
        {
            string decrypted = null;

            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.Key = aes_key;
                aes.IV = aes_iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform dec = aes.CreateDecryptor(aes.Key, aes.IV);

                using (var ms = new MemoryStream(cipher))
                using (var cs = new CryptoStream(ms, dec, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                    decrypted = sr.ReadToEnd();
            }

            return decrypted;
        }

        static string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an RijndaelManaged object
            // with the specified key and IV.
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;
                rijAlg.Padding = PaddingMode.None;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        var buffer = new byte[16];
                        csDecrypt.Read(buffer, 0, 16);
                        return BitConverter.ToString(buffer).Replace("-", "").ToLower();
                    }
                }

            }

            return plaintext;

        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

    }
}
