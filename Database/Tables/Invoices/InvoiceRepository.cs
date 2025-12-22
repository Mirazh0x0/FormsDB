using System;
using System.Collections.Generic;
using Npgsql;
using FormsDB.Database.Context;

namespace FormsDB.Tables.Invoices
{
    public class InvoiceRepository
    {
        public List<Invoice> GetAllInvoices()
        {
            var invoices = new List<Invoice>();

            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        i.InvoiceID,
                        i.CustomerID,
                        i.TotalAmount,
                        i.InvoiceDate::timestamp as InvoiceDate,
                        i.DueDate::timestamp as DueDate,
                        i.Status,
                        i.CreatedDate::timestamp as CreatedDate,
                        c.Name as CustomerName 
                    FROM Invoices i
                    LEFT JOIN Customers c ON i.CustomerID = c.CustomerID
                    ORDER BY i.InvoiceID DESC";

                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        invoices.Add(MapInvoiceFromReader(reader));
                    }
                }
            }

            return invoices;
        }

        public Invoice GetInvoiceById(int invoiceId)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        i.InvoiceID,
                        i.CustomerID,
                        i.TotalAmount,
                        i.InvoiceDate::timestamp as InvoiceDate,
                        i.DueDate::timestamp as DueDate,
                        i.Status,
                        i.CreatedDate::timestamp as CreatedDate,
                        c.Name as CustomerName 
                    FROM Invoices i
                    LEFT JOIN Customers c ON i.CustomerID = c.CustomerID
                    WHERE i.InvoiceID = @InvoiceID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@InvoiceID", invoiceId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapInvoiceFromReader(reader);
                        }
                    }
                }
            }

            return null;
        }

        public int AddInvoice(Invoice invoice)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    INSERT INTO Invoices (CustomerID, TotalAmount, InvoiceDate, DueDate, Status)
                    VALUES (@CustomerID, @TotalAmount, @InvoiceDate, @DueDate, @Status)
                    RETURNING InvoiceID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerID", invoice.CustomerID);
                    command.Parameters.AddWithValue("@TotalAmount", invoice.TotalAmount);

                    var invoiceDate = invoice.InvoiceDate.Date;
                    command.Parameters.AddWithValue("@InvoiceDate", invoiceDate);

                    if (invoice.DueDate.HasValue)
                    {
                        var dueDate = invoice.DueDate.Value.Date;
                        command.Parameters.AddWithValue("@DueDate", dueDate);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@DueDate", DBNull.Value);
                    }

                    command.Parameters.AddWithValue("@Status", invoice.Status ?? "Ожидает оплаты");

                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public bool UpdateInvoice(Invoice invoice)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    UPDATE Invoices 
                    SET CustomerID = @CustomerID, 
                        TotalAmount = @TotalAmount, 
                        InvoiceDate = @InvoiceDate, 
                        DueDate = @DueDate, 
                        Status = @Status
                    WHERE InvoiceID = @InvoiceID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@InvoiceID", invoice.InvoiceID);
                    command.Parameters.AddWithValue("@CustomerID", invoice.CustomerID);
                    command.Parameters.AddWithValue("@TotalAmount", invoice.TotalAmount);

                    var invoiceDate = invoice.InvoiceDate.Date;
                    command.Parameters.AddWithValue("@InvoiceDate", invoiceDate);

                    if (invoice.DueDate.HasValue)
                    {
                        var dueDate = invoice.DueDate.Value.Date;
                        command.Parameters.AddWithValue("@DueDate", dueDate);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@DueDate", DBNull.Value);
                    }

                    command.Parameters.AddWithValue("@Status", invoice.Status ?? "Ожидает оплаты");

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool DeleteInvoice(int invoiceId)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = "DELETE FROM Invoices WHERE InvoiceID = @InvoiceID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@InvoiceID", invoiceId);
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool UpdateInvoiceStatus(int invoiceId, string status)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = "UPDATE Invoices SET Status = @Status WHERE InvoiceID = @InvoiceID";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@InvoiceID", invoiceId);
                    command.Parameters.AddWithValue("@Status", status);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        private Invoice MapInvoiceFromReader(NpgsqlDataReader reader)
        {
            return new Invoice
            {
                InvoiceID = Convert.ToInt32(reader["InvoiceID"]),
                CustomerID = Convert.ToInt32(reader["CustomerID"]),
                TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),

                InvoiceDate = reader["InvoiceDate"] != DBNull.Value ?
                    Convert.ToDateTime(reader["InvoiceDate"]) : DateTime.MinValue,

                DueDate = reader["DueDate"] != DBNull.Value ?
                    (DateTime?)Convert.ToDateTime(reader["DueDate"]) : null,

                Status = reader["Status"].ToString(),

                CreatedDate = reader["CreatedDate"] != DBNull.Value ?
                    Convert.ToDateTime(reader["CreatedDate"]) : DateTime.MinValue,

                CustomerName = reader["CustomerName"] != DBNull.Value ?
                    reader["CustomerName"].ToString() : "Неизвестно"
            };
        }

        public List<Invoice> GetInvoicesByCustomerId(int customerId)
        {
            var invoices = new List<Invoice>();

            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        i.InvoiceID,
                        i.CustomerID,
                        i.TotalAmount,
                        i.InvoiceDate::timestamp as InvoiceDate,
                        i.DueDate::timestamp as DueDate,
                        i.Status,
                        i.CreatedDate::timestamp as CreatedDate,
                        c.Name as CustomerName 
                    FROM Invoices i
                    LEFT JOIN Customers c ON i.CustomerID = c.CustomerID
                    WHERE i.CustomerID = @CustomerID
                    ORDER BY i.InvoiceDate DESC";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerID", customerId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            invoices.Add(MapInvoiceFromReader(reader));
                        }
                    }
                }
            }

            return invoices;
        }

        public List<Invoice> GetInvoicesByDateRange(DateTime startDate, DateTime endDate)
        {
            var invoices = new List<Invoice>();

            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        i.InvoiceID,
                        i.CustomerID,
                        i.TotalAmount,
                        i.InvoiceDate::timestamp as InvoiceDate,
                        i.DueDate::timestamp as DueDate,
                        i.Status,
                        i.CreatedDate::timestamp as CreatedDate,
                        c.Name as CustomerName 
                    FROM Invoices i
                    LEFT JOIN Customers c ON i.CustomerID = c.CustomerID
                    WHERE i.InvoiceDate BETWEEN @StartDate AND @EndDate
                    ORDER BY i.InvoiceDate DESC";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StartDate", startDate.Date);
                    command.Parameters.AddWithValue("@EndDate", endDate.Date);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            invoices.Add(MapInvoiceFromReader(reader));
                        }
                    }
                }
            }

            return invoices;
        }

        public List<Invoice> GetInvoicesByStatus(string status)
        {
            var invoices = new List<Invoice>();

            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        i.InvoiceID,
                        i.CustomerID,
                        i.TotalAmount,
                        i.InvoiceDate::timestamp as InvoiceDate,
                        i.DueDate::timestamp as DueDate,
                        i.Status,
                        i.CreatedDate::timestamp as CreatedDate,
                        c.Name as CustomerName 
                    FROM Invoices i
                    LEFT JOIN Customers c ON i.CustomerID = c.CustomerID
                    WHERE i.Status = @Status
                    ORDER BY i.InvoiceDate DESC";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Status", status);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            invoices.Add(MapInvoiceFromReader(reader));
                        }
                    }
                }
            }

            return invoices;
        }

        public decimal GetTotalInvoicesAmount(DateTime? startDate = null, DateTime? endDate = null)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = "SELECT COALESCE(SUM(TotalAmount), 0) FROM Invoices WHERE 1=1";

                if (startDate.HasValue)
                {
                    query += " AND InvoiceDate >= @StartDate";
                }

                if (endDate.HasValue)
                {
                    query += " AND InvoiceDate <= @EndDate";
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

        public decimal GetPaidInvoicesAmount(DateTime? startDate = null, DateTime? endDate = null)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = "SELECT COALESCE(SUM(TotalAmount), 0) FROM Invoices WHERE Status = 'Оплачен'";

                if (startDate.HasValue)
                {
                    query += " AND InvoiceDate >= @StartDate";
                }

                if (endDate.HasValue)
                {
                    query += " AND InvoiceDate <= @EndDate";
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

        public int GetOverdueInvoicesCount()
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    SELECT COUNT(*) 
                    FROM Invoices 
                    WHERE Status != 'Оплачен' 
                    AND DueDate < CURRENT_DATE
                    AND DueDate IS NOT NULL";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    var result = command.ExecuteScalar();
                    return Convert.ToInt32(result ?? 0);
                }
            }
        }

        public List<Invoice> GetOverdueInvoices()
        {
            var invoices = new List<Invoice>();

            using (var connection = DatabaseContext.GetConnection())
            {
                var query = @"
                    SELECT 
                        i.InvoiceID,
                        i.CustomerID,
                        i.TotalAmount,
                        i.InvoiceDate::timestamp as InvoiceDate,
                        i.DueDate::timestamp as DueDate,
                        i.Status,
                        i.CreatedDate::timestamp as CreatedDate,
                        c.Name as CustomerName 
                    FROM Invoices i
                    LEFT JOIN Customers c ON i.CustomerID = c.CustomerID
                    WHERE i.Status != 'Оплачен' 
                    AND i.DueDate < CURRENT_DATE
                    AND i.DueDate IS NOT NULL
                    ORDER BY i.DueDate";

                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        invoices.Add(MapInvoiceFromReader(reader));
                    }
                }
            }

            return invoices;
        }
    }
}