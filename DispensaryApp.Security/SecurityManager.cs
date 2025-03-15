using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace DispensaryApp.Security
{
    public class SecurityManager
    {
        private static readonly string Key = "YourSecretKey123!@#"; // В реальном приложении хранить в безопасном месте

        // 1. Шифрование данных
        public static string EncryptData(string plainText)
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(Key.PadRight(32).Substring(0, 32));
                    aes.IV = new byte[16];

                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }

                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при шифровании данных: " + ex.Message);
            }
        }

        // 2. Дешифрование данных
        public static string DecryptData(string cipherText)
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(Key.PadRight(32).Substring(0, 32));
                    aes.IV = new byte[16];

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при дешифровании данных: " + ex.Message);
            }
        }

        // 3. Хеширование паролей
        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        // 4. Валидация входных данных
        public static bool ValidateInput(string input, string pattern)
        {
            try
            {
                return System.Text.RegularExpressions.Regex.IsMatch(input, pattern);
            }
            catch
            {
                return false;
            }
        }

        // 5. Защита от SQL-инъекций
        public static string SanitizeSqlInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return input.Replace("'", "''")
                       .Replace("--", "")
                       .Replace(";", "")
                       .Replace("/*", "")
                       .Replace("*/", "");
        }

        // 6. Проверка сложности пароля
        public static bool IsPasswordStrong(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            bool hasMinLength = password.Length >= 8;
            bool hasUpperCase = System.Text.RegularExpressions.Regex.IsMatch(password, "[A-Z]");
            bool hasLowerCase = System.Text.RegularExpressions.Regex.IsMatch(password, "[a-z]");
            bool hasDigit = System.Text.RegularExpressions.Regex.IsMatch(password, "[0-9]");
            bool hasSpecialChar = System.Text.RegularExpressions.Regex.IsMatch(password, "[^a-zA-Z0-9]");

            return hasMinLength && hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar;
        }

        // 7. Генерация безопасного токена
        public static string GenerateSecureToken()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var tokenBytes = new byte[32];
                rng.GetBytes(tokenBytes);
                return Convert.ToBase64String(tokenBytes);
            }
        }
    }
} 