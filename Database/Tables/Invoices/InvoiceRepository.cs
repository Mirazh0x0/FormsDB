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
                    SELECT i.*, c.Name as CustomerName 
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
                    SELECT i.*, c.Name as CustomerName 
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
                    command.Parameters.AddWithValue("@InvoiceDate", invoice.InvoiceDate);
                    command.Parameters.AddWithValue("@DueDate", (object)invoice.DueDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Status", invoice.Status ?? "Pending");

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
                    command.Parameters.AddWithValue("@InvoiceDate", invoice.InvoiceDate);
                    command.Parameters.AddWithValue("@DueDate", (object)invoice.DueDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Status", invoice.Status ?? "Pending");

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
                InvoiceDate = Convert.ToDateTime(reader["InvoiceDate"]),
                DueDate = reader["DueDate"] != DBNull.Value ? Convert.ToDateTime(reader["DueDate"]) : (DateTime?)null,
                Status = reader["Status"].ToString(),
                CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                CustomerName = reader["CustomerName"] != DBNull.Value ? reader["CustomerName"].ToString() : "Неизвестно"
            };
        }
    }
}