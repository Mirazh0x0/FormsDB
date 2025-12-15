using System;

namespace FormsDB.Tables.Shipments
{
    public class Shipment
    {
        public int ShipmentID { get; set; }
        public int CustomerID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime ShipmentDate { get; set; }
        public DateTime CreatedDate { get; set; }

        // Навигационные свойства
        public string CustomerName { get; set; }
        public string ProductName { get; set; }

        public override string ToString()
        {
            return $"Отгрузка #{ShipmentID} - {ProductName} x{Quantity}";
        }
    }
}