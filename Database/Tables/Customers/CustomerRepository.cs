using System;
using System.Collections.Generic;
using Npgsql;
using FormsDB.Database.Context;

namespace FormsDB.Tables.Customers
{
    public class CustomerRepository
    {
        public List<Customer> GetAllCustomers()
        {
            var customers = new List<Customer>();

            using (var connection = DatabaseContext.GetConnection())
            {
                var query = "SELECT * FROM Customers ORDER BY CustomerID";

                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        customers.Add(MapCustomerFromReader(reader));
                    }
                }
            }

            return customers;
        }

        public Customer GetCustomerById(int customerId)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = "SELECT * FROM Customers WHERE CustomerID = @CustomerID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerID", customerId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapCustomerFromReader(reader);
                        }
                    }
                }
            }

            return null;
        }

        public int AddCustomer(Customer customer)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    INSERT INTO Customers (Name, ContactPerson, Phone, Email, Address)
                    VALUES (@Name, @ContactPerson, @Phone, @Email, @Address)
                    RETURNING CustomerID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", customer.Name);
                    command.Parameters.AddWithValue("@ContactPerson", (object)customer.ContactPerson ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Phone", (object)customer.Phone ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Email", (object)customer.Email ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Address", (object)customer.Address ?? DBNull.Value);

                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public bool UpdateCustomer(Customer customer)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    UPDATE Customers 
                    SET Name = @Name, 
                        ContactPerson = @ContactPerson, 
                        Phone = @Phone, 
                        Email = @Email, 
                        Address = @Address
                    WHERE CustomerID = @CustomerID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerID", customer.CustomerID);
                    command.Parameters.AddWithValue("@Name", customer.Name);
                    command.Parameters.AddWithValue("@ContactPerson", (object)customer.ContactPerson ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Phone", (object)customer.Phone ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Email", (object)customer.Email ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Address", (object)customer.Address ?? DBNull.Value);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool DeleteCustomer(int customerId)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = "DELETE FROM Customers WHERE CustomerID = @CustomerID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerID", customerId);
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        private Customer MapCustomerFromReader(NpgsqlDataReader reader)
        {
            return new Customer
            {
                CustomerID = Convert.ToInt32(reader["CustomerID"]),
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