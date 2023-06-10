using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using LAMA.Models;
using System.IO;
using static Mapsui.Providers.ArcGIS.TileInfo;
using static Xamarin.Essentials.Permissions;
using System.Linq;
using System.Diagnostics;
using Xamarin.Forms.Internals;
using Mapsui.Styles;

namespace LAMA.Communicator
{
    internal static class Encryption
    {
        private static byte[] AESkey;

        /// <summary>
        /// Encrypt password using SHA256
        /// </summary>
        /// <param name="password">Nonencrypted password</param>
        /// <returns></returns>
        public static string EncryptPassword(string password)
        {
            SHA256 sha256 = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(password);
            byte[] encrypted = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(encrypted);
        }

        public static byte[] EncryptBytesToBytes_Aes(byte[] bytes, bool ECB = true)
        {
            using (Aes myAes = Aes.Create())
            {
                myAes.KeySize = 256;
                myAes.Key = AESkey;
                myAes.IV = new byte[16];
                myAes.Padding = PaddingMode.Zeros;
                if (!ECB)
                {
                    Random random = new Random();
                    random.NextBytes(myAes.IV);
                }
                ICryptoTransform encryptor = myAes.CreateEncryptor();
                byte[] encrypted;
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(bytes, 0, bytes.Length);
                        csEncrypt.FlushFinalBlock();
                        encrypted = msEncrypt.ToArray();
                    }
                }
                if (!ECB)
                {
                    myAes.IV.Concat(encrypted);
                }
                return encrypted;
            }
        }

        /// <summary>
        /// Decrypt AES encoded message from bytes
        /// </summary>
        /// <param name="bytes">encoded message as byte array</param>
        /// <param name="ECB">Use ECB version of algorithm - true by default, this is fine because the first part of message is
        /// always different (time the message was sent in milliseconds), otherwise ECB should be set to false, this would however
        /// increase the size of encoded messages</param>
        /// <returns>Decrypted message as string</returns>
        public static byte[] DecryptFromBytesToBytes_Aes(byte[] encrypted, bool ECB = true)
        {
            int offset = 0;
            if (encrypted.Length % 16 != 0)
            {
                offset = encrypted.Length;
                return null;
            }
            offset = 0;
            List<byte> decryptedBytes = new List<byte>();
            using (Aes myAes = Aes.Create())
            {
                myAes.Key = AESkey;
                myAes.IV = new byte[16];
                myAes.Padding = PaddingMode.Zeros;

                if (!ECB)
                {
                    myAes.IV = encrypted.Take(16).ToArray();
                    encrypted = encrypted.Skip(16).ToArray();
                    offset += 16;
                }
                ICryptoTransform decryptor = myAes.CreateDecryptor();
                using (MemoryStream msDecrypt = new MemoryStream(encrypted))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        byte[] buffer = new byte[16];
                        while (encrypted.Length - offset >= 16 && csDecrypt.Read(buffer, 0, 16) != 0)
                        {
                            offset += 16;
                            string decodingBuffer = Encoding.UTF8.GetString(buffer);
                            decryptedBytes.AddRange(buffer);
                        }
                    }
                }
            }
            return decryptedBytes.ToArray();
        }

        public static byte[] HuffmanCompressAESEncode(string plainText, Compression compressor, bool ECB = true)
        {
            byte[] compressed = compressor.Encode(plainText);
            byte[] encrypted = Encryption.EncryptBytesToBytes_Aes(compressed, ECB);
            return encrypted;
        }

        public static string AESDecryptHuffmanDecompress(byte[] encrypted, Compression compressor, bool ECB = true)
        {
            int offset = 0;
            int offsetChange;
            List<string> messages = new List<string>();
            while (offset < encrypted.Length / 16)
            {
                byte[] encodedPart = encrypted.Skip(16*offset).ToArray();
                byte[] decrypted = Encryption.DecryptFromBytesToBytes_Aes(encodedPart, ECB);
                if (decrypted == null) break;
                string uncompressed = compressor.DecodeFromAESBytes(decrypted, out offsetChange);
                messages.Add(uncompressed);
                offset += offsetChange;

            }
            string result = String.Join("", messages);
            return result;
        }

        /// <summary>
        /// Set key for AES algorithm
        /// </summary>
        /// <param name="Key">String representation of the key</param>
        public static void SetAESKey(string Key)
        {
            AESkey = Encoding.UTF8.GetBytes(Key).Take(32).ToArray();
        }
    }
}