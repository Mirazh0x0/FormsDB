using System;

namespace FormsDB.Tables.Products
{
    public class Product
    {
        public int ProductID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public decimal UnitPrice { get; set; }
        public int QuantityInStock { get; set; }
        public int? LocationID { get; set; }
        public DateTime CreatedDate { get; set; }

        // Навигационные свойства
        public string LocationName { get; set; }

        public override string ToString()
        {
            return $"{Name} (ID: {ProductID}) - {QuantityInStock} шт. - {UnitPrice:C}";
        }
    }
}