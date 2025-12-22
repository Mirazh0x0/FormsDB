using System;
using System.Collections.Generic;
using Npgsql;
using FormsDB.Database.Context;

namespace FormsDB.Tables.StorageLocations
{
    public class StorageLocationRepository
    {
        public List<StorageLocation> GetAllStorageLocations()
        {
            var locations = new List<StorageLocation>();

            using (var connection = DatabaseContext.GetConnection())
            {
                // ДОБАВЛЯЕМ ::timestamp для преобразования DATE в TIMESTAMP
                var query = @"
                    SELECT 
                        LocationID,
                        Name,
                        Description,
                        Capacity,
                        CreatedDate::timestamp as CreatedDate  -- Преобразуем DATE в TIMESTAMP
                    FROM StorageLocations 
                    ORDER BY LocationID";

                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        locations.Add(MapLocationFromReader(reader));
                    }
                }
            }

            return locations;
        }

        public StorageLocation GetLocationById(int locationId)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                // ДОБАВЛЯЕМ ::timestamp для преобразования DATE в TIMESTAMP
                var query = @"
                    SELECT 
                        LocationID,
                        Name,
                        Description,
                        Capacity,
                        CreatedDate::timestamp as CreatedDate  -- Преобразуем DATE в TIMESTAMP
                    FROM StorageLocations 
                    WHERE LocationID = @LocationID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@LocationID", locationId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapLocationFromReader(reader);
                        }
                    }
                }
            }

            return null;
        }

        public int AddStorageLocation(StorageLocation location)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    INSERT INTO StorageLocations (Name, Description, Capacity)
                    VALUES (@Name, @Description, @Capacity)
                    RETURNING LocationID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", location.Name);
                    command.Parameters.AddWithValue("@Description", (object)location.Description ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Capacity", (object)location.Capacity ?? DBNull.Value);

                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public bool UpdateStorageLocation(StorageLocation location)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    UPDATE StorageLocations 
                    SET Name = @Name, 
                        Description = @Description, 
                        Capacity = @Capacity
                    WHERE LocationID = @LocationID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@LocationID", location.LocationID);
                    command.Parameters.AddWithValue("@Name", location.Name);
                    command.Parameters.AddWithValue("@Description", (object)location.Description ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Capacity", (object)location.Capacity ?? DBNull.Value);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool DeleteStorageLocation(int locationId)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = "DELETE FROM StorageLocations WHERE LocationID = @LocationID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@LocationID", locationId);
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        private StorageLocation MapLocationFromReader(NpgsqlDataReader reader)
        {
            return new StorageLocation
            {
                LocationID = Convert.ToInt32(reader["LocationID"]),
                Name = reader["Name"].ToString(),
                Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : null,
                Capacity = reader["Capacity"] != DBNull.Value ? Convert.ToInt32(reader["Capacity"]) : (int?)null,

                // Теперь это будет DateTime благодаря ::timestamp
                CreatedDate = reader["CreatedDate"] != DBNull.Value ?
                    Convert.ToDateTime(reader["CreatedDate"]) : DateTime.MinValue
            };
        }

        public int GetLocationUsage(int locationId)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
            SELECT COALESCE(SUM(QuantityInStock), 0) 
            FROM Products 
            WHERE LocationID = @LocationID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@LocationID", locationId);
                    var result = command.ExecuteScalar();
                    return Convert.ToInt32(result ?? 0);
                }
            }
        }

        public bool IsLocationInUse(int locationId)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
            SELECT EXISTS (
                SELECT 1 
                FROM Products 
                WHERE LocationID = @LocationID 
                AND QuantityInStock > 0
            )";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@LocationID", locationId);
                    return Convert.ToBoolean(command.ExecuteScalar());
                }
            }
        }

        public List<StorageLocation> SearchLocations(string searchTerm)
        {
            var locations = new List<StorageLocation>();

            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        LocationID,
                        Name,
                        Description,
                        Capacity,
                        CreatedDate::timestamp as CreatedDate
                    FROM StorageLocations 
                    WHERE Name ILIKE @SearchTerm 
                       OR Description ILIKE @SearchTerm
                    ORDER BY LocationID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            locations.Add(MapLocationFromReader(reader));
                        }
                    }
                }
            }

            return locations;
        }
    }
}