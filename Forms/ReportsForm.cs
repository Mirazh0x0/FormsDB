using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
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

            // Панель управления
            var controlPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                Padding = new Padding(10)
            };

            // Поля дат
            var lblFromDate = new Label { Text = "С:", Location = new Point(10, 15), Size = new Size(30, 25) };
            dtpFromDate = new DateTimePicker
            {
                Location = new Point(40, 15),
                Size = new Size(120, 25),
                Value = DateTime.Now.AddMonths(-1)
            };

            var lblToDate = new Label { Text = "По:", Location = new Point(170, 15), Size = new Size(30, 25) };
            dtpToDate = new DateTimePicker
            {
                Location = new Point(200, 15),
                Size = new Size(120, 25),
                Value = DateTime.Now
            };

            // Выбор товара
            var lblProduct = new Label { Text = "Товар:", Location = new Point(10, 50), Size = new Size(50, 25) };
            cmbProduct = new ComboBox
            {
                Location = new Point(60, 50),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Кнопки
            var btnGenerate = new Button
            {
                Text = "Сформировать отчет",
                Location = new Point(270, 15),
                Size = new Size(150, 30)
            };

            var btnExport = new Button
            {
                Text = "Экспорт в CSV",
                Location = new Point(430, 15),
                Size = new Size(150, 30)
            };

            var btnPrint = new Button
            {
                Text = "Печать",
                Location = new Point(590, 15),
                Size = new Size(150, 30)
            };

            btnGenerate.Click += BtnGenerate_Click;
            btnExport.Click += BtnExport_Click;
            btnPrint.Click += BtnPrint_Click;

            controlPanel.Controls.AddRange(new Control[] {
                lblFromDate, dtpFromDate, lblToDate, dtpToDate,
                lblProduct, cmbProduct,
                btnGenerate, btnExport, btnPrint
            });

            // DataGridView для отчета
            dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
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