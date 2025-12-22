using System;
using System.Collections.Generic;
using Npgsql;
using FormsDB.Database.Context;

namespace FormsDB.Tables.AcceptanceCertificates
{
    public class AcceptanceCertificateRepository
    {
        public List<AcceptanceCertificate> GetAllCertificates()
        {
            var certificates = new List<AcceptanceCertificate>();

            using (var connection = DatabaseContext.GetConnection())
            {
                // ДОБАВЛЯЕМ ::timestamp для преобразования DATE в TIMESTAMP
                var query = @"
                    SELECT 
                        ac.CertificateID,
                        ac.SupplyID,
                        ac.AcceptedQuantity,
                        ac.AcceptedDate::timestamp as AcceptedDate,  -- Преобразуем DATE в TIMESTAMP
                        ac.InspectorName,
                        ac.Notes,
                        ac.CreatedDate::timestamp as CreatedDate,    -- Преобразуем DATE в TIMESTAMP
                        sp.SupplierID,
                        sup.Name as SupplierName, 
                        p.Name as ProductName, 
                        sp.Quantity as TotalQuantity
                    FROM AcceptanceCertificates ac
                    LEFT JOIN Supplies sp ON ac.SupplyID = sp.SupplyID
                    LEFT JOIN Suppliers sup ON sp.SupplierID = sup.SupplierID
                    LEFT JOIN Products p ON sp.ProductID = p.ProductID
                    ORDER BY ac.CertificateID DESC";

                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        certificates.Add(MapCertificateFromReader(reader));
                    }
                }
            }

            return certificates;
        }

        public AcceptanceCertificate GetCertificateById(int certificateId)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                // ДОБАВЛЯЕМ ::timestamp для преобразования DATE в TIMESTAMP
                var query = @"
                    SELECT 
                        ac.CertificateID,
                        ac.SupplyID,
                        ac.AcceptedQuantity,
                        ac.AcceptedDate::timestamp as AcceptedDate,  -- Преобразуем DATE в TIMESTAMP
                        ac.InspectorName,
                        ac.Notes,
                        ac.CreatedDate::timestamp as CreatedDate,    -- Преобразуем DATE в TIMESTAMP
                        sp.SupplierID,
                        sup.Name as SupplierName, 
                        p.Name as ProductName, 
                        sp.Quantity as TotalQuantity
                    FROM AcceptanceCertificates ac
                    LEFT JOIN Supplies sp ON ac.SupplyID = sp.SupplyID
                    LEFT JOIN Suppliers sup ON sp.SupplierID = sup.SupplierID
                    LEFT JOIN Products p ON sp.ProductID = p.ProductID
                    WHERE ac.CertificateID = @CertificateID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CertificateID", certificateId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapCertificateFromReader(reader);
                        }
                    }
                }
            }

            return null;
        }

        public int AddCertificate(AcceptanceCertificate certificate)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    INSERT INTO AcceptanceCertificates (SupplyID, AcceptedQuantity, AcceptedDate, InspectorName, Notes)
                    VALUES (@SupplyID, @AcceptedQuantity, @AcceptedDate, @InspectorName, @Notes)
                    RETURNING CertificateID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SupplyID", certificate.SupplyID);
                    command.Parameters.AddWithValue("@AcceptedQuantity", certificate.AcceptedQuantity);

                    // Если AcceptedDate содержит время, обрезаем до даты
                    var acceptedDate = certificate.AcceptedDate.Date;
                    command.Parameters.AddWithValue("@AcceptedDate", acceptedDate);

                    command.Parameters.AddWithValue("@InspectorName", (object)certificate.InspectorName ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Notes", (object)certificate.Notes ?? DBNull.Value);

                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public bool UpdateCertificate(AcceptanceCertificate certificate)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    UPDATE AcceptanceCertificates 
                    SET SupplyID = @SupplyID, 
                        AcceptedQuantity = @AcceptedQuantity, 
                        AcceptedDate = @AcceptedDate, 
                        InspectorName = @InspectorName, 
                        Notes = @Notes
                    WHERE CertificateID = @CertificateID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CertificateID", certificate.CertificateID);
                    command.Parameters.AddWithValue("@SupplyID", certificate.SupplyID);
                    command.Parameters.AddWithValue("@AcceptedQuantity", certificate.AcceptedQuantity);

                    // Если AcceptedDate содержит время, обрезаем до даты
                    var acceptedDate = certificate.AcceptedDate.Date;
                    command.Parameters.AddWithValue("@AcceptedDate", acceptedDate);

                    command.Parameters.AddWithValue("@InspectorName", (object)certificate.InspectorName ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Notes", (object)certificate.Notes ?? DBNull.Value);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool DeleteCertificate(int certificateId)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = "DELETE FROM AcceptanceCertificates WHERE CertificateID = @CertificateID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CertificateID", certificateId);
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        private AcceptanceCertificate MapCertificateFromReader(NpgsqlDataReader reader)
        {
            return new AcceptanceCertificate
            {
                CertificateID = Convert.ToInt32(reader["CertificateID"]),
                SupplyID = Convert.ToInt32(reader["SupplyID"]),
                AcceptedQuantity = Convert.ToInt32(reader["AcceptedQuantity"]),

                // Теперь это будет DateTime благодаря ::timestamp
                AcceptedDate = reader["AcceptedDate"] != DBNull.Value ?
                    Convert.ToDateTime(reader["AcceptedDate"]) : DateTime.MinValue,

                InspectorName = reader["InspectorName"] != DBNull.Value ?
                    reader["InspectorName"].ToString() : null,

                Notes = reader["Notes"] != DBNull.Value ?
                    reader["Notes"].ToString() : null,

                // Теперь это будет DateTime благодаря ::timestamp
                CreatedDate = reader["CreatedDate"] != DBNull.Value ?
                    Convert.ToDateTime(reader["CreatedDate"]) : DateTime.MinValue,

                SupplierName = reader["SupplierName"] != DBNull.Value ?
                    reader["SupplierName"].ToString() : "Неизвестно",

                ProductName = reader["ProductName"] != DBNull.Value ?
                    reader["ProductName"].ToString() : "Неизвестно",

                TotalQuantity = reader["TotalQuantity"] != DBNull.Value ?
                    Convert.ToInt32(reader["TotalQuantity"]) : 0
            };
        }

        // Дополнительный метод для получения актов по поставке
        public List<AcceptanceCertificate> GetCertificatesBySupplyId(int supplyId)
        {
            var certificates = new List<AcceptanceCertificate>();

            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        ac.CertificateID,
                        ac.SupplyID,
                        ac.AcceptedQuantity,
                        ac.AcceptedDate::timestamp as AcceptedDate,
                        ac.InspectorName,
                        ac.Notes,
                        ac.CreatedDate::timestamp as CreatedDate,
                        sp.SupplierID,
                        sup.Name as SupplierName, 
                        p.Name as ProductName, 
                        sp.Quantity as TotalQuantity
                    FROM AcceptanceCertificates ac
                    LEFT JOIN Supplies sp ON ac.SupplyID = sp.SupplyID
                    LEFT JOIN Suppliers sup ON sp.SupplierID = sup.SupplierID
                    LEFT JOIN Products p ON sp.ProductID = p.ProductID
                    WHERE ac.SupplyID = @SupplyID
                    ORDER BY ac.CertificateID DESC";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SupplyID", supplyId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            certificates.Add(MapCertificateFromReader(reader));
                        }
                    }
                }
            }

            return certificates;
        }

        // Метод для проверки суммы принятого количества
        public int GetTotalAcceptedQuantity(int supplyId)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = "SELECT COALESCE(SUM(AcceptedQuantity), 0) FROM AcceptanceCertificates WHERE SupplyID = @SupplyID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SupplyID", supplyId);
                    var result = command.ExecuteScalar();
                    return Convert.ToInt32(result ?? 0);
                }
            }
        }
    }
}