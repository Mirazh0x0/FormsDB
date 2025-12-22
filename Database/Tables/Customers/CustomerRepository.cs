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
                // ДОБАВЛЯЕМ ::timestamp для преобразования DATE в TIMESTAMP
                var query = @"
                    SELECT 
                        CustomerID,
                        Name,
                        ContactPerson,
                        Phone,
                        Email,
                        Address,
                        CreatedDate::timestamp as CreatedDate  -- Преобразуем DATE в TIMESTAMP
                    FROM Customers 
                    ORDER BY CustomerID";

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
                // ДОБАВЛЯЕМ ::timestamp для преобразования DATE в TIMESTAMP
                var query = @"
                    SELECT 
                        CustomerID,
                        Name,
                        ContactPerson,
                        Phone,
                        Email,
                        Address,
                        CreatedDate::timestamp as CreatedDate  -- Преобразуем DATE в TIMESTAMP
                    FROM Customers 
                    WHERE CustomerID = @CustomerID";

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

                // Теперь это будет DateTime благодаря ::timestamp
                CreatedDate = reader["CreatedDate"] != DBNull.Value ?
                    Convert.ToDateTime(reader["CreatedDate"]) : DateTime.MinValue
            };
        }

        // Дополнительные полезные методы:

        public List<Customer> SearchCustomers(string searchTerm)
        {
            var customers = new List<Customer>();

            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        CustomerID,
                        Name,
                        ContactPerson,
                        Phone,
                        Email,
                        Address,
                        CreatedDate::timestamp as CreatedDate
                    FROM Customers 
                    WHERE Name ILIKE @SearchTerm 
                       OR ContactPerson ILIKE @SearchTerm 
                       OR Email ILIKE @SearchTerm
                       OR Phone ILIKE @SearchTerm
                    ORDER BY Name";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            customers.Add(MapCustomerFromReader(reader));
                        }
                    }
                }
            }

            return customers;
        }

        public List<Customer> GetCustomersWithActiveInvoices()
        {
            var customers = new List<Customer>();

            using (var connection = DatabaseContext.GetConnection())
            {
                // Получаем клиентов с активными (неоплаченными) счетами
                var query = @"
                    SELECT DISTINCT
                        c.CustomerID,
                        c.Name,
                        c.ContactPerson,
                        c.Phone,
                        c.Email,
                        c.Address,
                        c.CreatedDate::timestamp as CreatedDate
                    FROM Customers c
                    INNER JOIN Invoices i ON c.CustomerID = i.CustomerID
                    WHERE i.Status != 'Paid'
                    ORDER BY c.Name";

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

        public List<Customer> GetCustomersByShipmentDate(DateTime startDate, DateTime endDate)
        {
            var customers = new List<Customer>();

            using (var connection = DatabaseContext.GetConnection())
            {
                // Получаем клиентов, у которых были отгрузки за указанный период
                var query = @"
                    SELECT DISTINCT
                        c.CustomerID,
                        c.Name,
                        c.ContactPerson,
                        c.Phone,
                        c.Email,
                        c.Address,
                        c.CreatedDate::timestamp as CreatedDate
                    FROM Customers c
                    INNER JOIN Shipments s ON c.CustomerID = s.CustomerID
                    WHERE s.ShipmentDate BETWEEN @StartDate AND @EndDate
                    ORDER BY c.Name";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StartDate", startDate.Date);
                    command.Parameters.AddWithValue("@EndDate", endDate.Date);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            customers.Add(MapCustomerFromReader(reader));
                        }
                    }
                }
            }

            return customers;
        }

        public int GetCustomerInvoiceCount(int customerId)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = "SELECT COUNT(*) FROM Invoices WHERE CustomerID = @CustomerID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerID", customerId);
                    var result = command.ExecuteScalar();
                    return Convert.ToInt32(result ?? 0);
                }
            }
        }

        public int GetCustomerShipmentCount(int customerId)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = "SELECT COUNT(*) FROM Shipments WHERE CustomerID = @CustomerID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerID", customerId);
                    var result = command.ExecuteScalar();
                    return Convert.ToInt32(result ?? 0);
                }
            }
        }

        public decimal GetCustomerTotalSpent(int customerId)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    SELECT COALESCE(SUM(TotalAmount), 0) 
                    FROM Invoices 
                    WHERE CustomerID = @CustomerID 
                    AND Status = 'Paid'";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerID", customerId);
                    var result = command.ExecuteScalar();
                    return Convert.ToDecimal(result ?? 0);
                }
            }
        }

        public bool CustomerExists(string name, string email)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    SELECT EXISTS (
                        SELECT 1 
                        FROM Customers 
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
    }
}