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
                // ДОБАВЛЯЕМ ::timestamp для преобразования DATE в TIMESTAMP
                var query = @"
                    SELECT 
                        s.SupplyID,
                        s.SupplierID,
                        s.ProductID,
                        s.Quantity,
                        s.UnitPrice,
                        s.TotalPrice,
                        s.SupplyDate::timestamp as SupplyDate,  -- Преобразуем DATE в TIMESTAMP
                        s.CreatedDate::timestamp as CreatedDate, -- Преобразуем DATE в TIMESTAMP
                        sup.Name as SupplierName, 
                        p.Name as ProductName
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
                // ДОБАВЛЯЕМ ::timestamp для преобразования DATE в TIMESTAMP
                var query = @"
                    SELECT 
                        s.SupplyID,
                        s.SupplierID,
                        s.ProductID,
                        s.Quantity,
                        s.UnitPrice,
                        s.TotalPrice,
                        s.SupplyDate::timestamp as SupplyDate,  -- Преобразуем DATE в TIMESTAMP
                        s.CreatedDate::timestamp as CreatedDate, -- Преобразуем DATE в TIMESTAMP
                        sup.Name as SupplierName, 
                        p.Name as ProductName
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

                    // Если SupplyDate содержит время, обрезаем до даты
                    var supplyDate = supply.SupplyDate.Date;
                    command.Parameters.AddWithValue("@SupplyDate", supplyDate);

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

                    // Если SupplyDate содержит время, обрезаем до даты
                    var supplyDate = supply.SupplyDate.Date;
                    command.Parameters.AddWithValue("@SupplyDate", supplyDate);

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

                // Теперь это будет DateTime благодаря ::timestamp
                SupplyDate = reader["SupplyDate"] != DBNull.Value ?
                    Convert.ToDateTime(reader["SupplyDate"]) : DateTime.MinValue,

                // Теперь это будет DateTime благодаря ::timestamp
                CreatedDate = reader["CreatedDate"] != DBNull.Value ?
                    Convert.ToDateTime(reader["CreatedDate"]) : DateTime.MinValue,

                SupplierName = reader["SupplierName"] != DBNull.Value ?
                    reader["SupplierName"].ToString() : "Неизвестно",

                ProductName = reader["ProductName"] != DBNull.Value ?
                    reader["ProductName"].ToString() : "Неизвестно"
            };
        }

        // Дополнительные полезные методы:

        public List<Supply> GetSuppliesBySupplierId(int supplierId)
        {
            var supplies = new List<Supply>();

            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        s.SupplyID,
                        s.SupplierID,
                        s.ProductID,
                        s.Quantity,
                        s.UnitPrice,
                        s.TotalPrice,
                        s.SupplyDate::timestamp as SupplyDate,
                        s.CreatedDate::timestamp as CreatedDate,
                        sup.Name as SupplierName, 
                        p.Name as ProductName
                    FROM Supplies s
                    LEFT JOIN Suppliers sup ON s.SupplierID = sup.SupplierID
                    LEFT JOIN Products p ON s.ProductID = p.ProductID
                    WHERE s.SupplierID = @SupplierID
                    ORDER BY s.SupplyDate DESC";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SupplierID", supplierId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            supplies.Add(MapSupplyFromReader(reader));
                        }
                    }
                }
            }

            return supplies;
        }

        public List<Supply> GetSuppliesByProductId(int productId)
        {
            var supplies = new List<Supply>();

            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        s.SupplyID,
                        s.SupplierID,
                        s.ProductID,
                        s.Quantity,
                        s.UnitPrice,
                        s.TotalPrice,
                        s.SupplyDate::timestamp as SupplyDate,
                        s.CreatedDate::timestamp as CreatedDate,
                        sup.Name as SupplierName, 
                        p.Name as ProductName
                    FROM Supplies s
                    LEFT JOIN Suppliers sup ON s.SupplierID = sup.SupplierID
                    LEFT JOIN Products p ON s.ProductID = p.ProductID
                    WHERE s.ProductID = @ProductID
                    ORDER BY s.SupplyDate DESC";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductID", productId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            supplies.Add(MapSupplyFromReader(reader));
                        }
                    }
                }
            }

            return supplies;
        }

        public decimal GetTotalSuppliesValue(DateTime? startDate = null, DateTime? endDate = null)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = "SELECT COALESCE(SUM(TotalPrice), 0) FROM Supplies WHERE 1=1";

                if (startDate.HasValue)
                {
                    query += " AND SupplyDate >= @StartDate";
                }

                if (endDate.HasValue)
                {
                    query += " AND SupplyDate <= @EndDate";
                }

                using (var command = new NpgsqlCommand(query, connection))
                {
                    if (startDate.HasValue)
                    {
                        command.Parameters.AddWithValue("@StartDate", startDate.Value.Date);
                    }

                    if (endDate.HasValue)
                    {
                        command.Parameters.AddWithValue("@EndDate", endDate.Value.Date);
                    }

                    var result = command.ExecuteScalar();
                    return Convert.ToDecimal(result ?? 0);
                }
            }
        }

        public int GetTotalSuppliedQuantity(int productId)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = "SELECT COALESCE(SUM(Quantity), 0) FROM Supplies WHERE ProductID = @ProductID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductID", productId);
                    var result = command.ExecuteScalar();
                    return Convert.ToInt32(result ?? 0);
                }
            }
        }
    }
}