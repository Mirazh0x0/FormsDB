using System;
using System.Drawing;
using System.Windows.Forms;
using FormsDB.Tables.Products;
using FormsDB.Tables.StorageLocations;

namespace FormsDB.Forms.DialogForms
{
    public partial class ProductDialog : Form
    {
        public Product Product { get; private set; }
        private StorageLocationRepository _locationRepository;

        public ProductDialog()
        {
            InitializeComponent();
            Product = new Product();
            LoadLocations();
        }

        public ProductDialog(Product product)
        {
            InitializeComponent();
            Product = product;
            LoadLocations();
            LoadProductData();
        }

        private void InitializeComponent()
        {
            this.Text = "Продукт";
            this.Size = new Size(400, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            var lblName = new Label { Text = "Название:", Location = new Point(10, 20), Size = new Size(100, 25) };
            txtName = new TextBox { Location = new Point(120, 20), Size = new Size(250, 25) };

            var lblDescription = new Label { Text = "Описание:", Location = new Point(10, 60), Size = new Size(100, 25) };
            txtDescription = new TextBox { Location = new Point(120, 60), Size = new Size(250, 25) };

            var lblCategory = new Label { Text = "Категория:", Location = new Point(10, 100), Size = new Size(100, 25) };
            txtCategory = new TextBox { Location = new Point(120, 100), Size = new Size(250, 25) };

            var lblUnitPrice = new Label { Text = "Цена:", Location = new Point(10, 140), Size = new Size(100, 25) };
            txtUnitPrice = new TextBox { Location = new Point(120, 140), Size = new Size(250, 25) };

            var lblQuantity = new Label { Text = "Количество:", Location = new Point(10, 180), Size = new Size(100, 25) };
            txtQuantity = new TextBox { Location = new Point(120, 180), Size = new Size(250, 25) };

            var lblLocation = new Label { Text = "Место хранения:", Location = new Point(10, 220), Size = new Size(100, 25) };
            cmbLocation = new ComboBox { Location = new Point(120, 220), Size = new Size(250, 25), DropDownStyle = ComboBoxStyle.DropDownList };

            var btnOK = new Button { Text = "OK", Location = new Point(120, 280), Size = new Size(100, 30), DialogResult = DialogResult.OK };
            var btnCancel = new Button { Text = "Отмена", Location = new Point(230, 280), Size = new Size(100, 30), DialogResult = DialogResult.Cancel };

            btnOK.Click += BtnOK_Click;

            panel.Controls.AddRange(new Control[] {
                lblName, txtName,
                lblDescription, txtDescription,
                lblCategory, txtCategory,
                lblUnitPrice, txtUnitPrice,
                lblQuantity, txtQuantity,
                lblLocation, cmbLocation,
                btnOK, btnCancel
            });

            this.Controls.Add(panel);
        }

        private TextBox txtName;
        private TextBox txtDescription;
        private TextBox txtCategory;
        private TextBox txtUnitPrice;
        private TextBox txtQuantity;
        private ComboBox cmbLocation;

        private void LoadLocations()
        {
            _locationRepository = new StorageLocationRepository();
            var locations = _locationRepository.GetAllStorageLocations();

            cmbLocation.Items.Add(new ComboBoxItem { Text = "Не выбрано", Value = null });

            foreach (var location in locations)
            {
                cmbLocation.Items.Add(new ComboBoxItem
                {
                    Text = location.Name,
                    Value = location.LocationID
                });
            }

            if (cmbLocation.Items.Count > 0)
                cmbLocation.SelectedIndex = 0;
        }

        private void LoadProductData()
        {
            txtName.Text = Product.Name;
            txtDescription.Text = Product.Description;
            txtCategory.Text = Product.Category;
            txtUnitPrice.Text = Product.UnitPrice.ToString();
            txtQuantity.Text = Product.QuantityInStock.ToString();

            // Выбираем местоположение
            if (Product.LocationID.HasValue)
            {
                foreach (ComboBoxItem item in cmbLocation.Items)
                {
                    if (item.Value != null && (int)item.Value == Product.LocationID.Value)
                    {
                        cmbLocation.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (ValidateInput())
            {
                Product.Name = txtName.Text;
                Product.Description = txtDescription.Text;
                Product.Category = txtCategory.Text;
                Product.UnitPrice = decimal.Parse(txtUnitPrice.Text);
                Product.QuantityInStock = int.Parse(txtQuantity.Text);

                var selectedItem = cmbLocation.SelectedItem as ComboBoxItem;
                Product.LocationID = selectedItem?.Value as int?;

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название продукта.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!decimal.TryParse(txtUnitPrice.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Введите корректную цену.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity < 0)
            {
                MessageBox.Show("Введите корректное количество.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private class ComboBoxItem
        {
            public string Text { get; set; }
            public object Value { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }
    }
}