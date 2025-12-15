using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using FormsDB.Tables.Suppliers;
using FormsDB.Forms.DialogForms;

namespace FormsDB.Tables.Suppliers
{
    public partial class SuppliersForm : Form
    {
        private SupplierRepository _repository;
        private List<Supplier> _suppliers;

        public SuppliersForm()
        {
            InitializeComponent();
            _repository = new SupplierRepository();
            LoadSuppliers();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление поставщиками";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // DataGridView для отображения поставщиков
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

        private void LoadSuppliers()
        {
            _suppliers = _repository.GetAllSuppliers();
            dataGridView.DataSource = _suppliers;

            // Настройка колонок
            if (dataGridView.Columns.Count > 0)
            {
                dataGridView.Columns["SupplierID"].HeaderText = "ID";
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
            using (var dialog = new SupplierDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var supplier = dialog.Supplier;
                    _repository.AddSupplier(supplier);
                    LoadSuppliers();
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                var selectedSupplier = dataGridView.SelectedRows[0].DataBoundItem as Supplier;
                if (selectedSupplier != null)
                {
                    using (var dialog = new SupplierDialog(selectedSupplier))
                    {
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            _repository.UpdateSupplier(dialog.Supplier);
                            LoadSuppliers();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите поставщика для редактирования.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                var selectedSupplier = dataGridView.SelectedRows[0].DataBoundItem as Supplier;
                if (selectedSupplier != null)
                {
                    var result = MessageBox.Show($"Удалить поставщика '{selectedSupplier.Name}'?", "Подтверждение",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        _repository.DeleteSupplier(selectedSupplier.SupplierID);
                        LoadSuppliers();
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите поставщика для удаления.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadSuppliers();
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            var searchTerm = ((TextBox)((Button)sender).Parent.Controls[4]).Text;
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                _suppliers = _repository.SearchSuppliers(searchTerm);
                dataGridView.DataSource = _suppliers;
            }
            else
            {
                LoadSuppliers();
            }
        }
    }
}