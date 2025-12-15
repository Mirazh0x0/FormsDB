using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using FormsDB.Tables.Products;

namespace FormsDB.Tables.Products
{
    public partial class ProductsForm : Form
    {
        private ProductRepository _repository;
        private List<Product> _products;

        public ProductsForm()
        {
            InitializeComponent();
            _repository = new ProductRepository();
            LoadProducts();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление продуктами";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // DataGridView для отображения продуктов
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

        private void LoadProducts()
        {
            _products = _repository.GetAllProducts();
            dataGridView.DataSource = _products;

            // Настройка колонок
            if (dataGridView.Columns.Count > 0)
            {
                dataGridView.Columns["ProductID"].HeaderText = "ID";
                dataGridView.Columns["Name"].HeaderText = "Название";
                dataGridView.Columns["Description"].HeaderText = "Описание";
                dataGridView.Columns["Category"].HeaderText = "Категория";
                dataGridView.Columns["UnitPrice"].HeaderText = "Цена";
                dataGridView.Columns["QuantityInStock"].HeaderText = "Количество";
                dataGridView.Columns["LocationName"].HeaderText = "Место хранения";
                dataGridView.Columns["CreatedDate"].HeaderText = "Дата создания";
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var dialog = new Forms.DialogForms.ProductDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var product = dialog.Product;
                    _repository.AddProduct(product);
                    LoadProducts();
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                var selectedProduct = dataGridView.SelectedRows[0].DataBoundItem as Product;
                if (selectedProduct != null)
                {
                    using (var dialog = new Forms.DialogForms.ProductDialog(selectedProduct))
                    {
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            _repository.UpdateProduct(dialog.Product);
                            LoadProducts();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите продукт для редактирования.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                var selectedProduct = dataGridView.SelectedRows[0].DataBoundItem as Product;
                if (selectedProduct != null)
                {
                    var result = MessageBox.Show($"Удалить продукт '{selectedProduct.Name}'?", "Подтверждение",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        _repository.DeleteProduct(selectedProduct.ProductID);
                        LoadProducts();
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите продукт для удаления.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadProducts();
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            var searchTerm = ((TextBox)((Button)sender).Parent.Controls[4]).Text;
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                _products = _repository.SearchProducts(searchTerm);
                dataGridView.DataSource = _products;
            }
            else
            {
                LoadProducts();
            }
        }
    }
}