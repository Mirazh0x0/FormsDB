using System;

namespace FormsDB.Tables.Invoices
{
    public class Invoice
    {
        public int InvoiceID { get; set; }
        public int CustomerID { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string Status { get; set; } = "Ожидает оплаты"; // Значение по умолчанию
        public DateTime CreatedDate { get; set; }

        // Навигационные свойства
        public string CustomerName { get; set; }

        public override string ToString()
        {
            return $"Счет #{InvoiceID} - {CustomerName} - {TotalAmount:C}";
        }
    }
}