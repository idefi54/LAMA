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

namespace LAMA.Communicator
{
    internal static class Encryption
    {
        private static RSACryptoServiceProvider rsaProvider;
        private static RSAParameters rsaPrivate;
        private static RSAParameters rsaPublic;

        private static byte[] AESkey;

        public static void GenerateRSAKeys()
        {
            rsaProvider = new RSACryptoServiceProvider(1024);

            //get the private key
            rsaPrivate = rsaProvider.ExportParameters(true);

            //get public key
            rsaPublic = rsaProvider.ExportParameters(false);
        }

        public static string GetPublicRSAKey()
        {
            var sw = new System.IO.StringWriter();
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            xs.Serialize(sw, rsaPublic);
            return sw.ToString();
        }

        public static RSAParameters DecodePublicKey(string keyString)
        {
            var sr = new System.IO.StringReader(keyString);
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            return (RSAParameters)xs.Deserialize(sr);
        }

        public static string DecryptRSA(string decriptedString)
        {
            rsaProvider.ImportParameters(rsaPrivate);
            byte[] bytes = System.Text.Encoding.Unicode.GetBytes(decriptedString);
            byte[] bytesDecripted = rsaProvider.Decrypt(bytes, true);
            return System.Text.Encoding.Unicode.GetString(bytesDecripted);
        }

        public static string EncryptRSA(string encryptedString, string publicKey)
        {
            rsaProvider.ImportParameters(DecodePublicKey(publicKey));
            byte[] bytes = System.Text.Encoding.Unicode.GetBytes(encryptedString);
            byte[] bytesEncrypted = rsaProvider.Encrypt(bytes, true);
            string textEncrypted = "Encryption1;" + System.Text.Encoding.Unicode.GetString(bytesEncrypted);
            return textEncrypted;
        }

        public static string EncryptPassword(string password)
        {
            SHA256 sha256 = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(password);
            byte[] encrypted = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(encrypted);
        }

        public static string EncryptAES(string inputString, bool ECB = true)
        {
            //Debug.WriteLine($"input: {inputString} \n");
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
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(inputString);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
                if (!ECB)
                {
                    myAes.IV.Concat(encrypted);
                }
                return System.Convert.ToBase64String(encrypted);
            }
        }

        public static byte[] EncryptStringToBytes_Aes(string plainText, bool ECB = true)
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
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
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

        public static string DecryptAES(string inputString, bool ECB = true)
        {
            byte[] bytes = System.Convert.FromBase64String(inputString);
            string decrypted = "";
            while (bytes.Length > 0) {
                int offset = 0;
                string message = ReadSingleMessageFromEncrypted(bytes, out offset, ECB);
                decrypted = decrypted + message;
                bytes = bytes.Skip(offset).ToArray();
            }
            return decrypted;
        }

        public static string DecryptStringFromBytes_Aes(byte[] bytes, bool ECB = true)
        {
            string decrypted = "";
            while (bytes.Length > 0)
            {
                int offset = 0;
                string message = ReadSingleMessageFromEncrypted(bytes, out offset, ECB);
                decrypted = decrypted + message;
                bytes = bytes.Skip(offset).ToArray();
            }
            return decrypted;
        }

        private static string ReadSingleMessageFromEncrypted(byte[] encrypted, out int offset, bool ECB = true)
        {
            offset = 0;
            string decrypted = "";
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
                        while (csDecrypt.Read(buffer, 0, 16) != 0)
                        {
                            offset += 16;
                            string decodingBuffer = Encoding.UTF8.GetString(buffer);
                            decryptedBytes.AddRange(buffer);
                            Debug.WriteLine(decodingBuffer + "\n");
                            if (decodingBuffer.Contains("|"))
                            {
                                decrypted = decrypted + decodingBuffer.Substring(0, decodingBuffer.IndexOf("|") + 1);
                                csDecrypt.Close();
                                break;
                            }
                            else
                            {
                                decrypted = decrypted + decodingBuffer;
                            }
                        }
                    }
                }
            }
            return Encoding.UTF8.GetString(decryptedBytes.ToArray());
            //return decrypted;
        }

        public static void SetAESKey(string Key)
        {
            AESkey = Encoding.UTF8.GetBytes(Key).Take(32).ToArray();
        }
    }

    internal class AESSettings
    {
        public byte[] Key;
        public byte[] IV;
    }
}