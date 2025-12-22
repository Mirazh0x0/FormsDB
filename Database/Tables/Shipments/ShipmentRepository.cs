using System;
using System.Collections.Generic;
using Npgsql;
using FormsDB.Database.Context;

namespace FormsDB.Tables.Shipments
{
    public class ShipmentRepository
    {
        public List<Shipment> GetAllShipments()
        {
            var shipments = new List<Shipment>();

            using (var connection = DatabaseContext.GetConnection())
            {
                // ДОБАВЛЯЕМ ::timestamp для преобразования DATE в TIMESTAMP
                var query = @"
                    SELECT 
                        s.ShipmentID,
                        s.CustomerID,
                        s.ProductID,
                        s.Quantity,
                        s.UnitPrice,
                        s.TotalPrice,
                        s.ShipmentDate::timestamp as ShipmentDate,  -- Преобразуем DATE в TIMESTAMP
                        s.CreatedDate::timestamp as CreatedDate,    -- Преобразуем DATE в TIMESTAMP
                        c.Name as CustomerName, 
                        p.Name as ProductName
                    FROM Shipments s
                    LEFT JOIN Customers c ON s.CustomerID = c.CustomerID
                    LEFT JOIN Products p ON s.ProductID = p.ProductID
                    ORDER BY s.ShipmentID DESC";

                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        shipments.Add(MapShipmentFromReader(reader));
                    }
                }
            }

            return shipments;
        }

        public Shipment GetShipmentById(int shipmentId)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                // ДОБАВЛЯЕМ ::timestamp для преобразования DATE в TIMESTAMP
                var query = @"
                    SELECT 
                        s.ShipmentID,
                        s.CustomerID,
                        s.ProductID,
                        s.Quantity,
                        s.UnitPrice,
                        s.TotalPrice,
                        s.ShipmentDate::timestamp as ShipmentDate,  -- Преобразуем DATE в TIMESTAMP
                        s.CreatedDate::timestamp as CreatedDate,    -- Преобразуем DATE в TIMESTAMP
                        c.Name as CustomerName, 
                        p.Name as ProductName
                    FROM Shipments s
                    LEFT JOIN Customers c ON s.CustomerID = c.CustomerID
                    LEFT JOIN Products p ON s.ProductID = p.ProductID
                    WHERE s.ShipmentID = @ShipmentID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ShipmentID", shipmentId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapShipmentFromReader(reader);
                        }
                    }
                }
            }

            return null;
        }

        public int AddShipment(Shipment shipment)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    INSERT INTO Shipments (CustomerID, ProductID, Quantity, UnitPrice, TotalPrice, ShipmentDate)
                    VALUES (@CustomerID, @ProductID, @Quantity, @UnitPrice, @TotalPrice, @ShipmentDate)
                    RETURNING ShipmentID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerID", shipment.CustomerID);
                    command.Parameters.AddWithValue("@ProductID", shipment.ProductID);
                    command.Parameters.AddWithValue("@Quantity", shipment.Quantity);
                    command.Parameters.AddWithValue("@UnitPrice", shipment.UnitPrice);
                    command.Parameters.AddWithValue("@TotalPrice", shipment.TotalPrice);

                    // Если ShipmentDate содержит время, обрезаем до даты
                    var shipmentDate = shipment.ShipmentDate.Date;
                    command.Parameters.AddWithValue("@ShipmentDate", shipmentDate);

                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public bool UpdateShipment(Shipment shipment)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    UPDATE Shipments 
                    SET CustomerID = @CustomerID, 
                        ProductID = @ProductID, 
                        Quantity = @Quantity, 
                        UnitPrice = @UnitPrice, 
                        TotalPrice = @TotalPrice, 
                        ShipmentDate = @ShipmentDate
                    WHERE ShipmentID = @ShipmentID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ShipmentID", shipment.ShipmentID);
                    command.Parameters.AddWithValue("@CustomerID", shipment.CustomerID);
                    command.Parameters.AddWithValue("@ProductID", shipment.ProductID);
                    command.Parameters.AddWithValue("@Quantity", shipment.Quantity);
                    command.Parameters.AddWithValue("@UnitPrice", shipment.UnitPrice);
                    command.Parameters.AddWithValue("@TotalPrice", shipment.TotalPrice);

                    // Если ShipmentDate содержит время, обрезаем до даты
                    var shipmentDate = shipment.ShipmentDate.Date;
                    command.Parameters.AddWithValue("@ShipmentDate", shipmentDate);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool DeleteShipment(int shipmentId)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = "DELETE FROM Shipments WHERE ShipmentID = @ShipmentID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ShipmentID", shipmentId);
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        private Shipment MapShipmentFromReader(NpgsqlDataReader reader)
        {
            return new Shipment
            {
                ShipmentID = Convert.ToInt32(reader["ShipmentID"]),
                CustomerID = Convert.ToInt32(reader["CustomerID"]),
                ProductID = Convert.ToInt32(reader["ProductID"]),
                Quantity = Convert.ToInt32(reader["Quantity"]),
                UnitPrice = Convert.ToDecimal(reader["UnitPrice"]),
                TotalPrice = Convert.ToDecimal(reader["TotalPrice"]),

                // Теперь это будет DateTime благодаря ::timestamp
                ShipmentDate = reader["ShipmentDate"] != DBNull.Value ?
                    Convert.ToDateTime(reader["ShipmentDate"]) : DateTime.MinValue,

                // Теперь это будет DateTime благодаря ::timestamp
                CreatedDate = reader["CreatedDate"] != DBNull.Value ?
                    Convert.ToDateTime(reader["CreatedDate"]) : DateTime.MinValue,

                CustomerName = reader["CustomerName"] != DBNull.Value ?
                    reader["CustomerName"].ToString() : "Неизвестно",

                ProductName = reader["ProductName"] != DBNull.Value ?
                    reader["ProductName"].ToString() : "Неизвестно"
            };
        }

        // Дополнительные полезные методы:

        public List<Shipment> GetShipmentsByCustomerId(int customerId)
        {
            var shipments = new List<Shipment>();

            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        s.ShipmentID,
                        s.CustomerID,
                        s.ProductID,
                        s.Quantity,
                        s.UnitPrice,
                        s.TotalPrice,
                        s.ShipmentDate::timestamp as ShipmentDate,
                        s.CreatedDate::timestamp as CreatedDate,
                        c.Name as CustomerName, 
                        p.Name as ProductName
                    FROM Shipments s
                    LEFT JOIN Customers c ON s.CustomerID = c.CustomerID
                    LEFT JOIN Products p ON s.ProductID = p.ProductID
                    WHERE s.CustomerID = @CustomerID
                    ORDER BY s.ShipmentDate DESC";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerID", customerId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            shipments.Add(MapShipmentFromReader(reader));
                        }
                    }
                }
            }

            return shipments;
        }

        public List<Shipment> GetShipmentsByProductId(int productId)
        {
            var shipments = new List<Shipment>();

            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        s.ShipmentID,
                        s.CustomerID,
                        s.ProductID,
                        s.Quantity,
                        s.UnitPrice,
                        s.TotalPrice,
                        s.ShipmentDate::timestamp as ShipmentDate,
                        s.CreatedDate::timestamp as CreatedDate,
                        c.Name as CustomerName, 
                        p.Name as ProductName
                    FROM Shipments s
                    LEFT JOIN Customers c ON s.CustomerID = c.CustomerID
                    LEFT JOIN Products p ON s.ProductID = p.ProductID
                    WHERE s.ProductID = @ProductID
                    ORDER BY s.ShipmentDate DESC";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductID", productId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            shipments.Add(MapShipmentFromReader(reader));
                        }
                    }
                }
            }

            return shipments;
        }

        public List<Shipment> GetShipmentsByDateRange(DateTime startDate, DateTime endDate)
        {
            var shipments = new List<Shipment>();

            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        s.ShipmentID,
                        s.CustomerID,
                        s.ProductID,
                        s.Quantity,
                        s.UnitPrice,
                        s.TotalPrice,
                        s.ShipmentDate::timestamp as ShipmentDate,
                        s.CreatedDate::timestamp as CreatedDate,
                        c.Name as CustomerName, 
                        p.Name as ProductName
                    FROM Shipments s
                    LEFT JOIN Customers c ON s.CustomerID = c.CustomerID
                    LEFT JOIN Products p ON s.ProductID = p.ProductID
                    WHERE s.ShipmentDate BETWEEN @StartDate AND @EndDate
                    ORDER BY s.ShipmentDate DESC";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StartDate", startDate.Date);
                    command.Parameters.AddWithValue("@EndDate", endDate.Date);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            shipments.Add(MapShipmentFromReader(reader));
                        }
                    }
                }
            }

            return shipments;
        }

        public decimal GetTotalShipmentsValue(DateTime? startDate = null, DateTime? endDate = null)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = "SELECT COALESCE(SUM(TotalPrice), 0) FROM Shipments WHERE 1=1";

                if (startDate.HasValue)
                {
                    query += " AND ShipmentDate >= @StartDate";
                }

                if (endDate.HasValue)
                {
                    query += " AND ShipmentDate <= @EndDate";
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

        public int GetTotalShippedQuantity(int productId)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = "SELECT COALESCE(SUM(Quantity), 0) FROM Shipments WHERE ProductID = @ProductID";

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