using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Npgsql;
using FormsDB.Database.Context;
using FormsDB.Services;
using FormsDB.Utilities;

namespace FormsDB.Forms
{
    public partial class Report5Form : Form
    {
        public Report5Form()
        {
            InitializeComponent();
            LoadReportData();
        }

        private void InitializeComponent()
        {
            this.Text = "Отчет 5: Движение товаров";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Панель управления с TableLayoutPanel для точного расположения
            var controlPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                Padding = new Padding(10)
            };

            // Используем TableLayoutPanel для точного позиционирования
            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 2,
                Padding = new Padding(0, 5, 0, 5)
            };

            // Настройка столбцов
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30));  // Метка "С:"
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120)); // dtpFromDate
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30));  // Метка "По:"
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120)); // dtpToDate
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50));  // Метка "Товар:"
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));  // cmbProduct

            // Настройка строк
            tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Первая строка: даты и кнопки
            // Поля дат в первой строке
            var lblFromDate = new Label
            {
                Text = "С:",
                TextAlign = ContentAlignment.MiddleLeft,
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };

            dtpFromDate = new DateTimePicker
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Value = DateTime.Now.AddMonths(-1)
            };

            var lblToDate = new Label
            {
                Text = "По:",
                TextAlign = ContentAlignment.MiddleLeft,
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };

            dtpToDate = new DateTimePicker
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Value = DateTime.Now
            };

            // Кнопки в первой строке (занимают несколько столбцов)
            var btnGenerate = new Button
            {
                Text = "Сформировать отчет",
                Anchor = AnchorStyles.Top,
                Size = new Size(150, 30)
            };

            var btnExport = new Button
            {
                Text = "Экспорт в CSV",
                Anchor = AnchorStyles.Top,
                Size = new Size(150, 30)
            };

            var btnPrint = new Button
            {
                Text = "Печать",
                Anchor = AnchorStyles.Top,
                Size = new Size(150, 30)
            };

            btnGenerate.Click += BtnGenerate_Click;
            btnExport.Click += BtnExport_Click;
            btnPrint.Click += BtnPrint_Click;

            // Вторая строка: выбор товара
            var lblProduct = new Label
            {
                Text = "Товар:",
                TextAlign = ContentAlignment.MiddleLeft,
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };

            cmbProduct = new ComboBox
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Size = new Size(200, 25)
            };

            // Добавляем элементы в TableLayoutPanel
            // Первая строка
            tableLayout.Controls.Add(lblFromDate, 0, 0);
            tableLayout.Controls.Add(dtpFromDate, 1, 0);
            tableLayout.Controls.Add(lblToDate, 2, 0);
            tableLayout.Controls.Add(dtpToDate, 3, 0);

            // Создаем панель для кнопок
            var buttonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            buttonPanel.Controls.Add(btnGenerate);
            buttonPanel.Controls.Add(btnExport);
            buttonPanel.Controls.Add(btnPrint);

            tableLayout.Controls.Add(buttonPanel, 4, 0);
            tableLayout.SetColumnSpan(buttonPanel, 2); // Кнопки занимают 2 столбца

            // Вторая строка
            tableLayout.Controls.Add(lblProduct, 0, 1);
            tableLayout.SetColumnSpan(lblProduct, 1);

            tableLayout.Controls.Add(cmbProduct, 1, 1);
            tableLayout.SetColumnSpan(cmbProduct, 5); // Combobox занимает оставшиеся 5 столбцов

            controlPanel.Controls.Add(tableLayout);

            // DataGridView для отчета
            dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Location = new Point(0, 120),
                Size = new Size(900, 480)
            };

            // Статус бар
            var statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel("Готово");
            statusStrip.Items.Add(lblStatus);

            this.Controls.AddRange(new Control[] { dataGridView, controlPanel, statusStrip });

            LoadProducts();
        }

        private DataGridView dataGridView;
        private DateTimePicker dtpFromDate;
        private DateTimePicker dtpToDate;
        private ComboBox cmbProduct;
        private ToolStripStatusLabel lblStatus;

        private void LoadProducts()
        {
            try
            {
                using (var connection = DatabaseContext.GetConnection())
                {
                    var query = "SELECT ProductID, Name FROM Products ORDER BY Name";

                    using (var command = new NpgsqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        cmbProduct.Items.Clear();
                        cmbProduct.Items.Add("Все товары");

                        while (reader.Read())
                        {
                            cmbProduct.Items.Add(new ComboBoxItem
                            {
                                Value = reader["ProductID"],
                                Text = reader["Name"].ToString()
                            });
                        }
                    }
                }

                if (cmbProduct.Items.Count > 0)
                    cmbProduct.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ex.ShowError("Ошибка загрузки товаров");
            }
        }

        private void LoadReportData()
        {
            try
            {
                var fromDate = dtpFromDate.Value.Date;
                var toDate = dtpToDate.Value.Date.AddDays(1).AddSeconds(-1);

                var query = @"
            SELECT 
                p.Name as Товар,
                COALESCE(SUM(sup.Quantity), 0) as Приход,
                COALESCE(SUM(ship.Quantity), 0) as Расход,
                p.QuantityInStock as Остаток,
                COALESCE(SUM(sup.TotalPrice), 0) as СуммаПрихода,
                COALESCE(SUM(ship.TotalPrice), 0) as СуммаРасхода
            FROM Products p
            LEFT JOIN Supplies sup ON p.ProductID = sup.ProductID 
                AND sup.SupplyDate BETWEEN @FromDate AND @ToDate
            LEFT JOIN Shipments ship ON p.ProductID = ship.ProductID 
                AND ship.ShipmentDate BETWEEN @FromDate AND @ToDate
            WHERE (@ProductID = 0 OR p.ProductID = @ProductID)
            GROUP BY p.ProductID, p.Name, p.QuantityInStock
            ORDER BY p.Name";

                using (var connection = DatabaseContext.GetConnection())
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FromDate", fromDate);
                    command.Parameters.AddWithValue("@ToDate", toDate);

                    int productId = 0;
                    if (cmbProduct.SelectedIndex > 0 && cmbProduct.SelectedItem is ComboBoxItem selectedItem)
                    {
                        productId = Convert.ToInt32(selectedItem.Value);
                    }
                    command.Parameters.AddWithValue("@ProductID", productId);

                    var dataTable = new DataTable();
                    using (var adapter = new NpgsqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }

                    dataGridView.DataSource = dataTable;

                    // Форматирование
                    if (dataGridView.Columns.Contains("СуммаПрихода"))
                        dataGridView.Columns["СуммаПрихода"].DefaultCellStyle.Format = "C2";
                    if (dataGridView.Columns.Contains("СуммаРасхода"))
                        dataGridView.Columns["СуммаРасхода"].DefaultCellStyle.Format = "C2";

                    lblStatus.Text = $"Найдено записей: {dataTable.Rows.Count}";
                }
            }
            catch (Exception ex)
            {
                ex.ShowError("Ошибка формирования отчета");
            }
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            LoadReportData();
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "CSV файлы (*.csv)|*.csv|Текстовые файлы (*.txt)|*.txt";
                saveDialog.FilterIndex = 1;
                saveDialog.DefaultExt = ".csv";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    bool success = false;

                    if (saveDialog.FilterIndex == 1)
                        success = ExportService.ExportToCsv(dataGridView, saveDialog.FileName);
                    else
                        success = ExportService.ExportToText(dataGridView, saveDialog.FileName);

                    if (success)
                    {
                        "Отчет успешно экспортирован.".ShowInfo();
                    }
                }
            }
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            // Простая реализация печати через PrintDialog
            using (var printDialog = new PrintDialog())
            {
                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    // Здесь можно добавить логику печати
                    "Функция печати в разработке.".ShowInfo();
                }
            }
        }

        private class ComboBoxItem
        {
            public object Value { get; set; }
            public string Text { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }
    }
}