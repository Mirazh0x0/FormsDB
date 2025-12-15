using System;
using System.Drawing;
using System.Windows.Forms;
using FormsDB.Tables.Products;
using FormsDB.Tables.Customers;
using FormsDB.Tables.Suppliers;
using FormsDB.Tables.StorageLocations;
using FormsDB.Tables.Invoices;
using FormsDB.Tables.Shipments;
using FormsDB.Tables.Supplies;
using FormsDB.Tables.AcceptanceCertificates;

namespace FormsDB.Forms
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Система управления оптовым складом";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;

            // Создаем меню
            var menuStrip = new MenuStrip();

            // Меню "Таблицы"
            var tablesMenu = new ToolStripMenuItem("Таблицы");

            var productsItem = new ToolStripMenuItem("Продукты");
            var customersItem = new ToolStripMenuItem("Клиенты");
            var suppliersItem = new ToolStripMenuItem("Поставщики");
            var locationsItem = new ToolStripMenuItem("Места хранения");
            var invoicesItem = new ToolStripMenuItem("Счета");
            var shipmentsItem = new ToolStripMenuItem("Отгрузки");
            var suppliesItem = new ToolStripMenuItem("Поставки");
            var certificatesItem = new ToolStripMenuItem("Акты приемки");

            productsItem.Click += (s, e) => OpenForm(new ProductsForm());
            customersItem.Click += (s, e) => OpenForm(new CustomersForm());
            suppliersItem.Click += (s, e) => OpenForm(new SuppliersForm());
            locationsItem.Click += (s, e) => OpenForm(new StorageLocationForm());
            invoicesItem.Click += (s, e) => OpenForm(new InvoiceForm());
            shipmentsItem.Click += (s, e) => OpenForm(new ShipmentForm());
            suppliesItem.Click += (s, e) => OpenForm(new SupplyForm());
            certificatesItem.Click += (s, e) => OpenForm(new AcceptanceCertificateForm());

            tablesMenu.DropDownItems.AddRange(new ToolStripItem[] {
                productsItem, customersItem, suppliersItem, locationsItem,
                new ToolStripSeparator(),
                invoicesItem, shipmentsItem, suppliesItem, certificatesItem
            });

            // Меню "Отчеты"
            var reportsMenu = new ToolStripMenuItem("Отчеты");
            var report5Item = new ToolStripMenuItem("Отчет 5");
            report5Item.Click += (s, e) => OpenForm(new Report5Form());
            reportsMenu.DropDownItems.Add(report5Item);

            // Меню "Сервис"
            var serviceMenu = new ToolStripMenuItem("Сервис");
            var testConnectionItem = new ToolStripMenuItem("Тест подключения");
            var exitItem = new ToolStripMenuItem("Выход");

            testConnectionItem.Click += TestConnectionItem_Click;
            exitItem.Click += (s, e) => Application.Exit();

            serviceMenu.DropDownItems.AddRange(new ToolStripItem[] {
                testConnectionItem,
                new ToolStripSeparator(),
                exitItem
            });

            menuStrip.Items.AddRange(new ToolStripItem[] { tablesMenu, reportsMenu, serviceMenu });

            // Создаем панель с кнопками для быстрого доступа
            var toolPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.LightGray
            };

            // Кнопки быстрого доступа
            var btnProducts = CreateQuickButton("Продукты", 20);
            var btnCustomers = CreateQuickButton("Клиенты", 140);
            var btnSuppliers = CreateQuickButton("Поставщики", 260);
            var btnInvoices = CreateQuickButton("Счета", 380);

            btnProducts.Click += (s, e) => OpenForm(new ProductsForm());
            btnCustomers.Click += (s, e) => OpenForm(new CustomersForm());
            btnSuppliers.Click += (s, e) => OpenForm(new SuppliersForm());
            btnInvoices.Click += (s, e) => OpenForm(new InvoiceForm());

            toolPanel.Controls.AddRange(new Control[] { btnProducts, btnCustomers, btnSuppliers, btnInvoices });

            // Панель для отображения форм
            _panelContent = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            // Статус бар
            var statusStrip = new StatusStrip();
            _lblStatus = new ToolStripStatusLabel("Готово");
            statusStrip.Items.Add(_lblStatus);

            this.Controls.AddRange(new Control[] { _panelContent, toolPanel, statusStrip });
            this.Controls.Add(menuStrip);
            this.MainMenuStrip = menuStrip;
        }

        private Panel _panelContent;
        private ToolStripStatusLabel _lblStatus;

        private Button CreateQuickButton(string text, int x)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, 20),
                Size = new Size(120, 40),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
        }

        private void OpenForm(Form form)
        {
            // Очищаем панель
            _panelContent.Controls.Clear();

            // Настраиваем форму
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;

            // Добавляем форму на панель
            _panelContent.Controls.Add(form);
            form.Show();

            _lblStatus.Text = $"Открыто: {form.Text}";
        }

        private void TestConnectionItem_Click(object sender, EventArgs e)
        {
            try
            {
                Database.Context.DatabaseContext.TestConnection();
                MessageBox.Show("Подключение к базе данных успешно!", "Тест подключения",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}", "Тест подключения",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}