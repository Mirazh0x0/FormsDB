using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using FormsDB.Tables.Shipments;
using FormsDB.Tables.Customers;
using FormsDB.Tables.Products;
using FormsDB.Forms.DialogForms;

namespace FormsDB.Tables.Shipments
{
    public partial class ShipmentForm : Form
    {
        private ShipmentRepository _repository;
        private CustomerRepository _customerRepository;
        private ProductRepository _productRepository;
        private List<Shipment> _shipments;

        public ShipmentForm()
        {
            InitializeComponent();
            _repository = new ShipmentRepository();
            _customerRepository = new CustomerRepository();
            _productRepository = new ProductRepository();
            LoadShipments();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление отгрузками";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // DataGridView для отображения отгрузок
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

            var btnAdd = new Button { Text = "Добавить отгрузку", Location = new Point(10, 10), Size = new Size(140, 30) };
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

        private void LoadShipments()
        {
            _shipments = _repository.GetAllShipments();
            dataGridView.DataSource = _shipments;

            // Настройка колонок
            if (dataGridView.Columns.Count > 0)
            {
                dataGridView.Columns["ShipmentID"].HeaderText = "№ Отгрузки";
                dataGridView.Columns["CustomerName"].HeaderText = "Клиент";
                dataGridView.Columns["ProductName"].HeaderText = "Товар";
                dataGridView.Columns["Quantity"].HeaderText = "Количество";
                dataGridView.Columns["UnitPrice"].HeaderText = "Цена за ед.";
                dataGridView.Columns["TotalPrice"].HeaderText = "Общая сумма";
                dataGridView.Columns["ShipmentDate"].HeaderText = "Дата отгрузки";
                dataGridView.Columns["CreatedDate"].HeaderText = "Дата создания";

                // Форматирование
                dataGridView.Columns["UnitPrice"].DefaultCellStyle.Format = "C2";
                dataGridView.Columns["TotalPrice"].DefaultCellStyle.Format = "C2";
                dataGridView.Columns["ShipmentDate"].DefaultCellStyle.Format = "dd.MM.yyyy";
                dataGridView.Columns["CreatedDate"].DefaultCellStyle.Format = "dd.MM.yyyy HH:mm";
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var dialog = new GenericDialog("Добавление отгрузки"))
            {
                var customers = _customerRepository.GetAllCustomers();
                var products = _productRepository.GetAllProducts();

                var customerItems = new List<string>();
                customerItems.Add("-- Выберите клиента --");
                foreach (var customer in customers)
                {
                    customerItems.Add($"{customer.CustomerID} - {customer.Name}");
                }

                var productItems = new List<string>();
                productItems.Add("-- Выберите товар --");
                foreach (var product in products)
                {
                    productItems.Add($"{product.ProductID} - {product.Name} (в наличии: {product.QuantityInStock})");
                }

                dialog.AddComboBox("Клиент:", "CustomerID", customerItems, true);
                dialog.AddComboBox("Товар:", "ProductID", productItems, true);
                dialog.AddNumericBox("Количество:", "Quantity", true);
                dialog.AddNumericBox("Цена за ед.:", "UnitPrice", true);
                dialog.AddDatePicker("Дата отгрузки:", "ShipmentDate", true, DateTime.Now);

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    int productId = ExtractIdFromCombo(dialog.GetValue("ProductID")?.ToString());
                    int quantity = Convert.ToInt32(dialog.GetValue("Quantity"));
                    decimal unitPrice = Convert.ToDecimal(dialog.GetValue("UnitPrice"));

                    // Проверка наличия товара
                    var product = _productRepository.GetProductById(productId);
                    if (product != null && product.QuantityInStock < quantity)
                    {
                        MessageBox.Show($"Недостаточно товара на складе. В наличии: {product.QuantityInStock}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var shipment = new Shipment
                    {
                        CustomerID = ExtractIdFromCombo(dialog.GetValue("CustomerID")?.ToString()),
                        ProductID = productId,
                        Quantity = quantity,
                        UnitPrice = unitPrice,
                        TotalPrice = quantity * unitPrice,
                        ShipmentDate = Convert.ToDateTime(dialog.GetValue("ShipmentDate"))
                    };

                    // Обновляем количество товара на складе
                    product.QuantityInStock -= quantity;
                    _productRepository.UpdateProduct(product);

                    _repository.AddShipment(shipment);
                    LoadShipments();
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                var selectedShipment = dataGridView.SelectedRows[0].DataBoundItem as Shipment;
                if (selectedShipment != null)
                {
                    using (var dialog = new GenericDialog("Редактирование отгрузки"))
                    {
                        var customers = _customerRepository.GetAllCustomers();
                        var products = _productRepository.GetAllProducts();

                        var customerItems = new List<string>();
                        foreach (var customer in customers)
                        {
                            customerItems.Add($"{customer.CustomerID} - {customer.Name}");
                        }

                        var productItems = new List<string>();
                        foreach (var product in products)
                        {
                            productItems.Add($"{product.ProductID} - {product.Name}");
                        }

                        dialog.AddComboBox("Клиент:", "CustomerID", customerItems, true,
                            $"{selectedShipment.CustomerID} - {selectedShipment.CustomerName}");
                        dialog.AddComboBox("Товар:", "ProductID", productItems, true,
                            $"{selectedShipment.ProductID} - {selectedShipment.ProductName}");
                        dialog.AddNumericBox("Количество:", "Quantity", true, selectedShipment.Quantity);
                        dialog.AddNumericBox("Цена за ед.:", "UnitPrice", true, (int)selectedShipment.UnitPrice);
                        dialog.AddDatePicker("Дата отгрузки:", "ShipmentDate", true, selectedShipment.ShipmentDate);

                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            // Восстанавливаем старое количество товара
                            var oldProduct = _productRepository.GetProductById(selectedShipment.ProductID);
                            if (oldProduct != null)
                            {
                                oldProduct.QuantityInStock += selectedShipment.Quantity;
                                _productRepository.UpdateProduct(oldProduct);
                            }

                            selectedShipment.CustomerID = ExtractIdFromCombo(dialog.GetValue("CustomerID")?.ToString());
                            selectedShipment.ProductID = ExtractIdFromCombo(dialog.GetValue("ProductID")?.ToString());
                            selectedShipment.Quantity = Convert.ToInt32(dialog.GetValue("Quantity"));
                            selectedShipment.UnitPrice = Convert.ToDecimal(dialog.GetValue("UnitPrice"));
                            selectedShipment.TotalPrice = selectedShipment.Quantity * selectedShipment.UnitPrice;
                            selectedShipment.ShipmentDate = Convert.ToDateTime(dialog.GetValue("ShipmentDate"));

                            // Вычитаем новое количество товара
                            var newProduct = _productRepository.GetProductById(selectedShipment.ProductID);
                            if (newProduct != null)
                            {
                                newProduct.QuantityInStock -= selectedShipment.Quantity;
                                _productRepository.UpdateProduct(newProduct);
                            }

                            _repository.UpdateShipment(selectedShipment);
                            LoadShipments();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите отгрузку для редактирования.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                var selectedShipment = dataGridView.SelectedRows[0].DataBoundItem as Shipment;
                if (selectedShipment != null)
                {
                    var result = MessageBox.Show($"Удалить отгрузку #{selectedShipment.ShipmentID}?", "Подтверждение",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        // Восстанавливаем количество товара
                        var product = _productRepository.GetProductById(selectedShipment.ProductID);
                        if (product != null)
                        {
                            product.QuantityInStock += selectedShipment.Quantity;
                            _productRepository.UpdateProduct(product);
                        }

                        _repository.DeleteShipment(selectedShipment.ShipmentID);
                        LoadShipments();
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите отгрузку для удаления.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadShipments();
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