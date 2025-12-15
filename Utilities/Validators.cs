using System;
using System.Collections.Generic;

namespace FormsDB.Utilities
{
    public static class Validators
    {
        private static readonly HashSet<string> AllowedStatuses = new HashSet<string>
        {
            "Pending", "Paid", "Cancelled", "Active", "Inactive"
        };

        public static ValidationResult ValidateStatus(string status)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(status))
            {
                result.IsValid = false;
                result.Message = "Статус не может быть пустым.";
            }
            else if (!AllowedStatuses.Contains(status))
            {
                result.IsValid = false;
                result.Message = $"Недопустимый статус. Допустимые значения: {string.Join(", ", AllowedStatuses)}";
            }
            else
            {
                result.IsValid = true;
            }

            return result;
        }

        public static ValidationResult ValidatePrice(decimal price)
        {
            var result = new ValidationResult();

            if (price < 0)
            {
                result.IsValid = false;
                result.Message = "Цена не может быть отрицательной.";
            }
            else if (price > 1000000)
            {
                result.IsValid = false;
                result.Message = "Цена слишком велика.";
            }
            else
            {
                result.IsValid = true;
            }

            return result;
        }

        public static ValidationResult ValidateQuantity(int quantity)
        {
            var result = new ValidationResult();

            if (quantity < 0)
            {
                result.IsValid = false;
                result.Message = "Количество не может быть отрицательным.";
            }
            else if (quantity > 10000)
            {
                result.IsValid = false;
                result.Message = "Количество слишком велико.";
            }
            else
            {
                result.IsValid = true;
            }

            return result;
        }

        public static ValidationResult ValidateDateRange(DateTime fromDate, DateTime toDate)
        {
            var result = new ValidationResult();

            if (fromDate > toDate)
            {
                result.IsValid = false;
                result.Message = "Дата начала не может быть позже даты окончания.";
            }
            else if ((toDate - fromDate).TotalDays > 365)
            {
                result.IsValid = false;
                result.Message = "Период не может превышать 1 год.";
            }
            else
            {
                result.IsValid = true;
            }

            return result;
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
    }
}