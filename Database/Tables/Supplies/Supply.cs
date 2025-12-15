using System;

namespace FormsDB.Tables.Supplies
{
    public class Supply
    {
        public int SupplyID { get; set; }
        public int SupplierID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime SupplyDate { get; set; }
        public DateTime CreatedDate { get; set; }

        // Навигационные свойства
        public string SupplierName { get; set; }
        public string ProductName { get; set; }

        public override string ToString()
        {
            return $"Поставка #{SupplyID} - {ProductName} x{Quantity}";
        }
    }
}