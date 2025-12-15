using System;
using System.Drawing;
using System.Windows.Forms;
using FormsDB.Tables.Suppliers;

namespace FormsDB.Forms.DialogForms
{
    public partial class SupplierDialog : Form
    {
        public Supplier Supplier { get; private set; }

        public SupplierDialog()
        {
            InitializeComponent();
            Supplier = new Supplier();
        }

        public SupplierDialog(Supplier supplier)
        {
            InitializeComponent();
            Supplier = supplier;
            LoadSupplierData();
        }

        private void InitializeComponent()
        {
            this.Text = "Поставщик";
            this.Size = new Size(400, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            var lblName = new Label { Text = "Название:", Location = new Point(10, 20), Size = new Size(100, 25) };
            txtName = new TextBox { Location = new Point(120, 20), Size = new Size(250, 25) };

            var lblContactPerson = new Label { Text = "Контактное лицо:", Location = new Point(10, 60), Size = new Size(100, 25) };
            txtContactPerson = new TextBox { Location = new Point(120, 60), Size = new Size(250, 25) };

            var lblPhone = new Label { Text = "Телефон:", Location = new Point(10, 100), Size = new Size(100, 25) };
            txtPhone = new TextBox { Location = new Point(120, 100), Size = new Size(250, 25) };

            var lblEmail = new Label { Text = "Email:", Location = new Point(10, 140), Size = new Size(100, 25) };
            txtEmail = new TextBox { Location = new Point(120, 140), Size = new Size(250, 25) };

            var lblAddress = new Label { Text = "Адрес:", Location = new Point(10, 180), Size = new Size(100, 25) };
            txtAddress = new TextBox { Location = new Point(120, 180), Size = new Size(250, 25) };

            var btnOK = new Button { Text = "OK", Location = new Point(120, 230), Size = new Size(100, 30), DialogResult = DialogResult.OK };
            var btnCancel = new Button { Text = "Отмена", Location = new Point(230, 230), Size = new Size(100, 30), DialogResult = DialogResult.Cancel };

            btnOK.Click += BtnOK_Click;

            panel.Controls.AddRange(new Control[] {
                lblName, txtName,
                lblContactPerson, txtContactPerson,
                lblPhone, txtPhone,
                lblEmail, txtEmail,
                lblAddress, txtAddress,
                btnOK, btnCancel
            });

            this.Controls.Add(panel);
        }

        private TextBox txtName;
        private TextBox txtContactPerson;
        private TextBox txtPhone;
        private TextBox txtEmail;
        private TextBox txtAddress;

        private void LoadSupplierData()
        {
            txtName.Text = Supplier.Name;
            txtContactPerson.Text = Supplier.ContactPerson;
            txtPhone.Text = Supplier.Phone;
            txtEmail.Text = Supplier.Email;
            txtAddress.Text = Supplier.Address;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (ValidateInput())
            {
                Supplier.Name = txtName.Text;
                Supplier.ContactPerson = txtContactPerson.Text;
                Supplier.Phone = txtPhone.Text;
                Supplier.Email = txtEmail.Text;
                Supplier.Address = txtAddress.Text;

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название поставщика.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }
    }
}