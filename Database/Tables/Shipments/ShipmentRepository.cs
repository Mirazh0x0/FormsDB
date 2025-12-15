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
                var query = @"
                    SELECT s.*, c.Name as CustomerName, p.Name as ProductName
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
                var query = @"
                    SELECT s.*, c.Name as CustomerName, p.Name as ProductName
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
                    command.Parameters.AddWithValue("@ShipmentDate", shipment.ShipmentDate);

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
                    command.Parameters.AddWithValue("@ShipmentDate", shipment.ShipmentDate);

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
                ShipmentDate = Convert.ToDateTime(reader["ShipmentDate"]),
                CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                CustomerName = reader["CustomerName"] != DBNull.Value ? reader["CustomerName"].ToString() : "Неизвестно",
                ProductName = reader["ProductName"] != DBNull.Value ? reader["ProductName"].ToString() : "Неизвестно"
            };
        }
    }
}