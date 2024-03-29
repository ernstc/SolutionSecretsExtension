﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SolutionSecrets.Core.Encryption
{
    public class Cipher : ICipher
    {

        const string APP_DATA_FILENAME = "cipher.json";

        private string _key;


        class CipherAppData
        {
            public string key { get; set; }
        }


        public Task<bool> IsReady()
        {
            return Task.FromResult(!string.IsNullOrEmpty(_key));
        }


        public Task RefreshStatus()
        {
            var cipherAppData = AppData.LoadData<CipherAppData>(APP_DATA_FILENAME);
            _key = cipherAppData?.key;
            return Task.CompletedTask;
        }


        public void Init(string passphrase)
        {
            if (String.IsNullOrWhiteSpace(passphrase))
            {
                // Remove the key
                AppData.SaveData(APP_DATA_FILENAME, new { });
                return;
            }

            byte[] data = UTF8Encoding.UTF8.GetBytes(passphrase);
            GenerateEncryptionKey(data);
        }


        public void Init(Stream keyfile)
        {
            if (keyfile == null)
            {
                // Remove the key
                AppData.SaveData(APP_DATA_FILENAME, new { });
                return;
            }

            byte[] data = new byte[keyfile.Length];
            int read = keyfile.Read(data, 0, data.Length);
            if (read != data.Length)
            {
                Console.WriteLine("    ERR: Error loading key file.");
                return;
            }
            GenerateEncryptionKey(data);
        }


        private void GenerateEncryptionKey(byte[] data)
        {
            byte[] result;
            var hashAlgorithm = HashAlgorithm.Create("SHA512");
            if (hashAlgorithm == null)
            {
                Console.WriteLine("    ERR: Cannot create encryption key.");
                return;
            }

            result = hashAlgorithm.ComputeHash(data);
            _key = Convert.ToBase64String(result);

            AppData.SaveData(APP_DATA_FILENAME, new CipherAppData
            {
                key = _key
            });
        }


        public string Encrypt(string plainText)
        {
            if (
                plainText != null
                && plainText.Length > 0
                && _key != null
                && _key.Length > 0
                )
	        {
	            try
	            {
	                byte[] encrypted;
	                byte[] Key = new byte[32];
	                byte[] IV = new byte[16];
	
	                byte[] keyBytes = Convert.FromBase64String(_key);
	                for (int i = 0; i < Key.Length; i++) Key[i] = keyBytes[i];
	                for (int i = 0; i < IV.Length; i++) IV[i] = keyBytes[i + 32];
	
	                // Create an Aes object
	                // with the specified key and IV.
	                using (Aes aesAlg = Aes.Create())
	                {
	                    aesAlg.Key = Key;
	                    aesAlg.IV = IV;
	
	                    // Create an encryptor to perform the stream transform.
	                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
	
	                    // Create the streams used for encryption.
	                    using (MemoryStream msEncrypt = new MemoryStream())
	                    {
	                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
	                        {
	                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
	                            {
	                                //Write all data to the stream.
	                                swEncrypt.Write(plainText);
	                            }
	                            encrypted = msEncrypt.ToArray();
	                        }
	                    }
	                }
	
	                // Return the encrypted bytes from the memory stream.
	                return Convert.ToBase64String(encrypted);
	            }
	            catch
                { }
            }
            return null;
        }


        public string Decrypt(string encrypted)
        {
            if (
                encrypted != null
                && encrypted.Length > 0
                && _key != null
                && _key.Length > 0
                )
	        {
	            try
	            {
	                byte[] Key = new byte[32];
	                byte[] IV = new byte[16];
	
	                byte[] keyBytes = Convert.FromBase64String(_key);
	                for (int i = 0; i < Key.Length; i++) Key[i] = keyBytes[i];
	                for (int i = 0; i < IV.Length; i++) IV[i] = keyBytes[i + 32];
	
	                // Declare the string used to hold
	                // the decrypted text.
	                string plaintext;
	
	                // Create an Aes object
	                // with the specified key and IV.
	                using (Aes aesAlg = Aes.Create())
	                {
	                    aesAlg.Key = Key;
	                    aesAlg.IV = IV;
	
	                    // Create a decryptor to perform the stream transform.
	                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
	
	                    // Create the streams used for decryption.
	                    byte[] encryptedBytes = Convert.FromBase64String(encrypted);
	                    using (MemoryStream msDecrypt = new MemoryStream(encryptedBytes))
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
	            catch
                { }
            }
            return null;
        }

    }
}

