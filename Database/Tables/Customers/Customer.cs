using System;

namespace FormsDB.Tables.Customers
{
    public class Customer
    {
        public int CustomerID { get; set; }
        public string Name { get; set; }
        public string ContactPerson { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public DateTime CreatedDate { get; set; }

        public override string ToString()
        {
            return $"{Name} ({ContactPerson}) - {Phone}";
        }
    }
}