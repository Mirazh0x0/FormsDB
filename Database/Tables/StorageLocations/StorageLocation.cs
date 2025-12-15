using System;

namespace FormsDB.Tables.StorageLocations
{
    public class StorageLocation
    {
        public int LocationID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? Capacity { get; set; }
        public DateTime CreatedDate { get; set; }

        public override string ToString()
        {
            return $"{Name} (Емкость: {Capacity ?? 0})";
        }
    }
}