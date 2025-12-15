using System;
using System.Data;
using System.IO;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using Npgsql;
using FormsDB.Database.Context;
using FormsDB.Services;

namespace FormsDB.ReportGenerators
{
    public class ReportGenerator
    {
        public enum ReportType
        {
            SalesReport,
            InventoryReport,
            SupplierReport,
            CustomerReport,
            FinancialReport,
            MovementReport,
            InvoiceReport,
            AcceptanceReport
        }

        public DataTable GenerateReport(ReportType reportType, DateTime startDate, DateTime endDate, object additionalParams = null)
        {
            switch (reportType)
            {
                case ReportType.SalesReport:
                    return GenerateSalesReport(startDate, endDate);
                case ReportType.InventoryReport:
                    return GenerateInventoryReport();
                case ReportType.SupplierReport:
                    return GenerateSupplierReport(startDate, endDate);
                case ReportType.CustomerReport:
                    return GenerateCustomerReport(startDate, endDate);
                case ReportType.FinancialReport:
                    return GenerateFinancialReport(startDate, endDate);
                case ReportType.MovementReport:
                    return GenerateMovementReport(startDate, endDate, additionalParams);
                case ReportType.InvoiceReport:
                    return GenerateInvoiceReport(startDate, endDate);
                case ReportType.AcceptanceReport:
                    return GenerateAcceptanceReport(startDate, endDate);
                default:
                    throw new ArgumentException("Неизвестный тип отчета");
            }
        }

        private DataTable GenerateSalesReport(DateTime startDate, DateTime endDate)
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
                            s.TotalPrice as Сумма,
                            CASE 
                                WHEN i.Status IS NULL THEN 'Не выставлен'
                                ELSE i.Status
                            END as 'Статус счета'
                        FROM Shipments s
                        LEFT JOIN Customers c ON s.CustomerID = c.CustomerID
                        LEFT JOIN Products p ON s.ProductID = p.ProductID
                        LEFT JOIN Invoices i ON s.CustomerID = i.CustomerID 
                            AND DATE_TRUNC('day', s.ShipmentDate) = DATE_TRUNC('day', i.InvoiceDate)
                        WHERE s.ShipmentDate BETWEEN @StartDate AND @EndDate
                        ORDER BY s.ShipmentDate DESC, c.Name";

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
                throw new Exception($"Ошибка генерации отчета по продажам: {ex.Message}", ex);
            }

            return dataTable;
        }

        private DataTable GenerateInventoryReport()
        {
            var dataTable = new DataTable();

            try
            {
                using (var connection = DatabaseContext.GetConnection())
                {
                    var query = @"
                        SELECT 
                            p.Name as Товар,
                            p.Category as Категория,
                            p.QuantityInStock as 'Количество в наличии',
                            p.UnitPrice as 'Цена за ед.',
                            (p.QuantityInStock * p.UnitPrice) as 'Общая стоимость',
                            sl.Name as 'Место хранения',
                            CASE 
                                WHEN p.QuantityInStock = 0 THEN 'Нет в наличии'
                                WHEN p.QuantityInStock < 10 THEN 'Критически низкий'
                                WHEN p.QuantityInStock < 50 THEN 'Низкий'
                                ELSE 'Нормальный'
                            END as 'Статус запасов'
                        FROM Products p
                        LEFT JOIN StorageLocations sl ON p.LocationID = sl.LocationID
                        WHERE p.QuantityInStock > 0
                        ORDER BY (p.QuantityInStock * p.UnitPrice) DESC";

                    using (var command = new NpgsqlCommand(query, connection))
                    using (var adapter = new NpgsqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка генерации отчета по инвентарю: {ex.Message}", ex);
            }

            return dataTable;
        }

        private DataTable GenerateSupplierReport(DateTime startDate, DateTime endDate)
        {
            var dataTable = new DataTable();

            try
            {
                using (var connection = DatabaseContext.GetConnection())
                {
                    var query = @"
                        SELECT 
                            s.Name as Поставщик,
                            COUNT(DISTINCT sup.SupplyID) as 'Количество поставок',
                            SUM(sup.Quantity) as 'Общее количество',
                            SUM(sup.TotalPrice) as 'Общая сумма',
                            AVG(sup.UnitPrice) as 'Средняя цена',
                            MIN(sup.SupplyDate) as 'Первая поставка',
                            MAX(sup.SupplyDate) as 'Последняя поставка',
                            COUNT(DISTINCT sup.ProductID) as 'Разнообразие товаров'
                        FROM Suppliers s
                        LEFT JOIN Supplies sup ON s.SupplierID = sup.SupplierID
                        WHERE sup.SupplyDate BETWEEN @StartDate AND @EndDate
                        GROUP BY s.SupplierID, s.Name
                        ORDER BY 'Общая сумма' DESC";

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
                throw new Exception($"Ошибка генерации отчета по поставщикам: {ex.Message}", ex);
            }

            return dataTable;
        }

        private DataTable GenerateCustomerReport(DateTime startDate, DateTime endDate)
        {
            var dataTable = new DataTable();

            try
            {
                using (var connection = DatabaseContext.GetConnection())
                {
                    var query = @"
                        SELECT 
                            c.Name as Клиент,
                            c.ContactPerson as 'Контактное лицо',
                            c.Phone as Телефон,
                            c.Email as Email,
                            COUNT(DISTINCT ship.ShipmentID) as 'Количество отгрузок',
                            SUM(ship.TotalPrice) as 'Общая сумма покупок',
                            COUNT(DISTINCT i.InvoiceID) as 'Количество счетов',
                            SUM(CASE WHEN i.Status = 'Pending' THEN i.TotalAmount ELSE 0 END) as 'Текущая задолженность',
                            MAX(ship.ShipmentDate) as 'Последняя покупка',
                            CASE 
                                WHEN MAX(ship.ShipmentDate) < CURRENT_DATE - INTERVAL '90 days' THEN 'Неактивный'
                                WHEN MAX(ship.ShipmentDate) < CURRENT_DATE - INTERVAL '30 days' THEN 'Редко покупает'
                                ELSE 'Активный'
                            END as 'Статус активности'
                        FROM Customers c
                        LEFT JOIN Shipments ship ON c.CustomerID = ship.CustomerID
                        LEFT JOIN Invoices i ON c.CustomerID = i.CustomerID
                        WHERE ship.ShipmentDate BETWEEN @StartDate AND @EndDate
                        GROUP BY c.CustomerID, c.Name, c.ContactPerson, c.Phone, c.Email
                        ORDER BY 'Общая сумма покупок' DESC";

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
                throw new Exception($"Ошибка генерации отчета по клиентам: {ex.Message}", ex);
            }

            return dataTable;
        }

        private DataTable GenerateFinancialReport(DateTime startDate, DateTime endDate)
        {
            var dataTable = new DataTable();

            try
            {
                using (var connection = DatabaseContext.GetConnection())
                {
                    var query = @"
                        SELECT 
                            TO_CHAR(date_range.date, 'MM.YYYY') as Месяц,
                            COALESCE(SUM(ship.TotalPrice), 0) as Выручка,
                            COALESCE(SUM(sup.TotalPrice), 0) as 'Затраты на закупки',
                            COALESCE(SUM(ship.TotalPrice), 0) - COALESCE(SUM(sup.TotalPrice), 0) as 'Валовая прибыль',
                            COALESCE(SUM(i.TotalAmount), 0) as 'Выставлено счетов',
                            COALESCE(SUM(CASE WHEN i.Status = 'Paid' THEN i.TotalAmount ELSE 0 END), 0) as 'Оплаченные счета',
                            COUNT(DISTINCT ship.CustomerID) as 'Количество клиентов',
                            COUNT(DISTINCT sup.SupplierID) as 'Количество поставщиков'
                        FROM (
                            SELECT generate_series(
                                DATE_TRUNC('month', @StartDate), 
                                DATE_TRUNC('month', @EndDate), 
                                '1 month'::interval
                            ) as date
                        ) date_range
                        LEFT JOIN Shipments ship ON DATE_TRUNC('month', ship.ShipmentDate) = date_range.date
                        LEFT JOIN Supplies sup ON DATE_TRUNC('month', sup.SupplyDate) = date_range.date
                        LEFT JOIN Invoices i ON DATE_TRUNC('month', i.InvoiceDate) = date_range.date
                        GROUP BY date_range.date
                        ORDER BY date_range.date";

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
                throw new Exception($"Ошибка генерации финансового отчета: {ex.Message}", ex);
            }

            return dataTable;
        }

        private DataTable GenerateMovementReport(DateTime startDate, DateTime endDate, object additionalParams)
        {
            var dataTable = new DataTable();
            int? productId = additionalParams as int?;

            try
            {
                using (var connection = DatabaseContext.GetConnection())
                {
                    var query = @"
                        SELECT 
                            p.Name as Товар,
                            p.Category as Категория,
                            COALESCE(SUM(CASE WHEN sup.SupplyDate BETWEEN @StartDate AND @EndDate THEN sup.Quantity ELSE 0 END), 0) as Приход,
                            COALESCE(SUM(CASE WHEN ship.ShipmentDate BETWEEN @StartDate AND @EndDate THEN ship.Quantity ELSE 0 END), 0) as Расход,
                            p.QuantityInStock as 'Текущий остаток',
                            COALESCE(SUM(CASE WHEN sup.SupplyDate BETWEEN @StartDate AND @EndDate THEN sup.TotalPrice ELSE 0 END), 0) as 'Сумма прихода',
                            COALESCE(SUM(CASE WHEN ship.ShipmentDate BETWEEN @StartDate AND @EndDate THEN ship.TotalPrice ELSE 0 END), 0) as 'Сумма расхода',
                            (p.QuantityInStock * p.UnitPrice) as 'Стоимость остатка'
                        FROM Products p
                        LEFT JOIN Supplies sup ON p.ProductID = sup.ProductID
                        LEFT JOIN Shipments ship ON p.ProductID = ship.ProductID
                        WHERE (@ProductID IS NULL OR p.ProductID = @ProductID)
                        GROUP BY p.ProductID, p.Name, p.Category, p.QuantityInStock, p.UnitPrice
                        ORDER BY p.Name";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StartDate", startDate);
                        command.Parameters.AddWithValue("@EndDate", endDate);
                        command.Parameters.AddWithValue("@ProductID", (object)productId ?? DBNull.Value);

                        using (var adapter = new NpgsqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка генерации отчета по движению товаров: {ex.Message}", ex);
            }

            return dataTable;
        }

        private DataTable GenerateInvoiceReport(DateTime startDate, DateTime endDate)
        {
            var dataTable = new DataTable();

            try
            {
                using (var connection = DatabaseContext.GetConnection())
                {
                    var query = @"
                        SELECT 
                            c.Name as Клиент,
                            COUNT(i.InvoiceID) as 'Количество счетов',
                            SUM(i.TotalAmount) as 'Общая сумма',
                            SUM(CASE WHEN i.Status = 'Pending' THEN i.TotalAmount ELSE 0 END) as 'Сумма ожидающих',
                            SUM(CASE WHEN i.DueDate < CURRENT_DATE AND i.Status = 'Pending' THEN i.TotalAmount ELSE 0 END) as 'Просроченная сумма',
                            MAX(i.DueDate) as 'Последний срок оплаты'
                        FROM Customers c
                        LEFT JOIN Invoices i ON c.CustomerID = i.CustomerID
                        WHERE i.InvoiceDate BETWEEN @StartDate AND @EndDate
                        GROUP BY c.CustomerID, c.Name
                        HAVING SUM(CASE WHEN i.Status = 'Pending' THEN i.TotalAmount ELSE 0 END) > 0
                        ORDER BY 'Просроченная сумма' DESC, 'Общая сумма' DESC";

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
                throw new Exception($"Ошибка генерации отчета по счетам: {ex.Message}", ex);
            }

            return dataTable;
        }

        private DataTable GenerateAcceptanceReport(DateTime startDate, DateTime endDate)
        {
            var dataTable = new DataTable();

            try
            {
                using (var connection = DatabaseContext.GetConnection())
                {
                    var query = @"
                        SELECT 
                            ac.CertificateID as '№ Акта',
                            TO_CHAR(ac.AcceptedDate, 'DD.MM.YYYY') as 'Дата приемки',
                            s.Name as Поставщик,
                            p.Name as Товар,
                            sup.Quantity as 'Заказанное количество',
                            ac.AcceptedQuantity as 'Принятое количество',
                            (sup.Quantity - ac.AcceptedQuantity) as Разница,
                            ac.InspectorName as Инспектор,
                            CASE 
                                WHEN ac.AcceptedQuantity = sup.Quantity THEN 'Полностью принято'
                                WHEN ac.AcceptedQuantity = 0 THEN 'Не принято'
                                WHEN ac.AcceptedQuantity < sup.Quantity THEN 'Частично принято'
                                ELSE 'Принято с избытком'
                            END as 'Статус приемки',
                            ac.Notes as Примечания
                        FROM AcceptanceCertificates ac
                        LEFT JOIN Supplies sup ON ac.SupplyID = sup.SupplyID
                        LEFT JOIN Suppliers s ON sup.SupplierID = s.SupplierID
                        LEFT JOIN Products p ON sup.ProductID = p.ProductID
                        WHERE ac.AcceptedDate BETWEEN @StartDate AND @EndDate
                        ORDER BY ac.AcceptedDate DESC";

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
                throw new Exception($"Ошибка генерации отчета по актам приемки: {ex.Message}", ex);
            }

            return dataTable;
        }

        public string ExportToHtml(DataTable dataTable, string reportTitle, string reportPeriod = "")
        {
            var html = new StringBuilder();

            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='ru'>");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset='UTF-8'>");
            html.AppendLine($"    <title>{reportTitle}</title>");
            html.AppendLine("    <style>");
            html.AppendLine("        body { font-family: Arial, sans-serif; margin: 20px; }");
            html.AppendLine("        h1 { color: #333; border-bottom: 2px solid #4CAF50; padding-bottom: 10px; }");
            html.AppendLine("        .report-info { margin-bottom: 20px; color: #666; }");
            html.AppendLine("        table { border-collapse: collapse; width: 100%; margin-top: 20px; }");
            html.AppendLine("        th { background-color: #4CAF50; color: white; text-align: left; padding: 12px; }");
            html.AppendLine("        td { border: 1px solid #ddd; padding: 8px; }");
            html.AppendLine("        tr:nth-child(even) { background-color: #f2f2f2; }");
            html.AppendLine("        tr:hover { background-color: #ddd; }");
            html.AppendLine("        .total-row { font-weight: bold; background-color: #e8f5e9; }");
            html.AppendLine("        .footer { margin-top: 30px; text-align: right; font-size: 12px; color: #777; }");
            html.AppendLine("    </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine($"    <h1>{reportTitle}</h1>");

            if (!string.IsNullOrEmpty(reportPeriod))
            {
                html.AppendLine($"    <div class='report-info'>Период: {reportPeriod}</div>");
            }

            html.AppendLine($"    <div class='report-info'>Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}</div>");

            if (dataTable.Rows.Count > 0)
            {
                html.AppendLine("    <table>");
                html.AppendLine("        <thead>");
                html.AppendLine("            <tr>");

                // Заголовки колонок
                foreach (DataColumn column in dataTable.Columns)
                {
                    html.AppendLine($"            <th>{column.ColumnName}</th>");
                }

                html.AppendLine("            </tr>");
                html.AppendLine("        </thead>");
                html.AppendLine("        <tbody>");

                // Данные
                foreach (DataRow row in dataTable.Rows)
                {
                    html.AppendLine("            <tr>");

                    foreach (var item in row.ItemArray)
                    {
                        string cellValue = item?.ToString() ?? "";

                        // Форматирование для числовых значений
                        if (item is decimal decimalValue)
                        {
                            cellValue = decimalValue.ToString("C2");
                        }
                        else if (item is DateTime dateValue)
                        {
                            cellValue = dateValue.ToString("dd.MM.yyyy");
                        }

                        html.AppendLine($"            <td>{cellValue}</td>");
                    }

                    html.AppendLine("            </tr>");
                }

                html.AppendLine("        </tbody>");
                html.AppendLine("    </table>");

                // Итоговая строка (если есть числовые колонки)
                bool hasNumericColumns = false;
                foreach (DataColumn column in dataTable.Columns)
                {
                    if (column.DataType == typeof(decimal) || column.DataType == typeof(int))
                    {
                        hasNumericColumns = true;
                        break;
                    }
                }

                if (hasNumericColumns)
                {
                    html.AppendLine("    <table>");
                    html.AppendLine("        <tr class='total-row'>");
                    html.AppendLine("            <td colspan='" + (dataTable.Columns.Count - 1) + "'><strong>ИТОГО:</strong></td>");

                    // Расчет итогов для каждой числовой колонки
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        if (column.DataType == typeof(decimal))
                        {
                            decimal total = 0;
                            foreach (DataRow row in dataTable.Rows)
                            {
                                if (row[column] != DBNull.Value)
                                {
                                    total += Convert.ToDecimal(row[column]);
                                }
                            }
                            html.AppendLine($"            <td><strong>{total:C2}</strong></td>");
                        }
                        else if (column.DataType == typeof(int))
                        {
                            int total = 0;
                            foreach (DataRow row in dataTable.Rows)
                            {
                                if (row[column] != DBNull.Value)
                                {
                                    total += Convert.ToInt32(row[column]);
                                }
                            }
                            html.AppendLine($"            <td><strong>{total}</strong></td>");
                        }
                    }

                    html.AppendLine("        </tr>");
                    html.AppendLine("    </table>");
                }
            }
            else
            {
                html.AppendLine("    <p>Нет данных для отображения</p>");
            }

            html.AppendLine("    <div class='footer'>");
            html.AppendLine("        Сформировано системой управления складом");
            html.AppendLine("    </div>");
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
                throw new Exception($"Ошибка сохранения отчета: {ex.Message}", ex);
            }
        }

        public void PrintReport(DataTable dataTable, string reportTitle)
        {
            try
            {
                using (var printDialog = new PrintDialog())
                {
                    if (printDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Создаем документ для печати
                        var printDocument = new System.Drawing.Printing.PrintDocument();
                        printDocument.DocumentName = reportTitle;

                        var printPreview = new PrintPreviewDialog();
                        printPreview.Document = printDocument;

                        printDocument.PrintPage += (sender, e) =>
                        {
                            PrintReportPage(e, dataTable, reportTitle);
                        };

                        printPreview.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при печати: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintReportPage(System.Drawing.Printing.PrintPageEventArgs e, DataTable dataTable, string reportTitle)
        {
            var graphics = e.Graphics;
            var font = new Font("Arial", 10);
            var boldFont = new Font("Arial", 12, FontStyle.Bold);
            var smallFont = new Font("Arial", 8);

            float yPos = 50;
            float leftMargin = e.MarginBounds.Left;
            float topMargin = e.MarginBounds.Top;

            // Заголовок отчета
            graphics.DrawString(reportTitle, boldFont, Brushes.Black, leftMargin, yPos);
            yPos += 30;

            // Дата формирования
            graphics.DrawString($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}", font, Brushes.Black, leftMargin, yPos);
            yPos += 20;

            if (dataTable.Rows.Count > 0)
            {
                // Заголовки таблицы
                float xPos = leftMargin;
                float columnWidth = e.MarginBounds.Width / dataTable.Columns.Count;

                foreach (DataColumn column in dataTable.Columns)
                {
                    graphics.DrawString(column.ColumnName, font, Brushes.Black, xPos, yPos);
                    xPos += columnWidth;
                }

                yPos += 20;
                graphics.DrawLine(Pens.Black, leftMargin, yPos, leftMargin + e.MarginBounds.Width, yPos);
                yPos += 5;

                // Данные таблицы
                foreach (DataRow row in dataTable.Rows)
                {
                    if (yPos > e.MarginBounds.Bottom - 50)
                    {
                        e.HasMorePages = true;
                        return;
                    }

                    xPos = leftMargin;

                    foreach (var item in row.ItemArray)
                    {
                        string cellValue = item?.ToString() ?? "";

                        if (item is decimal decimalValue)
                        {
                            cellValue = decimalValue.ToString("C2");
                        }
                        else if (item is DateTime dateValue)
                        {
                            cellValue = dateValue.ToString("dd.MM.yyyy");
                        }

                        graphics.DrawString(cellValue, font, Brushes.Black, xPos, yPos);
                        xPos += columnWidth;
                    }

                    yPos += 20;
                }
            }
            else
            {
                graphics.DrawString("Нет данных для отображения", font, Brushes.Black, leftMargin, yPos);
            }

            // Подпись
            yPos = e.MarginBounds.Bottom - 30;
            graphics.DrawString("Сформировано системой управления складом", smallFont, Brushes.Black, leftMargin, yPos);
        }

        public void ShowReportPreview(DataTable dataTable, string reportTitle)
        {
            try
            {
                var form = new Form
                {
                    Text = $"Просмотр отчета: {reportTitle}",
                    Size = new Size(1000, 600),
                    StartPosition = FormStartPosition.CenterScreen
                };

                var dataGridView = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    DataSource = dataTable,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    ReadOnly = true,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
                };

                // Панель с кнопками
                var panel = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 50,
                    Padding = new Padding(10)
                };

                var btnExport = new Button { Text = "Экспорт", Size = new Size(100, 30), Location = new Point(10, 10) };
                var btnPrint = new Button { Text = "Печать", Size = new Size(100, 30), Location = new Point(120, 10) };
                var btnClose = new Button { Text = "Закрыть", Size = new Size(100, 30), Location = new Point(230, 10) };

                btnExport.Click += (s, e) =>
                {
                    using (var saveDialog = new SaveFileDialog())
                    {
                        saveDialog.Filter = "HTML файлы (*.html)|*.html|CSV файлы (*.csv)|*.csv|Текстовые файлы (*.txt)|*.txt";
                        saveDialog.FilterIndex = 1;
                        saveDialog.FileName = $"{reportTitle}_{DateTime.Now:yyyyMMdd}";

                        if (saveDialog.ShowDialog() == DialogResult.OK)
                        {
                            try
                            {
                                if (saveDialog.FilterIndex == 1) // HTML
                                {
                                    string htmlContent = ExportToHtml(dataTable, reportTitle);
                                    SaveReportToFile(htmlContent, saveDialog.FileName);
                                }
                                else if (saveDialog.FilterIndex == 2) // CSV
                                {
                                    ExportService.ExportToCsv(dataGridView, saveDialog.FileName);
                                }
                                else // TXT
                                {
                                    ExportService.ExportToText(dataGridView, saveDialog.FileName);
                                }

                                MessageBox.Show("Отчет успешно экспортирован", "Успех",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                };

                btnPrint.Click += (s, e) => PrintReport(dataTable, reportTitle);
                btnClose.Click += (s, e) => form.Close();

                panel.Controls.AddRange(new Control[] { btnExport, btnPrint, btnClose });

                form.Controls.AddRange(new Control[] { dataGridView, panel });
                form.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при просмотре отчета: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}