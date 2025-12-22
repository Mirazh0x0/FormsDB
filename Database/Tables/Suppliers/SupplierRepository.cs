using System;
using System.Collections.Generic;
using Npgsql;
using FormsDB.Database.Context;

namespace FormsDB.Tables.Suppliers
{
    public class SupplierRepository
    {
        public List<Supplier> GetAllSuppliers()
        {
            var suppliers = new List<Supplier>();

            using (var connection = DatabaseContext.GetConnection())
            {
                // ДОБАВЛЯЕМ ::timestamp для преобразования DATE в TIMESTAMP
                var query = @"
                    SELECT 
                        SupplierID,
                        Name,
                        ContactPerson,
                        Phone,
                        Email,
                        Address,
                        CreatedDate::timestamp as CreatedDate  -- Преобразуем DATE в TIMESTAMP
                    FROM Suppliers 
                    ORDER BY SupplierID";

                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        suppliers.Add(MapSupplierFromReader(reader));
                    }
                }
            }

            return suppliers;
        }

        public Supplier GetSupplierById(int supplierId)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                // ДОБАВЛЯЕМ ::timestamp для преобразования DATE в TIMESTAMP
                var query = @"
                    SELECT 
                        SupplierID,
                        Name,
                        ContactPerson,
                        Phone,
                        Email,
                        Address,
                        CreatedDate::timestamp as CreatedDate  -- Преобразуем DATE в TIMESTAMP
                    FROM Suppliers 
                    WHERE SupplierID = @SupplierID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SupplierID", supplierId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapSupplierFromReader(reader);
                        }
                    }
                }
            }

            return null;
        }

        public int AddSupplier(Supplier supplier)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    INSERT INTO Suppliers (Name, ContactPerson, Phone, Email, Address)
                    VALUES (@Name, @ContactPerson, @Phone, @Email, @Address)
                    RETURNING SupplierID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", supplier.Name);
                    command.Parameters.AddWithValue("@ContactPerson", (object)supplier.ContactPerson ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Phone", (object)supplier.Phone ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Email", (object)supplier.Email ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Address", (object)supplier.Address ?? DBNull.Value);

                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public bool UpdateSupplier(Supplier supplier)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    UPDATE Suppliers 
                    SET Name = @Name, 
                        ContactPerson = @ContactPerson, 
                        Phone = @Phone, 
                        Email = @Email, 
                        Address = @Address
                    WHERE SupplierID = @SupplierID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SupplierID", supplier.SupplierID);
                    command.Parameters.AddWithValue("@Name", supplier.Name);
                    command.Parameters.AddWithValue("@ContactPerson", (object)supplier.ContactPerson ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Phone", (object)supplier.Phone ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Email", (object)supplier.Email ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Address", (object)supplier.Address ?? DBNull.Value);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool DeleteSupplier(int supplierId)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = "DELETE FROM Suppliers WHERE SupplierID = @SupplierID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SupplierID", supplierId);
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public List<Supplier> SearchSuppliers(string searchTerm)
        {
            var suppliers = new List<Supplier>();

            using (var connection = DatabaseContext.GetConnection())
            {
                // ДОБАВЛЯЕМ ::timestamp для преобразования DATE в TIMESTAMP
                var query = @"
                    SELECT 
                        SupplierID,
                        Name,
                        ContactPerson,
                        Phone,
                        Email,
                        Address,
                        CreatedDate::timestamp as CreatedDate  -- Преобразуем DATE в TIMESTAMP
                    FROM Suppliers 
                    WHERE Name ILIKE @SearchTerm 
                       OR ContactPerson ILIKE @SearchTerm 
                       OR Email ILIKE @SearchTerm
                    ORDER BY SupplierID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            suppliers.Add(MapSupplierFromReader(reader));
                        }
                    }
                }
            }

            return suppliers;
        }

        private Supplier MapSupplierFromReader(NpgsqlDataReader reader)
        {
            return new Supplier
            {
                SupplierID = Convert.ToInt32(reader["SupplierID"]),
                Name = reader["Name"].ToString(),
                ContactPerson = reader["ContactPerson"] != DBNull.Value ? reader["ContactPerson"].ToString() : null,
                Phone = reader["Phone"] != DBNull.Value ? reader["Phone"].ToString() : null,
                Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : null,
                Address = reader["Address"] != DBNull.Value ? reader["Address"].ToString() : null,

                // Теперь это будет DateTime благодаря ::timestamp
                CreatedDate = reader["CreatedDate"] != DBNull.Value ?
                    Convert.ToDateTime(reader["CreatedDate"]) : DateTime.MinValue
            };
        }

        // Дополнительные полезные методы:

        public List<Supplier> GetSuppliersWithActiveSupplies()
        {
            var suppliers = new List<Supplier>();

            using (var connection = DatabaseContext.GetConnection())
            {
                // Получаем поставщиков, у которых есть активные поставки
                var query = @"
                    SELECT DISTINCT
                        s.SupplierID,
                        s.Name,
                        s.ContactPerson,
                        s.Phone,
                        s.Email,
                        s.Address,
                        s.CreatedDate::timestamp as CreatedDate
                    FROM Suppliers s
                    INNER JOIN Supplies sp ON s.SupplierID = sp.SupplierID
                    WHERE sp.SupplyDate >= CURRENT_DATE - INTERVAL '30 days'
                    ORDER BY s.Name";

                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        suppliers.Add(MapSupplierFromReader(reader));
                    }
                }
            }

            return suppliers;
        }

        public int GetSupplierSupplyCount(int supplierId)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = "SELECT COUNT(*) FROM Supplies WHERE SupplierID = @SupplierID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SupplierID", supplierId);
                    var result = command.ExecuteScalar();
                    return Convert.ToInt32(result ?? 0);
                }
            }
        }

        public decimal GetTotalSupplierValue(int supplierId)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = "SELECT COALESCE(SUM(TotalPrice), 0) FROM Supplies WHERE SupplierID = @SupplierID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SupplierID", supplierId);
                    var result = command.ExecuteScalar();
                    return Convert.ToDecimal(result ?? 0);
                }
            }
        }

        public bool SupplierExists(string name, string email)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    SELECT EXISTS (
                        SELECT 1 
                        FROM Suppliers 
                        WHERE Name = @Name 
                        OR (Email = @Email AND Email IS NOT NULL AND Email != '')
                    )";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@Email", email ?? string.Empty);

                    return Convert.ToBoolean(command.ExecuteScalar());
                }
            }
        }

        public List<Supplier> GetSuppliersByProductCategory(string category)
        {
            var suppliers = new List<Supplier>();

            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    SELECT DISTINCT
                        s.SupplierID,
                        s.Name,
                        s.ContactPerson,
                        s.Phone,
                        s.Email,
                        s.Address,
                        s.CreatedDate::timestamp as CreatedDate
                    FROM Suppliers s
                    INNER JOIN Supplies sp ON s.SupplierID = sp.SupplierID
                    INNER JOIN Products p ON sp.ProductID = p.ProductID
                    WHERE p.Category = @Category
                    ORDER BY s.Name";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Category", category);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            suppliers.Add(MapSupplierFromReader(reader));
                        }
                    }
                }
            }

            return suppliers;
        }
    }
}