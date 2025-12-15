using System;
using System.Data;
using Npgsql;
using FormsDB.Database.Context;

namespace FormsDB.Database.Queries
{
    public static class ReportQueries
    {
        public static DataTable GetMonthlySalesReport(DateTime month)
        {
            var dataTable = new DataTable();

            try
            {
                using (var connection = DatabaseContext.GetConnection())
                {
                    var query = @"
                        SELECT 
                            TO_CHAR(s.ShipmentDate, 'DD.MM.YYYY') as Дата,
                            c.Name as Клиент,
                            p.Name as Товар,
                            s.Quantity as Количество,
                            s.UnitPrice as Цена,
                            s.TotalPrice as Сумма
                        FROM Shipments s
                        LEFT JOIN Customers c ON s.CustomerID = c.CustomerID
                        LEFT JOIN Products p ON s.ProductID = p.ProductID
                        WHERE EXTRACT(MONTH FROM s.ShipmentDate) = @Month 
                          AND EXTRACT(YEAR FROM s.ShipmentDate) = @Year
                        ORDER BY s.ShipmentDate DESC";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Month", month.Month);
                        command.Parameters.AddWithValue("@Year", month.Year);

                        using (var adapter = new NpgsqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения отчета по продажам: {ex.Message}");
            }

            return dataTable;
        }

        public static DataTable GetInventoryReport()
        {
            var dataTable = new DataTable();

            try
            {
                using (var connection = DatabaseContext.GetConnection())
                {
                    var query = @"
                        SELECT 
                            p.Name as Товар,
                            p.QuantityInStock as Количество,
                            p.UnitPrice as Цена,
                            (p.QuantityInStock * p.UnitPrice) as ОбщаяСтоимость,
                            sl.Name as МестоХранения,
                            p.Category as Категория
                        FROM Products p
                        LEFT JOIN StorageLocations sl ON p.LocationID = sl.LocationID
                        WHERE p.QuantityInStock > 0
                        ORDER BY p.Name";

                    using (var command = new NpgsqlCommand(query, connection))
                    using (var adapter = new NpgsqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения отчета по инвентарю: {ex.Message}");
            }

            return dataTable;
        }

        public static DataTable GetSupplierPerformanceReport(DateTime startDate, DateTime endDate)
        {
            var dataTable = new DataTable();

            try
            {
                using (var connection = DatabaseContext.GetConnection())
                {
                    var query = @"
                        SELECT 
                            sup.Name as Поставщик,
                            COUNT(DISTINCT s.SupplyID) as КоличествоПоставок,
                            SUM(s.Quantity) as ОбщееКоличество,
                            SUM(s.TotalPrice) as ОбщаяСумма,
                            AVG(s.UnitPrice) as СредняяЦена
                        FROM Supplies s
                        LEFT JOIN Suppliers sup ON s.SupplierID = sup.SupplierID
                        WHERE s.SupplyDate BETWEEN @StartDate AND @EndDate
                        GROUP BY sup.SupplierID, sup.Name
                        ORDER BY ОбщаяСумма DESC";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StartDate", startDate);
                        command.Parameters.AddWithValue("@EndDate", endDate);

                        using (var adapter = new NpgsqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения отчета по поставщикам: {ex.Message}");
            }

            return dataTable;
        }
    }
}