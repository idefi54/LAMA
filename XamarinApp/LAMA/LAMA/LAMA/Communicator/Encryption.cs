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
        private static RSACryptoServiceProvider rsaProvider;
        private static RSAParameters rsaPrivate;
        private static RSAParameters rsaPublic;

        private static byte[] AESkey;
        /// <summary>
        /// Generate the private and public key for RSA encryption
        /// </summary>
        public static void GenerateRSAKeys()
        {
            rsaProvider = new RSACryptoServiceProvider(1024);

            //get the private key
            rsaPrivate = rsaProvider.ExportParameters(true);

            //get public key
            rsaPublic = rsaProvider.ExportParameters(false);
        }

        /// <summary>
        /// Get string representation of the currently used public key
        /// </summary>
        /// <returns>string representation of the public RSA key</returns>
        public static string GetPublicRSAKey()
        {
            var sw = new System.IO.StringWriter();
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            xs.Serialize(sw, rsaPublic);
            return sw.ToString();
        }

        /// <summary>
        /// Load public RSA key from its string representation
        /// </summary>
        /// <param name="keyString">string representation of the public RSA key</param>
        /// <returns>RSA parameters with supplied public key</returns>
        public static RSAParameters DecodePublicKey(string keyString)
        {
            var sr = new System.IO.StringReader(keyString);
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            return (RSAParameters)xs.Deserialize(sr);
        }

        /// <summary>
        /// Decrypt string using RSA
        /// </summary>
        /// <param name="decryptedString">Encoded string to be decrypted</param>
        /// <returns>Decrypted message</returns>
        public static string DecryptRSA(string decryptedString)
        {
            rsaProvider.ImportParameters(rsaPrivate);
            byte[] bytes = System.Text.Encoding.Unicode.GetBytes(decryptedString);
            byte[] bytesDecripted = rsaProvider.Decrypt(bytes, true);
            return System.Text.Encoding.Unicode.GetString(bytesDecripted);
        }

        /// <summary>
        /// Encrypt message using RSA and a supplied public key.
        /// </summary>
        /// <param name="encryptedString"></param>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        public static string EncryptRSA(string encryptedString, string publicKey)
        {
            rsaProvider.ImportParameters(DecodePublicKey(publicKey));
            byte[] bytes = System.Text.Encoding.Unicode.GetBytes(encryptedString);
            byte[] bytesEncrypted = rsaProvider.Encrypt(bytes, true);
            string textEncrypted = "Encryption1;" + System.Text.Encoding.Unicode.GetString(bytesEncrypted);
            return textEncrypted;
        }

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

        /// <summary>
        /// Encrypt message using AES algorithm
        /// </summary>
        /// <param name="inputString">Message to be encrypted</param>
        /// <param name="ECB">Use ECB version of algorithm - true by default, this is fine because the first part of message is
        /// always different (time the message was sent in milliseconds), otherwise ECB should be set to false, this would however
        /// increase the size of encoded messages</param>
        /// <returns>Base64String of the bytes the message was encoded to</returns>
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

        /// <summary>
        /// Encrypt message using AES algorithm
        /// </summary>
        /// <param name="inputString">Message to be encrypted</param>
        /// <param name="ECB">Use ECB version of algorithm - true by default, this is fine because the first part of message is
        /// always different (time the message was sent in milliseconds), otherwise ECB should be set to false, this would however
        /// increase the size of encoded messages</param>
        /// <returns>encoded message as byte array</returns>
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

        public static byte[] EncryptBytesToBytes_Aes(byte[] bytes, bool ECB = true)
        {
            using (Aes myAes = Aes.Create())
            {
                myAes.KeySize = 256;
                myAes.Key = AESkey;
                myAes.IV = new byte[16];
                myAes.Padding = PaddingMode.Zeros;
                Debug.WriteLine($"bytes length: {bytes.Length}");
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
                Debug.WriteLine($"Encrypted length: {encrypted.Length}");
                return encrypted;
            }
        }

        /// <summary>
        /// Decrypt AES encoded message from Base64String
        /// </summary>
        /// <param name="inputString">encoded message in form of Base64String</param>
        /// <param name="ECB">Use ECB version of algorithm - true by default, this is fine because the first part of message is
        /// always different (time the message was sent in milliseconds), otherwise ECB should be set to false, this would however
        /// increase the size of encoded messages</param>
        /// <returns>Decrypted message as string</returns>
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

        /// <summary>
        /// Decrypt AES encoded message from bytes
        /// </summary>
        /// <param name="bytes">encoded message as byte array</param>
        /// <param name="ECB">Use ECB version of algorithm - true by default, this is fine because the first part of message is
        /// always different (time the message was sent in milliseconds), otherwise ECB should be set to false, this would however
        /// increase the size of encoded messages</param>
        /// <returns>Decrypted message as string</returns>
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
                Debug.WriteLine($"Wrong length message: {encrypted.Length}");
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
            Debug.WriteLine($"Decrypted length: {decryptedBytes.Count}");
            return decryptedBytes.ToArray();
        }
        /// <summary>
        /// Read one message from encrypted (there can be multiple messages after each other in the encrypted data)
        /// </summary>
        /// <param name="encrypted">Bytes of the message/messages we are trying to decrypt</param>
        /// <param name="offset">Start of the decrypted message (if there are more messages encrypted)</param>
        /// <param name="ECB">Use ECB version of AES?</param>
        /// <returns>Single decrypted message</returns>
        private static string ReadSingleMessageFromEncrypted(byte[] encrypted, out int offset, bool ECB = true)
        {
            if (encrypted.Length % 16 != 0)
            {
                Debug.WriteLine($"Wrong length message: {encrypted.Length}");
                offset = encrypted.Length;
                return "";
            }
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
                        while (encrypted.Length - offset >= 16 && csDecrypt.Read(buffer, 0, 16) != 0)
                        {
                            offset += 16;
                            string decodingBuffer = Encoding.UTF8.GetString(buffer);
                            decryptedBytes.AddRange(buffer);
                            if (decodingBuffer.Contains(Separators.messageSeparator))
                            {
                                decrypted = decrypted + decodingBuffer.Substring(0, decodingBuffer.IndexOf(Separators.messageSeparator) + 1);
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

    internal class AESSettings
    {
        public byte[] Key;
        public byte[] IV;
    }
}