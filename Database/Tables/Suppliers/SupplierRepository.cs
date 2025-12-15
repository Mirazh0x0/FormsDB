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
                var query = "SELECT * FROM Suppliers ORDER BY SupplierID";

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
                var query = "SELECT * FROM Suppliers WHERE SupplierID = @SupplierID";

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
                var query = @"
                    SELECT * FROM Suppliers 
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
                CreatedDate = Convert.ToDateTime(reader["CreatedDate"])
            };
        }
    }
}