using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using FormsDB.Database.Context;

namespace FormsDB.Services
{
    public static class SearchService
    {
        public static DataTable SearchInAllTables(string searchTerm)
        {
            var result = new DataTable();
            result.Columns.Add("Таблица", typeof(string));
            result.Columns.Add("ID", typeof(int));
            result.Columns.Add("Название", typeof(string));
            result.Columns.Add("Описание", typeof(string));

            try
            {
                using (var connection = DatabaseContext.GetConnection())
                {
                    // Поиск в таблице Products
                    var productQuery = @"
                        SELECT 'Продукты' as TableName, ProductID as ID, Name, Description 
                        FROM Products 
                        WHERE Name ILIKE @SearchTerm OR Description ILIKE @SearchTerm";

                    using (var command = new NpgsqlCommand(productQuery, connection))
                    {
                        command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
                        AddToDataTable(command, result);
                    }

                    // Поиск в таблице Customers
                    var customerQuery = @"
                        SELECT 'Клиенты' as TableName, CustomerID as ID, Name, ContactPerson as Description 
                        FROM Customers 
                        WHERE Name ILIKE @SearchTerm OR ContactPerson ILIKE @SearchTerm";

                    using (var command = new NpgsqlCommand(customerQuery, connection))
                    {
                        command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
                        AddToDataTable(command, result);
                    }

                    // Поиск в таблице Suppliers
                    var supplierQuery = @"
                        SELECT 'Поставщики' as TableName, SupplierID as ID, Name, ContactPerson as Description 
                        FROM Suppliers 
                        WHERE Name ILIKE @SearchTerm OR ContactPerson ILIKE @SearchTerm";

                    using (var command = new NpgsqlCommand(supplierQuery, connection))
                    {
                        command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
                        AddToDataTable(command, result);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка поиска: {ex.Message}");
            }

            return result;
        }

        private static void AddToDataTable(NpgsqlCommand command, DataTable dataTable)
        {
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    dataTable.Rows.Add(
                        reader["TableName"],
                        reader["ID"],
                        reader["Name"],
                        reader["Description"]
                    );
                }
            }
        }
    }
}