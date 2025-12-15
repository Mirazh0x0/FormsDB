using System;
using System.Data;
using System.IO;
using System.Text;
using Npgsql;
using FormsDB.Database.Context;

namespace FormsDB.Services
{
    public class ReportService
    {
        public DataTable GenerateSalesReport(DateTime startDate, DateTime endDate)
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
                            s.UnitPrice as 'Цена за ед.',
                            s.TotalPrice as Сумма
                        FROM Shipments s
                        LEFT JOIN Customers c ON s.CustomerID = c.CustomerID
                        LEFT JOIN Products p ON s.ProductID = p.ProductID
                        WHERE s.ShipmentDate BETWEEN @StartDate AND @EndDate
                        ORDER BY s.ShipmentDate DESC";

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
                Console.WriteLine($"Ошибка генерации отчета по продажам: {ex.Message}");
            }

            return dataTable;
        }

        public string ExportReportToHtml(DataTable dataTable, string title)
        {
            var html = new StringBuilder();

            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine($"<title>{title}</title>");
            html.AppendLine("<style>");
            html.AppendLine("table { border-collapse: collapse; width: 100%; }");
            html.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
            html.AppendLine("th { background-color: #4CAF50; color: white; }");
            html.AppendLine("tr:nth-child(even) { background-color: #f2f2f2; }");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine($"<h1>{title}</h1>");
            html.AppendLine($"<p>Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}</p>");

            if (dataTable.Rows.Count > 0)
            {
                html.AppendLine("<table>");

                // Заголовки
                html.AppendLine("<tr>");
                foreach (DataColumn column in dataTable.Columns)
                {
                    html.AppendLine($"<th>{column.ColumnName}</th>");
                }
                html.AppendLine("</tr>");

                // Данные
                foreach (DataRow row in dataTable.Rows)
                {
                    html.AppendLine("<tr>");
                    foreach (var item in row.ItemArray)
                    {
                        html.AppendLine($"<td>{item}</td>");
                    }
                    html.AppendLine("</tr>");
                }

                html.AppendLine("</table>");
            }
            else
            {
                html.AppendLine("<p>Нет данных для отображения</p>");
            }

            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        public bool SaveReportToFile(string content, string filePath)
        {
            try
            {
                File.WriteAllText(filePath, content, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения отчета: {ex.Message}");
                return false;
            }
        }
    }
}