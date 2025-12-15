using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using FormsDB.Tables.Supplies;
using FormsDB.Tables.Suppliers;
using FormsDB.Tables.Products;
using FormsDB.Forms.DialogForms;

namespace FormsDB.Tables.Supplies
{
    public partial class SupplyForm : Form
    {
        private SupplyRepository _repository;
        private SupplierRepository _supplierRepository;
        private ProductRepository _productRepository;
        private List<Supply> _supplies;

        public SupplyForm()
        {
            InitializeComponent();
            _repository = new SupplyRepository();
            _supplierRepository = new SupplierRepository();
            _productRepository = new ProductRepository();
            LoadSupplies();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление поставками";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // DataGridView для отображения поставок
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

            var btnAdd = new Button { Text = "Добавить поставку", Location = new Point(10, 10), Size = new Size(140, 30) };
            var btnEdit = new Button { Text = "Редактировать", Location = new Point(160, 10), Size = new Size(120, 30) };
            var btnDelete = new Button { Text = "Удалить", Location = new Point(290, 10), Size = new Size(120, 30) };
            var btnRefresh = new Button { Text = "Обновить", Location = new Point(420, 10), Size = new Size(120, 30) };

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += BtnRefresh_Click;

            panel.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnRefresh });

            this.Controls.Add(dataGridView);
            this.Controls.Add(panel);
        }

        private DataGridView dataGridView;

        private void LoadSupplies()
        {
            _supplies = _repository.GetAllSupplies();
            dataGridView.DataSource = _supplies;

            // Настройка колонок
            if (dataGridView.Columns.Count > 0)
            {
                dataGridView.Columns["SupplyID"].HeaderText = "№ Поставки";
                dataGridView.Columns["SupplierName"].HeaderText = "Поставщик";
                dataGridView.Columns["ProductName"].HeaderText = "Товар";
                dataGridView.Columns["Quantity"].HeaderText = "Количество";
                dataGridView.Columns["UnitPrice"].HeaderText = "Цена за ед.";
                dataGridView.Columns["TotalPrice"].HeaderText = "Общая сумма";
                dataGridView.Columns["SupplyDate"].HeaderText = "Дата поставки";
                dataGridView.Columns["CreatedDate"].HeaderText = "Дата создания";

                // Форматирование
                dataGridView.Columns["UnitPrice"].DefaultCellStyle.Format = "C2";
                dataGridView.Columns["TotalPrice"].DefaultCellStyle.Format = "C2";
                dataGridView.Columns["SupplyDate"].DefaultCellStyle.Format = "dd.MM.yyyy";
                dataGridView.Columns["CreatedDate"].DefaultCellStyle.Format = "dd.MM.yyyy HH:mm";
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var dialog = new GenericDialog("Добавление поставки"))
            {
                var suppliers = _supplierRepository.GetAllSuppliers();
                var products = _productRepository.GetAllProducts();

                var supplierItems = new List<string>();
                supplierItems.Add("-- Выберите поставщика --");
                foreach (var supplier in suppliers)
                {
                    supplierItems.Add($"{supplier.SupplierID} - {supplier.Name}");
                }

                var productItems = new List<string>();
                productItems.Add("-- Выберите товар --");
                foreach (var product in products)
                {
                    productItems.Add($"{product.ProductID} - {product.Name}");
                }

                dialog.AddComboBox("Поставщик:", "SupplierID", supplierItems, true);
                dialog.AddComboBox("Товар:", "ProductID", productItems, true);
                dialog.AddNumericBox("Количество:", "Quantity", true);
                dialog.AddNumericBox("Цена за ед.:", "UnitPrice", true);
                dialog.AddDatePicker("Дата поставки:", "SupplyDate", true, DateTime.Now);

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    int productId = ExtractIdFromCombo(dialog.GetValue("ProductID")?.ToString());
                    int quantity = Convert.ToInt32(dialog.GetValue("Quantity"));
                    decimal unitPrice = Convert.ToDecimal(dialog.GetValue("UnitPrice"));

                    var supply = new Supply
                    {
                        SupplierID = ExtractIdFromCombo(dialog.GetValue("SupplierID")?.ToString()),
                        ProductID = productId,
                        Quantity = quantity,
                        UnitPrice = unitPrice,
                        TotalPrice = quantity * unitPrice,
                        SupplyDate = Convert.ToDateTime(dialog.GetValue("SupplyDate"))
                    };

                    // Обновляем количество товара на складе
                    var product = _productRepository.GetProductById(productId);
                    if (product != null)
                    {
                        product.QuantityInStock += quantity;
                        _productRepository.UpdateProduct(product);
                    }

                    _repository.AddSupply(supply);
                    LoadSupplies();
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                var selectedSupply = dataGridView.SelectedRows[0].DataBoundItem as Supply;
                if (selectedSupply != null)
                {
                    using (var dialog = new GenericDialog("Редактирование поставки"))
                    {
                        var suppliers = _supplierRepository.GetAllSuppliers();
                        var products = _productRepository.GetAllProducts();

                        var supplierItems = new List<string>();
                        foreach (var supplier in suppliers)
                        {
                            supplierItems.Add($"{supplier.SupplierID} - {supplier.Name}");
                        }

                        var productItems = new List<string>();
                        foreach (var product in products)
                        {
                            productItems.Add($"{product.ProductID} - {product.Name}");
                        }

                        dialog.AddComboBox("Поставщик:", "SupplierID", supplierItems, true,
                            $"{selectedSupply.SupplierID} - {selectedSupply.SupplierName}");
                        dialog.AddComboBox("Товар:", "ProductID", productItems, true,
                            $"{selectedSupply.ProductID} - {selectedSupply.ProductName}");
                        dialog.AddNumericBox("Количество:", "Quantity", true, selectedSupply.Quantity);
                        dialog.AddNumericBox("Цена за ед.:", "UnitPrice", true, (int)selectedSupply.UnitPrice);
                        dialog.AddDatePicker("Дата поставки:", "SupplyDate", true, selectedSupply.SupplyDate);

                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            // Восстанавливаем старое количество товара
                            var oldProduct = _productRepository.GetProductById(selectedSupply.ProductID);
                            if (oldProduct != null)
                            {
                                oldProduct.QuantityInStock -= selectedSupply.Quantity;
                                _productRepository.UpdateProduct(oldProduct);
                            }

                            selectedSupply.SupplierID = ExtractIdFromCombo(dialog.GetValue("SupplierID")?.ToString());
                            selectedSupply.ProductID = ExtractIdFromCombo(dialog.GetValue("ProductID")?.ToString());
                            selectedSupply.Quantity = Convert.ToInt32(dialog.GetValue("Quantity"));
                            selectedSupply.UnitPrice = Convert.ToDecimal(dialog.GetValue("UnitPrice"));
                            selectedSupply.TotalPrice = selectedSupply.Quantity * selectedSupply.UnitPrice;
                            selectedSupply.SupplyDate = Convert.ToDateTime(dialog.GetValue("SupplyDate"));

                            // Добавляем новое количество товара
                            var newProduct = _productRepository.GetProductById(selectedSupply.ProductID);
                            if (newProduct != null)
                            {
                                newProduct.QuantityInStock += selectedSupply.Quantity;
                                _productRepository.UpdateProduct(newProduct);
                            }

                            _repository.UpdateSupply(selectedSupply);
                            LoadSupplies();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите поставку для редактирования.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                var selectedSupply = dataGridView.SelectedRows[0].DataBoundItem as Supply;
                if (selectedSupply != null)
                {
                    var result = MessageBox.Show($"Удалить поставку #{selectedSupply.SupplyID}?", "Подтверждение",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        // Восстанавливаем количество товара
                        var product = _productRepository.GetProductById(selectedSupply.ProductID);
                        if (product != null)
                        {
                            product.QuantityInStock -= selectedSupply.Quantity;
                            _productRepository.UpdateProduct(product);
                        }

                        _repository.DeleteSupply(selectedSupply.SupplyID);
                        LoadSupplies();
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите поставку для удаления.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadSupplies();
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