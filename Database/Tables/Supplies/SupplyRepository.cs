using System;
using System.Collections.Generic;
using Npgsql;
using FormsDB.Database.Context;

namespace FormsDB.Tables.Supplies
{
    public class SupplyRepository
    {
        public List<Supply> GetAllSupplies()
        {
            var supplies = new List<Supply>();

            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    SELECT s.*, sup.Name as SupplierName, p.Name as ProductName
                    FROM Supplies s
                    LEFT JOIN Suppliers sup ON s.SupplierID = sup.SupplierID
                    LEFT JOIN Products p ON s.ProductID = p.ProductID
                    ORDER BY s.SupplyID DESC";

                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        supplies.Add(MapSupplyFromReader(reader));
                    }
                }
            }

            return supplies;
        }

        public Supply GetSupplyById(int supplyId)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    SELECT s.*, sup.Name as SupplierName, p.Name as ProductName
                    FROM Supplies s
                    LEFT JOIN Suppliers sup ON s.SupplierID = sup.SupplierID
                    LEFT JOIN Products p ON s.ProductID = p.ProductID
                    WHERE s.SupplyID = @SupplyID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SupplyID", supplyId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapSupplyFromReader(reader);
                        }
                    }
                }
            }

            return null;
        }

        public int AddSupply(Supply supply)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    INSERT INTO Supplies (SupplierID, ProductID, Quantity, UnitPrice, TotalPrice, SupplyDate)
                    VALUES (@SupplierID, @ProductID, @Quantity, @UnitPrice, @TotalPrice, @SupplyDate)
                    RETURNING SupplyID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SupplierID", supply.SupplierID);
                    command.Parameters.AddWithValue("@ProductID", supply.ProductID);
                    command.Parameters.AddWithValue("@Quantity", supply.Quantity);
                    command.Parameters.AddWithValue("@UnitPrice", supply.UnitPrice);
                    command.Parameters.AddWithValue("@TotalPrice", supply.TotalPrice);
                    command.Parameters.AddWithValue("@SupplyDate", supply.SupplyDate);

                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public bool UpdateSupply(Supply supply)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    UPDATE Supplies 
                    SET SupplierID = @SupplierID, 
                        ProductID = @ProductID, 
                        Quantity = @Quantity, 
                        UnitPrice = @UnitPrice, 
                        TotalPrice = @TotalPrice, 
                        SupplyDate = @SupplyDate
                    WHERE SupplyID = @SupplyID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SupplyID", supply.SupplyID);
                    command.Parameters.AddWithValue("@SupplierID", supply.SupplierID);
                    command.Parameters.AddWithValue("@ProductID", supply.ProductID);
                    command.Parameters.AddWithValue("@Quantity", supply.Quantity);
                    command.Parameters.AddWithValue("@UnitPrice", supply.UnitPrice);
                    command.Parameters.AddWithValue("@TotalPrice", supply.TotalPrice);
                    command.Parameters.AddWithValue("@SupplyDate", supply.SupplyDate);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool DeleteSupply(int supplyId)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = "DELETE FROM Supplies WHERE SupplyID = @SupplyID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SupplyID", supplyId);
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        private Supply MapSupplyFromReader(NpgsqlDataReader reader)
        {
            return new Supply
            {
                SupplyID = Convert.ToInt32(reader["SupplyID"]),
                SupplierID = Convert.ToInt32(reader["SupplierID"]),
                ProductID = Convert.ToInt32(reader["ProductID"]),
                Quantity = Convert.ToInt32(reader["Quantity"]),
                UnitPrice = Convert.ToDecimal(reader["UnitPrice"]),
                TotalPrice = Convert.ToDecimal(reader["TotalPrice"]),
                SupplyDate = Convert.ToDateTime(reader["SupplyDate"]),
                CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                SupplierName = reader["SupplierName"] != DBNull.Value ? reader["SupplierName"].ToString() : "Неизвестно",
                ProductName = reader["ProductName"] != DBNull.Value ? reader["ProductName"].ToString() : "Неизвестно"
            };
        }
    }
}