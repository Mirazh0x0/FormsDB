using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using FormsDB.Database.Context;
using FormsDB.Tables.Invoices;
using FormsDB.Tables.Customers;

namespace FormsDB.Forms.SubForms
{
    public partial class InvoiceSubForm : UserControl
    {
        private InvoiceRepository _repository;
        private CustomerRepository _customerRepository;
        private List<Invoice> _invoices;
        private List<Customer> _customers;

        public event EventHandler InvoiceSelected;

        public Invoice SelectedInvoice { get; private set; }

        public InvoiceSubForm()
        {
            InitializeComponent();
            _repository = new InvoiceRepository();
            _customerRepository = new CustomerRepository();
            LoadInvoices();
            LoadCustomers();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(900, 400);
            this.BorderStyle = BorderStyle.FixedSingle;

            // Панель управления
            var controlPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(5)
            };

            var btnRefresh = new Button { Text = "Обновить", Location = new Point(5, 5), Size = new Size(80, 25) };
            var btnCreate = new Button { Text = "Создать", Location = new Point(95, 5), Size = new Size(80, 25) };
            var btnEdit = new Button { Text = "Изменить", Location = new Point(185, 5), Size = new Size(80, 25) };
            var btnMarkPaid = new Button { Text = "Оплатить", Location = new Point(275, 5), Size = new Size(80, 25) };
            var btnPrint = new Button { Text = "Печать", Location = new Point(365, 5), Size = new Size(80, 25) };

            // Фильтры
            var lblCustomerFilter = new Label { Text = "Клиент:", Location = new Point(5, 40), Size = new Size(50, 25) };
            cmbCustomerFilter = new ComboBox { Location = new Point(60, 40), Size = new Size(200, 25), DropDownStyle = ComboBoxStyle.DropDownList };

            var lblStatusFilter = new Label { Text = "Статус:", Location = new Point(270, 40), Size = new Size(50, 25) };
            cmbStatusFilter = new ComboBox { Location = new Point(325, 40), Size = new Size(150, 25), DropDownStyle = ComboBoxStyle.DropDownList };

            var lblDateFilter = new Label { Text = "С:", Location = new Point(485, 40), Size = new Size(20, 25) };
            dtpFromDate = new DateTimePicker { Location = new Point(510, 40), Size = new Size(100, 25), Format = DateTimePickerFormat.Short };
            var lblTo = new Label { Text = "По:", Location = new Point(615, 40), Size = new Size(25, 25) };
            dtpToDate = new DateTimePicker { Location = new Point(645, 40), Size = new Size(100, 25), Format = DateTimePickerFormat.Short };

            var btnApplyFilter = new Button { Text = "Применить", Location = new Point(755, 40), Size = new Size(80, 25) };

            // Установка дат по умолчанию
            dtpFromDate.Value = DateTime.Now.AddMonths(-1);
            dtpToDate.Value = DateTime.Now;

            // Заполнение фильтров
            cmbStatusFilter.Items.AddRange(new string[] { "Все", "Pending", "Paid", "Cancelled" });
            cmbStatusFilter.SelectedIndex = 0;

            btnRefresh.Click += BtnRefresh_Click;
            btnCreate.Click += BtnCreate_Click;
            btnEdit.Click += BtnEdit_Click;
            btnMarkPaid.Click += BtnMarkPaid_Click;
            btnPrint.Click += BtnPrint_Click;
            btnApplyFilter.Click += BtnApplyFilter_Click;

            controlPanel.Controls.AddRange(new Control[] {
                btnRefresh, btnCreate, btnEdit, btnMarkPaid, btnPrint,
                lblCustomerFilter, cmbCustomerFilter,
                lblStatusFilter, cmbStatusFilter,
                lblDateFilter, dtpFromDate, lblTo, dtpToDate,
                btnApplyFilter
            });

            // DataGridView для счетов
            dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };

            dataGridView.SelectionChanged += DataGridView_SelectionChanged;
            dataGridView.CellFormatting += DataGridView_CellFormatting;

            // Статус бар
            var statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel("Готово");
            statusStrip.Items.Add(lblStatus);

            this.Controls.AddRange(new Control[] { dataGridView, controlPanel, statusStrip });
        }

        private DataGridView dataGridView;
        private ComboBox cmbCustomerFilter;
        private ComboBox cmbStatusFilter;
        private DateTimePicker dtpFromDate;
        private DateTimePicker dtpToDate;
        private ToolStripStatusLabel lblStatus;

        private void LoadInvoices()
        {
            try
            {
                _invoices = _repository.GetAllInvoices();
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки счетов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadCustomers()
        {
            try
            {
                _customers = _customerRepository.GetAllCustomers();
                cmbCustomerFilter.Items.Clear();
                cmbCustomerFilter.Items.Add("Все клиенты");

                foreach (var customer in _customers)
                {
                    cmbCustomerFilter.Items.Add(customer);
                }

                cmbCustomerFilter.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки клиентов: {ex.Message}");
            }
        }

        private void ApplyFilters()
        {
            var filtered = new List<Invoice>(_invoices);

            // Фильтр по клиенту
            if (cmbCustomerFilter.SelectedIndex > 0 && cmbCustomerFilter.SelectedItem is Customer selectedCustomer)
            {
                filtered = filtered.FindAll(i => i.CustomerID == selectedCustomer.CustomerID);
            }

            // Фильтр по статусу
            if (cmbStatusFilter.SelectedIndex > 0)
            {
                string status = cmbStatusFilter.SelectedItem.ToString();
                filtered = filtered.FindAll(i => i.Status == status);
            }

            // Фильтр по дате
            filtered = filtered.FindAll(i =>
                i.InvoiceDate >= dtpFromDate.Value.Date &&
                i.InvoiceDate <= dtpToDate.Value.Date.AddDays(1).AddSeconds(-1));

            dataGridView.DataSource = filtered;

            // Настройка колонок
            if (dataGridView.Columns.Count > 0)
            {
                dataGridView.Columns["InvoiceID"].HeaderText = "№ Счета";
                dataGridView.Columns["CustomerName"].HeaderText = "Клиент";
                dataGridView.Columns["TotalAmount"].HeaderText = "Сумма";
                dataGridView.Columns["InvoiceDate"].HeaderText = "Дата";
                dataGridView.Columns["DueDate"].HeaderText = "Срок оплаты";
                dataGridView.Columns["Status"].HeaderText = "Статус";
                dataGridView.Columns["CreatedDate"].Visible = false;
                dataGridView.Columns["CustomerID"].Visible = false;

                // Форматирование
                dataGridView.Columns["TotalAmount"].DefaultCellStyle.Format = "C2";
                dataGridView.Columns["InvoiceDate"].DefaultCellStyle.Format = "dd.MM.yyyy";
                dataGridView.Columns["DueDate"].DefaultCellStyle.Format = "dd.MM.yyyy";
            }

            // Расчет статистики
            decimal totalAmount = 0;
            decimal pendingAmount = 0;
            int overdueCount = 0;

            foreach (var invoice in filtered)
            {
                totalAmount += invoice.TotalAmount;

                if (invoice.Status == "Pending")
                {
                    pendingAmount += invoice.TotalAmount;

                    if (invoice.DueDate.HasValue && invoice.DueDate.Value < DateTime.Now.Date)
                    {
                        overdueCount++;
                    }
                }
            }

            lblStatus.Text = $"Всего: {filtered.Count} счетов | Сумма: {totalAmount:C} | Ожидает оплаты: {pendingAmount:C} | Просрочено: {overdueCount}";
        }

        private void DataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                SelectedInvoice = dataGridView.SelectedRows[0].DataBoundItem as Invoice;
                InvoiceSelected?.Invoke(this, EventArgs.Empty);
            }
        }

        private void DataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridView.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string status = e.Value.ToString();

                switch (status)
                {
                    case "Pending":
                        e.CellStyle.ForeColor = Color.Orange;
                        e.CellStyle.Font = new Font(dataGridView.Font, FontStyle.Bold);
                        break;
                    case "Paid":
                        e.CellStyle.ForeColor = Color.Green;
                        break;
                    case "Cancelled":
                        e.CellStyle.ForeColor = Color.Red;
                        break;
                }
            }

            if (dataGridView.Columns[e.ColumnIndex].Name == "DueDate" && e.Value != null)
            {
                DateTime dueDate = (DateTime)e.Value;
                if (dueDate < DateTime.Now.Date)
                {
                    e.CellStyle.ForeColor = Color.Red;
                    e.CellStyle.Font = new Font(dataGridView.Font, FontStyle.Bold);
                }
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadInvoices();
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            using (var dialog = new Forms.DialogForms.GenericDialog("Создание счета"))
            {
                var customerItems = new List<string>();
                customerItems.Add("-- Выберите клиента --");
                foreach (var customer in _customers)
                {
                    customerItems.Add($"{customer.CustomerID} - {customer.Name}");
                }

                dialog.AddComboBox("Клиент:", "CustomerID", customerItems, true);
                dialog.AddNumericBox("Сумма:", "TotalAmount", true);
                dialog.AddDatePicker("Дата выставления:", "InvoiceDate", true, DateTime.Now);
                dialog.AddDatePicker("Срок оплаты:", "DueDate", false, DateTime.Now.AddDays(30));
                dialog.AddComboBox("Статус:", "Status",
                    new List<string> { "Pending", "Paid", "Cancelled" }, true, "Pending");

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var invoice = new Invoice
                        {
                            CustomerID = ExtractIdFromCombo(dialog.GetValue("CustomerID")?.ToString()),
                            TotalAmount = Convert.ToDecimal(dialog.GetValue("TotalAmount")),
                            InvoiceDate = Convert.ToDateTime(dialog.GetValue("InvoiceDate")),
                            DueDate = dialog.GetValue("DueDate") as DateTime?,
                            Status = dialog.GetValue("Status")?.ToString()
                        };

                        int newInvoiceId = _repository.AddInvoice(invoice);
                        LoadInvoices();

                        MessageBox.Show($"Счет №{newInvoiceId} успешно создан.", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка создания счета: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (SelectedInvoice != null)
            {
                using (var dialog = new Forms.DialogForms.GenericDialog("Редактирование счета"))
                {
                    var customerItems = new List<string>();
                    foreach (var customer in _customers)
                    {
                        customerItems.Add($"{customer.CustomerID} - {customer.Name}");
                    }

                    dialog.AddComboBox("Клиент:", "CustomerID", customerItems, true,
                        $"{SelectedInvoice.CustomerID} - {SelectedInvoice.CustomerName}");
                    dialog.AddNumericBox("Сумма:", "TotalAmount", true, (int)SelectedInvoice.TotalAmount);
                    dialog.AddDatePicker("Дата выставления:", "InvoiceDate", true, SelectedInvoice.InvoiceDate);
                    dialog.AddDatePicker("Срок оплаты:", "DueDate", false, SelectedInvoice.DueDate);
                    dialog.AddComboBox("Статус:", "Status",
                        new List<string> { "Pending", "Paid", "Cancelled" }, true, SelectedInvoice.Status);

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            SelectedInvoice.CustomerID = ExtractIdFromCombo(dialog.GetValue("CustomerID")?.ToString());
                            SelectedInvoice.TotalAmount = Convert.ToDecimal(dialog.GetValue("TotalAmount"));
                            SelectedInvoice.InvoiceDate = Convert.ToDateTime(dialog.GetValue("InvoiceDate"));
                            SelectedInvoice.DueDate = dialog.GetValue("DueDate") as DateTime?;
                            SelectedInvoice.Status = dialog.GetValue("Status")?.ToString();

                            _repository.UpdateInvoice(SelectedInvoice);
                            LoadInvoices();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка редактирования счета: {ex.Message}", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите счет для редактирования.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnMarkPaid_Click(object sender, EventArgs e)
        {
            if (SelectedInvoice != null)
            {
                if (SelectedInvoice.Status == "Pending")
                {
                    var result = MessageBox.Show($"Отметить счет №{SelectedInvoice.InvoiceID} как оплаченный?",
                        "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        _repository.UpdateInvoiceStatus(SelectedInvoice.InvoiceID, "Paid");
                        LoadInvoices();

                        MessageBox.Show("Счет отмечен как оплаченный.", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Можно отмечать только счета со статусом 'Pending'.", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Выберите счет для изменения статуса.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            if (SelectedInvoice != null)
            {
                PrintInvoice();
            }
            else
            {
                MessageBox.Show("Выберите счет для печати.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void PrintInvoice()
        {
            try
            {
                var printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    // Здесь должна быть логика печати счета
                    // Для простоты создаем текстовый файл

                    using (var saveDialog = new SaveFileDialog())
                    {
                        saveDialog.Filter = "Текстовые файлы (*.txt)|*.txt";
                        saveDialog.FileName = $"Счет_{SelectedInvoice.InvoiceID}_{DateTime.Now:yyyyMMdd}.txt";

                        if (saveDialog.ShowDialog() == DialogResult.OK)
                        {
                            var sb = new System.Text.StringBuilder();
                            sb.AppendLine(new string('=', 50));
                            sb.AppendLine("                 СЧЕТ НА ОПЛАТУ");
                            sb.AppendLine(new string('=', 50));
                            sb.AppendLine();
                            sb.AppendLine($"Номер счета: {SelectedInvoice.InvoiceID}");
                            sb.AppendLine($"Дата выставления: {SelectedInvoice.InvoiceDate:dd.MM.yyyy}");
                            sb.AppendLine($"Клиент: {SelectedInvoice.CustomerName}");
                            sb.AppendLine($"Сумма: {SelectedInvoice.TotalAmount:C}");

                            if (SelectedInvoice.DueDate.HasValue)
                            {
                                sb.AppendLine($"Срок оплаты: {SelectedInvoice.DueDate.Value:dd.MM.yyyy}");
                            }

                            sb.AppendLine($"Статус: {SelectedInvoice.Status}");
                            sb.AppendLine();
                            sb.AppendLine(new string('-', 50));
                            sb.AppendLine("                ПОДПИСЬ И ПЕЧАТЬ");
                            sb.AppendLine(new string('=', 50));

                            System.IO.File.WriteAllText(saveDialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);

                            MessageBox.Show($"Счет сохранен в файл:\n{saveDialog.FileName}", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при печати: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnApplyFilter_Click(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private int ExtractIdFromCombo(string comboValue)
        {
            if (string.IsNullOrEmpty(comboValue))
                return 0;

            var parts = comboValue.Split('-');
            if (parts.Length > 0 && int.TryParse(parts[0].Trim(), out int id))
                return id;

            return 0;
        }

        public void RefreshData()
        {
            LoadInvoices();
        }

        public DataTable GetInvoicesDataTable()
        {
            var dataTable = new DataTable();

            try
            {
                using (var connection = DatabaseContext.GetConnection())
                {
                    var query = @"
                        SELECT 
                            i.InvoiceID,
                            i.InvoiceDate,
                            c.Name as CustomerName,
                            i.TotalAmount,
                            i.DueDate,
                            i.Status
                        FROM Invoices i
                        LEFT JOIN Customers c ON i.CustomerID = c.CustomerID
                        WHERE i.InvoiceDate BETWEEN @FromDate AND @ToDate
                        ORDER BY i.InvoiceDate DESC";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FromDate", dtpFromDate.Value.Date);
                        command.Parameters.AddWithValue("@ToDate", dtpToDate.Value.Date.AddDays(1).AddSeconds(-1));

                        using (var adapter = new NpgsqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения данных: {ex.Message}");
            }

            return dataTable;
        }
    }
}