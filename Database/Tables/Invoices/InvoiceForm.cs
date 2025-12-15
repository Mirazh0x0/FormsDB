using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using FormsDB.Tables.Invoices;
using FormsDB.Tables.Customers;
using FormsDB.Forms.DialogForms;

namespace FormsDB.Tables.Invoices
{
    public partial class InvoiceForm : Form
    {
        private InvoiceRepository _repository;
        private CustomerRepository _customerRepository;
        private List<Invoice> _invoices;
        private List<Customer> _customers;

        public InvoiceForm()
        {
            InitializeComponent();
            _repository = new InvoiceRepository();
            _customerRepository = new CustomerRepository();
            LoadInvoices();
            LoadCustomers();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление счетами";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // DataGridView для отображения счетов
            dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            // Панель управления
            var panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80
            };

            var btnAdd = new Button { Text = "Добавить счет", Location = new Point(10, 10), Size = new Size(120, 30) };
            var btnEdit = new Button { Text = "Редактировать", Location = new Point(140, 10), Size = new Size(120, 30) };
            var btnDelete = new Button { Text = "Удалить", Location = new Point(270, 10), Size = new Size(120, 30) };
            var btnRefresh = new Button { Text = "Обновить", Location = new Point(400, 10), Size = new Size(120, 30) };
            var btnMarkPaid = new Button { Text = "Отметить как оплаченный", Location = new Point(530, 10), Size = new Size(180, 30) };

            // Фильтры
            var lblFilter = new Label { Text = "Фильтр:", Location = new Point(10, 50), Size = new Size(50, 25) };
            cmbCustomerFilter = new ComboBox { Location = new Point(70, 50), Size = new Size(200, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatusFilter = new ComboBox { Location = new Point(280, 50), Size = new Size(150, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            var btnFilter = new Button { Text = "Применить", Location = new Point(440, 50), Size = new Size(100, 25) };
            var btnClearFilter = new Button { Text = "Сбросить", Location = new Point(550, 50), Size = new Size(100, 25) };

            cmbCustomerFilter.Items.Add("Все клиенты");
            cmbStatusFilter.Items.AddRange(new string[] { "Все статусы", "Pending", "Paid", "Cancelled" });

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += BtnRefresh_Click;
            btnMarkPaid.Click += BtnMarkPaid_Click;
            btnFilter.Click += BtnFilter_Click;
            btnClearFilter.Click += BtnClearFilter_Click;

            panel.Controls.AddRange(new Control[] {
                btnAdd, btnEdit, btnDelete, btnRefresh, btnMarkPaid,
                lblFilter, cmbCustomerFilter, cmbStatusFilter, btnFilter, btnClearFilter
            });

            this.Controls.Add(dataGridView);
            this.Controls.Add(panel);
        }

        private DataGridView dataGridView;
        private ComboBox cmbCustomerFilter;
        private ComboBox cmbStatusFilter;

        private void LoadInvoices()
        {
            _invoices = _repository.GetAllInvoices();
            dataGridView.DataSource = _invoices;

            // Настройка колонок
            if (dataGridView.Columns.Count > 0)
            {
                dataGridView.Columns["InvoiceID"].HeaderText = "№ Счета";
                dataGridView.Columns["CustomerName"].HeaderText = "Клиент";
                dataGridView.Columns["TotalAmount"].HeaderText = "Сумма";
                dataGridView.Columns["InvoiceDate"].HeaderText = "Дата выставления";
                dataGridView.Columns["DueDate"].HeaderText = "Срок оплаты";
                dataGridView.Columns["Status"].HeaderText = "Статус";
                dataGridView.Columns["CreatedDate"].HeaderText = "Дата создания";

                // Форматирование
                dataGridView.Columns["TotalAmount"].DefaultCellStyle.Format = "C2";
                dataGridView.Columns["InvoiceDate"].DefaultCellStyle.Format = "dd.MM.yyyy";
                dataGridView.Columns["DueDate"].DefaultCellStyle.Format = "dd.MM.yyyy";
                dataGridView.Columns["CreatedDate"].DefaultCellStyle.Format = "dd.MM.yyyy HH:mm";
            }
        }

        private void LoadCustomers()
        {
            _customers = _customerRepository.GetAllCustomers();
            cmbCustomerFilter.Items.Clear();
            cmbCustomerFilter.Items.Add("Все клиенты");

            foreach (var customer in _customers)
            {
                cmbCustomerFilter.Items.Add(customer);
            }

            if (cmbCustomerFilter.Items.Count > 0)
                cmbCustomerFilter.SelectedIndex = 0;
            if (cmbStatusFilter.Items.Count > 0)
                cmbStatusFilter.SelectedIndex = 0;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var dialog = new GenericDialog("Добавление счета"))
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
                    var invoice = new Invoice
                    {
                        CustomerID = ExtractIdFromCombo(dialog.GetValue("CustomerID")?.ToString()),
                        TotalAmount = Convert.ToDecimal(dialog.GetValue("TotalAmount")),
                        InvoiceDate = Convert.ToDateTime(dialog.GetValue("InvoiceDate")),
                        DueDate = dialog.GetValue("DueDate") as DateTime?,
                        Status = dialog.GetValue("Status")?.ToString()
                    };

                    _repository.AddInvoice(invoice);
                    LoadInvoices();
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                var selectedInvoice = dataGridView.SelectedRows[0].DataBoundItem as Invoice;
                if (selectedInvoice != null)
                {
                    using (var dialog = new GenericDialog("Редактирование счета"))
                    {
                        var customerItems = new List<string>();
                        foreach (var customer in _customers)
                        {
                            customerItems.Add($"{customer.CustomerID} - {customer.Name}");
                        }

                        dialog.AddComboBox("Клиент:", "CustomerID", customerItems, true,
                            $"{selectedInvoice.CustomerID} - {selectedInvoice.CustomerName}");
                        dialog.AddNumericBox("Сумма:", "TotalAmount", true, (int)selectedInvoice.TotalAmount);
                        dialog.AddDatePicker("Дата выставления:", "InvoiceDate", true, selectedInvoice.InvoiceDate);
                        dialog.AddDatePicker("Срок оплаты:", "DueDate", false, selectedInvoice.DueDate);
                        dialog.AddComboBox("Статус:", "Status",
                            new List<string> { "Pending", "Paid", "Cancelled" }, true, selectedInvoice.Status);

                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            selectedInvoice.CustomerID = ExtractIdFromCombo(dialog.GetValue("CustomerID")?.ToString());
                            selectedInvoice.TotalAmount = Convert.ToDecimal(dialog.GetValue("TotalAmount"));
                            selectedInvoice.InvoiceDate = Convert.ToDateTime(dialog.GetValue("InvoiceDate"));
                            selectedInvoice.DueDate = dialog.GetValue("DueDate") as DateTime?;
                            selectedInvoice.Status = dialog.GetValue("Status")?.ToString();

                            _repository.UpdateInvoice(selectedInvoice);
                            LoadInvoices();
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

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                var selectedInvoice = dataGridView.SelectedRows[0].DataBoundItem as Invoice;
                if (selectedInvoice != null)
                {
                    var result = MessageBox.Show($"Удалить счет #{selectedInvoice.InvoiceID}?", "Подтверждение",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        _repository.DeleteInvoice(selectedInvoice.InvoiceID);
                        LoadInvoices();
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите счет для удаления.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadInvoices();
        }

        private void BtnMarkPaid_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                var selectedInvoice = dataGridView.SelectedRows[0].DataBoundItem as Invoice;
                if (selectedInvoice != null)
                {
                    _repository.UpdateInvoiceStatus(selectedInvoice.InvoiceID, "Paid");
                    LoadInvoices();
                    MessageBox.Show("Счет отмечен как оплаченный.", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Выберите счет для изменения статуса.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnFilter_Click(object sender, EventArgs e)
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

            dataGridView.DataSource = filtered;
        }

        private void BtnClearFilter_Click(object sender, EventArgs e)
        {
            cmbCustomerFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndex = 0;
            dataGridView.DataSource = _invoices;
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
    }
}