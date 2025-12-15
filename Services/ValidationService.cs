using System;
using System.Text.RegularExpressions;

namespace FormsDB.Services
{
    public static class ValidationService
    {
        public static bool ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return true; // Email не обязателен

            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        public static bool ValidatePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return true; // Телефон не обязателен

            // Простая валидация телефона (разрешены цифры, пробелы, скобки, плюс, дефис)
            var regex = new Regex(@"^[\+\d\s\-\(\)]+$");
            return regex.IsMatch(phone);
        }

        public static bool ValidateDecimal(string value, out decimal result)
        {
            return decimal.TryParse(value, out result) && result >= 0;
        }

        public static bool ValidateInteger(string value, out int result)
        {
            return int.TryParse(value, out result) && result >= 0;
        }

        public static bool ValidateRequired(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        public static bool ValidateDate(string value, out DateTime result)
        {
            return DateTime.TryParse(value, out result);
        }
    }
}