using System.Security.Cryptography;
using System.Text;

namespace SmartJobSystem.Server.Helpers
{
    public static class SecurityHelper
    {
        // High Security AES-256 Encryption
        // Best practice: The Key and IV should be stored securely (Key Vault, Environment Variables, or Encrypted Config)
        // For this implementation, we will use a key from the configuration.

        public static string Encrypt(string plainText, string key)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;

            // Ensure the key is exactly 32 bytes for AES-256
            byte[] keyBytes = GetKeyBytes(key);
            
            using Aes aes = Aes.Create();
            aes.Key = keyBytes;
            aes.GenerateIV(); // Unique IV for every encryption
            byte[] iv = aes.IV;

            using var encryptor = aes.CreateEncryptor(aes.Key, iv);
            using var ms = new MemoryStream();
            
            // Prepend the IV to the ciphertext so it can be used for decryption
            ms.Write(iv, 0, iv.Length);

            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        public static string Decrypt(string cipherText, string key)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;

            byte[] fullCipher = Convert.FromBase64String(cipherText);
            byte[] keyBytes = GetKeyBytes(key);

            using Aes aes = Aes.Create();
            aes.Key = keyBytes;

            // Extract the IV from the beginning of the ciphertext
            byte[] iv = new byte[aes.BlockSize / 8];
            Array.Copy(fullCipher, 0, iv, 0, iv.Length);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }

        private static byte[] GetKeyBytes(string key)
        {
            // Hash the key to ensure it's always exactly 32 bytes (256 bits)
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
        }
    }
}
