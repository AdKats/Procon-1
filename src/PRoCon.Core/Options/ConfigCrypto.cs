using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PRoCon.Core.Options
{
    /// <summary>
    /// AES-256 encryption for sensitive config values (RCON passwords, account passwords).
    /// Key is derived from a machine-specific seed stored in the data directory.
    /// </summary>
    public static class ConfigCrypto
    {
        private static byte[] _key;
        private static readonly object _lock = new();
        private const string KeyFileName = ".procon-key";

        /// <summary>
        /// Encrypts a plaintext string. Returns base64-encoded "iv:ciphertext".
        /// </summary>
        public static string Encrypt(string plaintext)
        {
            if (string.IsNullOrEmpty(plaintext)) return "";

            byte[] key = GetOrCreateKey();
            using var aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            byte[] plainBytes = Encoding.UTF8.GetBytes(plaintext);
            byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            return Convert.ToBase64String(aes.IV) + ":" + Convert.ToBase64String(cipherBytes);
        }

        /// <summary>
        /// Decrypts a "iv:ciphertext" base64 string back to plaintext.
        /// Returns the original string if decryption fails (backward compat with unencrypted values).
        /// </summary>
        public static string Decrypt(string encrypted)
        {
            if (string.IsNullOrEmpty(encrypted)) return "";

            // If it doesn't look encrypted (no colon or not base64), return as-is
            int colonIdx = encrypted.IndexOf(':');
            if (colonIdx < 0) return encrypted;

            try
            {
                byte[] iv = Convert.FromBase64String(encrypted.Substring(0, colonIdx));
                byte[] cipherBytes = Convert.FromBase64String(encrypted.Substring(colonIdx + 1));

                byte[] key = GetOrCreateKey();
                using var aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;

                using var decryptor = aes.CreateDecryptor();
                byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch
            {
                // Decryption failed — value is probably plaintext (pre-encryption config)
                return encrypted;
            }
        }

        private static byte[] GetOrCreateKey()
        {
            lock (_lock)
            {
                if (_key != null) return _key;

                string keyPath = Path.Combine(ProConPaths.ConfigsDirectory, KeyFileName);

                if (File.Exists(keyPath))
                {
                    _key = Convert.FromBase64String(File.ReadAllText(keyPath).Trim());
                }
                else
                {
                    _key = new byte[32]; // AES-256
                    RandomNumberGenerator.Fill(_key);

                    Directory.CreateDirectory(Path.GetDirectoryName(keyPath));
                    File.WriteAllText(keyPath, Convert.ToBase64String(_key));

                    // Restrict permissions on Linux/macOS
                    try
                    {
                        if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                            System.Runtime.InteropServices.OSPlatform.Windows))
                        {
                            File.SetUnixFileMode(keyPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
                        }
                    }
                    catch { }
                }

                return _key;
            }
        }
    }
}
