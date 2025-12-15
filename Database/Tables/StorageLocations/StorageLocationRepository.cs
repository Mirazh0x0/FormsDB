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
                var query = "SELECT * FROM StorageLocations ORDER BY LocationID";

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
                var query = "SELECT * FROM StorageLocations WHERE LocationID = @LocationID";

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
                CreatedDate = Convert.ToDateTime(reader["CreatedDate"])
            };
        }
    }
}