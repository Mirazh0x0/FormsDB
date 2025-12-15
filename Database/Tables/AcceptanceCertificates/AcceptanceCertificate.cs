using System;

namespace FormsDB.Tables.AcceptanceCertificates
{
    public class AcceptanceCertificate
    {
        public int CertificateID { get; set; }
        public int SupplyID { get; set; }
        public int AcceptedQuantity { get; set; }
        public DateTime AcceptedDate { get; set; }
        public string InspectorName { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedDate { get; set; }

        // Навигационные свойства
        public string SupplierName { get; set; }
        public string ProductName { get; set; }
        public int TotalQuantity { get; set; }

        public override string ToString()
        {
            return $"Акт #{CertificateID} - {ProductName} - {AcceptedQuantity} из {TotalQuantity}";
        }
    }
}