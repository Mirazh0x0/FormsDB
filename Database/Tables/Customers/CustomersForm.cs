using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using FormsDB.Tables.Customers;
using FormsDB.Forms.DialogForms;

namespace FormsDB.Tables.Customers
{
    public partial class CustomersForm : Form
    {
        private CustomerRepository _repository;
        private List<Customer> _customers;

        public CustomersForm()
        {
            InitializeComponent();
            _repository = new CustomerRepository();
            LoadCustomers();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление клиентами";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // DataGridView для отображения клиентов
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
                Height = 50
            };

            var btnAdd = new Button { Text = "Добавить", Location = new Point(10, 10), Size = new Size(100, 30) };
            var btnEdit = new Button { Text = "Редактировать", Location = new Point(120, 10), Size = new Size(100, 30) };
            var btnDelete = new Button { Text = "Удалить", Location = new Point(230, 10), Size = new Size(100, 30) };
            var btnRefresh = new Button { Text = "Обновить", Location = new Point(340, 10), Size = new Size(100, 30) };

            var txtSearch = new TextBox { Location = new Point(450, 15), Size = new Size(200, 25) };
            var btnSearch = new Button { Text = "Поиск", Location = new Point(660, 10), Size = new Size(100, 30) };

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += BtnRefresh_Click;
            btnSearch.Click += BtnSearch_Click;

            panel.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnRefresh, txtSearch, btnSearch });

            this.Controls.Add(dataGridView);
            this.Controls.Add(panel);
        }

        private DataGridView dataGridView;

        private void LoadCustomers()
        {
            _customers = _repository.GetAllCustomers();
            dataGridView.DataSource = _customers;

            // Настройка колонок
            if (dataGridView.Columns.Count > 0)
            {
                dataGridView.Columns["CustomerID"].HeaderText = "ID";
                dataGridView.Columns["Name"].HeaderText = "Название";
                dataGridView.Columns["ContactPerson"].HeaderText = "Контактное лицо";
                dataGridView.Columns["Phone"].HeaderText = "Телефон";
                dataGridView.Columns["Email"].HeaderText = "Email";
                dataGridView.Columns["Address"].HeaderText = "Адрес";
                dataGridView.Columns["CreatedDate"].HeaderText = "Дата создания";
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var dialog = new CustomerDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var customer = dialog.Customer;
                    _repository.AddCustomer(customer);
                    LoadCustomers();
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                var selectedCustomer = dataGridView.SelectedRows[0].DataBoundItem as Customer;
                if (selectedCustomer != null)
                {
                    using (var dialog = new CustomerDialog(selectedCustomer))
                    {
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            _repository.UpdateCustomer(dialog.Customer);
                            LoadCustomers();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите клиента для редактирования.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                var selectedCustomer = dataGridView.SelectedRows[0].DataBoundItem as Customer;
                if (selectedCustomer != null)
                {
                    var result = MessageBox.Show($"Удалить клиента '{selectedCustomer.Name}'?", "Подтверждение",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        _repository.DeleteCustomer(selectedCustomer.CustomerID);
                        LoadCustomers();
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите клиента для удаления.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadCustomers();
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            var searchTerm = ((TextBox)((Button)sender).Parent.Controls[4]).Text;
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                // Простой поиск по имени
                var filtered = _customers.FindAll(c =>
                    c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (c.ContactPerson != null && c.ContactPerson.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (c.Email != null && c.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));

                dataGridView.DataSource = filtered;
            }
            else
            {
                LoadCustomers();
            }
        }
    }
}