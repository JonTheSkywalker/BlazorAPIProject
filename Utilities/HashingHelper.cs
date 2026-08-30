using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace BlazorAPIProject.Utilities
{
    public class HashingHelper
    {
        private byte[] Key = new byte[32];
        private byte[] IV = new byte[16];

        public HashingHelper(IConfiguration configuration)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] keyBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(configuration["HashingKey:Key"]!));
                byte[] ivBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(configuration["HashingKey:Key"]!));

                Key = new byte[32];
                IV = new byte[16];
                Array.Copy(keyBytes, Key, 32);
                Array.Copy(ivBytes, IV, 16);
            }
        }

        public byte[]? EncryptByte<T>(T toEncrypt)
        {
            string json = JsonConvert.SerializeObject(toEncrypt);
            byte[] encrypted;

            try
            {
                using Aes aes = Aes.Create();
                aes.Key = Key;
                aes.IV = IV;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using MemoryStream ms = new MemoryStream();
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (StreamWriter sw = new StreamWriter(cs))
                {
                    sw.Write(json);
                }
                encrypted = ms.ToArray();
            }
            catch
            {
                Console.WriteLine("Encryption failed. Please check the key and IV.");
                return null;
            }
            return encrypted;
        }

        public string EncryptString<T>(T toEncrypt)
        {
            string json = JsonConvert.SerializeObject(toEncrypt);
            byte[] encrypted;

            try
            {
                using Aes aes = Aes.Create();
                aes.Key = Key;
                aes.IV = IV;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using MemoryStream ms = new MemoryStream();
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (StreamWriter sw = new StreamWriter(cs))
                {
                    sw.Write(json);
                }
                encrypted = ms.ToArray();
            }
            catch
            {
                Console.WriteLine("Encryption failed. Please check the key and IV.");
                return string.Empty;
            }
            string base64 = Convert.ToBase64String(encrypted);
            string base64UrlSafe = base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
            return base64UrlSafe;
        }

        public string EncryptString(string toEncrypt)
        {
            return EncryptString<string>(toEncrypt);
        }

        public T? Decrypt<T>(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
            {
                return default;
            }

            string padded = encryptedText.Replace('-', '+').Replace('_', '/');
            switch (padded.Length % 4)
            {
                case 2: padded += "=="; break;
                case 3: padded += "="; break;
            }

            byte[] buffer = Convert.FromBase64String(padded);
            string json;
            try
            {
                using Aes aes = Aes.Create();
                aes.Key = Key;
                aes.IV = IV;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using MemoryStream ms = new MemoryStream(buffer);
                using CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using StreamReader sr = new StreamReader(cs);
                json = sr.ReadToEnd();
            }
            catch
            {
                Console.WriteLine("Decryption failed. Please check the key and IV.");
                return default;
            }
            return JsonConvert.DeserializeObject<T>(json)!;
        }

        public string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
            {
                return string.Empty;
            }

            string padded = encryptedText.Replace('-', '+').Replace('_', '/');
            switch (padded.Length % 4)
            {
                case 2: padded += "=="; break;
                case 3: padded += "="; break;
            }

            byte[] buffer = Convert.FromBase64String(padded);
            string json;
            try
            {
                using Aes aes = Aes.Create();
                aes.Key = Key;
                aes.IV = IV;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using MemoryStream ms = new MemoryStream(buffer);
                using CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using StreamReader sr = new StreamReader(cs);
                json = sr.ReadToEnd();
            }
            catch
            {
                Console.WriteLine("Decryption failed. Please check the key and IV.");
                return string.Empty;
            }
            return json;
        }
    }
}
