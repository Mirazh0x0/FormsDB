using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using FormsDB.Database.Context;

namespace FormsDB.Tables.Products
{
    public class ProductRepository
    {
        public List<Product> GetAllProducts()
        {
            var products = new List<Product>();

            using (var connection = DatabaseContext.GetConnection())
            {
                // ДОБАВЛЯЕМ ::timestamp для преобразования DATE в TIMESTAMP
                var query = @"
                    SELECT 
                        p.ProductID,
                        p.Name,
                        p.Description,
                        p.Category,
                        p.UnitPrice,
                        p.QuantityInStock,
                        p.LocationID,
                        p.CreatedDate::timestamp as CreatedDate,  -- Преобразуем DATE в TIMESTAMP
                        sl.Name as LocationName 
                    FROM Products p
                    LEFT JOIN StorageLocations sl ON p.LocationID = sl.LocationID
                    ORDER BY p.ProductID";

                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(MapProductFromReader(reader));
                    }
                }
            }

            return products;
        }

        public Product GetProductById(int productId)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                // ДОБАВЛЯЕМ ::timestamp для преобразования DATE в TIMESTAMP
                var query = @"
                    SELECT 
                        p.ProductID,
                        p.Name,
                        p.Description,
                        p.Category,
                        p.UnitPrice,
                        p.QuantityInStock,
                        p.LocationID,
                        p.CreatedDate::timestamp as CreatedDate,  -- Преобразуем DATE в TIMESTAMP
                        sl.Name as LocationName 
                    FROM Products p
                    LEFT JOIN StorageLocations sl ON p.LocationID = sl.LocationID
                    WHERE p.ProductID = @ProductID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductID", productId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapProductFromReader(reader);
                        }
                    }
                }
            }

            return null;
        }

        public int AddProduct(Product product)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    INSERT INTO Products (Name, Description, Category, UnitPrice, QuantityInStock, LocationID)
                    VALUES (@Name, @Description, @Category, @UnitPrice, @QuantityInStock, @LocationID)
                    RETURNING ProductID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", product.Name);
                    command.Parameters.AddWithValue("@Description", (object)product.Description ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Category", (object)product.Category ?? DBNull.Value);
                    command.Parameters.AddWithValue("@UnitPrice", product.UnitPrice);
                    command.Parameters.AddWithValue("@QuantityInStock", product.QuantityInStock);
                    command.Parameters.AddWithValue("@LocationID", (object)product.LocationID ?? DBNull.Value);

                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public bool UpdateProduct(Product product)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    UPDATE Products 
                    SET Name = @Name, 
                        Description = @Description, 
                        Category = @Category, 
                        UnitPrice = @UnitPrice, 
                        QuantityInStock = @QuantityInStock, 
                        LocationID = @LocationID
                    WHERE ProductID = @ProductID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductID", product.ProductID);
                    command.Parameters.AddWithValue("@Name", product.Name);
                    command.Parameters.AddWithValue("@Description", (object)product.Description ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Category", (object)product.Category ?? DBNull.Value);
                    command.Parameters.AddWithValue("@UnitPrice", product.UnitPrice);
                    command.Parameters.AddWithValue("@QuantityInStock", product.QuantityInStock);
                    command.Parameters.AddWithValue("@LocationID", (object)product.LocationID ?? DBNull.Value);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool DeleteProduct(int productId)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = "DELETE FROM Products WHERE ProductID = @ProductID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductID", productId);
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public List<Product> SearchProducts(string searchTerm)
        {
            var products = new List<Product>();

            using (var connection = DatabaseContext.GetConnection())
            {
                // ДОБАВЛЯЕМ ::timestamp для преобразования DATE в TIMESTAMP
                var query = @"
                    SELECT 
                        p.ProductID,
                        p.Name,
                        p.Description,
                        p.Category,
                        p.UnitPrice,
                        p.QuantityInStock,
                        p.LocationID,
                        p.CreatedDate::timestamp as CreatedDate,  -- Преобразуем DATE в TIMESTAMP
                        sl.Name as LocationName 
                    FROM Products p
                    LEFT JOIN StorageLocations sl ON p.LocationID = sl.LocationID
                    WHERE p.Name ILIKE @SearchTerm 
                       OR p.Description ILIKE @SearchTerm 
                       OR p.Category ILIKE @SearchTerm
                    ORDER BY p.ProductID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(MapProductFromReader(reader));
                        }
                    }
                }
            }

            return products;
        }

        private Product MapProductFromReader(NpgsqlDataReader reader)
        {
            return new Product
            {
                ProductID = Convert.ToInt32(reader["ProductID"]),
                Name = reader["Name"].ToString(),
                Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : null,
                Category = reader["Category"] != DBNull.Value ? reader["Category"].ToString() : null,
                UnitPrice = Convert.ToDecimal(reader["UnitPrice"]),
                QuantityInStock = Convert.ToInt32(reader["QuantityInStock"]),
                LocationID = reader["LocationID"] != DBNull.Value ? Convert.ToInt32(reader["LocationID"]) : (int?)null,

                // Теперь это будет DateTime благодаря ::timestamp
                CreatedDate = reader["CreatedDate"] != DBNull.Value ?
                    Convert.ToDateTime(reader["CreatedDate"]) : DateTime.MinValue,

                LocationName = reader["LocationName"] != DBNull.Value ? reader["LocationName"].ToString() : null
            };
        }

        // Дополнительные полезные методы:

        public List<Product> GetProductsByCategory(string category)
        {
            var products = new List<Product>();

            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        p.ProductID,
                        p.Name,
                        p.Description,
                        p.Category,
                        p.UnitPrice,
                        p.QuantityInStock,
                        p.LocationID,
                        p.CreatedDate::timestamp as CreatedDate,
                        sl.Name as LocationName 
                    FROM Products p
                    LEFT JOIN StorageLocations sl ON p.LocationID = sl.LocationID
                    WHERE p.Category = @Category
                    ORDER BY p.Name";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Category", category);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(MapProductFromReader(reader));
                        }
                    }
                }
            }

            return products;
        }

        public List<Product> GetProductsByLocation(int locationId)
        {
            var products = new List<Product>();

            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        p.ProductID,
                        p.Name,
                        p.Description,
                        p.Category,
                        p.UnitPrice,
                        p.QuantityInStock,
                        p.LocationID,
                        p.CreatedDate::timestamp as CreatedDate,
                        sl.Name as LocationName 
                    FROM Products p
                    LEFT JOIN StorageLocations sl ON p.LocationID = sl.LocationID
                    WHERE p.LocationID = @LocationID
                    ORDER BY p.Name";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@LocationID", locationId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(MapProductFromReader(reader));
                        }
                    }
                }
            }

            return products;
        }

        public List<Product> GetLowStockProducts(int threshold = 10)
        {
            var products = new List<Product>();

            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        p.ProductID,
                        p.Name,
                        p.Description,
                        p.Category,
                        p.UnitPrice,
                        p.QuantityInStock,
                        p.LocationID,
                        p.CreatedDate::timestamp as CreatedDate,
                        sl.Name as LocationName 
                    FROM Products p
                    LEFT JOIN StorageLocations sl ON p.LocationID = sl.LocationID
                    WHERE p.QuantityInStock <= @Threshold
                    ORDER BY p.QuantityInStock";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Threshold", threshold);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(MapProductFromReader(reader));
                        }
                    }
                }
            }

            return products;
        }

        public bool UpdateProductStock(int productId, int quantityChange)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    UPDATE Products 
                    SET QuantityInStock = QuantityInStock + @QuantityChange
                    WHERE ProductID = @ProductID
                    AND QuantityInStock + @QuantityChange >= 0";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductID", productId);
                    command.Parameters.AddWithValue("@QuantityChange", quantityChange);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public decimal GetTotalInventoryValue()
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = "SELECT COALESCE(SUM(UnitPrice * QuantityInStock), 0) FROM Products";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    var result = command.ExecuteScalar();
                    return Convert.ToDecimal(result ?? 0);
                }
            }
        }
    }
}