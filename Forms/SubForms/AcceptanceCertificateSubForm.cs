using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using FormsDB.Database.Context;
using FormsDB.Tables.AcceptanceCertificates;
using System.Text;

namespace FormsDB.Forms.SubForms
{
    public partial class AcceptanceCertificateSubForm : UserControl
    {
        private AcceptanceCertificateRepository _repository;
        private List<AcceptanceCertificate> _certificates;

        public event EventHandler CertificateSelected;

        public AcceptanceCertificate SelectedCertificate { get; private set; }

        public AcceptanceCertificateSubForm()
        {
            InitializeComponent();
            _repository = new AcceptanceCertificateRepository();
            LoadCertificates();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(800, 400);
            this.BorderStyle = BorderStyle.FixedSingle;

            // Панель управления
            var controlPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                Padding = new Padding(5)
            };

            var btnRefresh = new Button { Text = "Обновить", Location = new Point(5, 5), Size = new Size(80, 25) };
            var btnViewDetails = new Button { Text = "Просмотр", Location = new Point(95, 5), Size = new Size(80, 25) };
            var btnCreateReport = new Button { Text = "Отчет", Location = new Point(185, 5), Size = new Size(80, 25) };

            var lblFilter = new Label { Text = "Фильтр:", Location = new Point(275, 10), Size = new Size(40, 25) };
            txtFilter = new TextBox { Location = new Point(320, 8), Size = new Size(150, 25) };
            var btnFilter = new Button { Text = "Применить", Location = new Point(480, 5), Size = new Size(80, 25) };

            btnRefresh.Click += BtnRefresh_Click;
            btnViewDetails.Click += BtnViewDetails_Click;
            btnCreateReport.Click += BtnCreateReport_Click;
            btnFilter.Click += BtnFilter_Click;

            controlPanel.Controls.AddRange(new Control[] {
                btnRefresh, btnViewDetails, btnCreateReport,
                lblFilter, txtFilter, btnFilter
            });

            // DataGridView для актов
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
            dataGridView.CellDoubleClick += DataGridView_CellDoubleClick;

            // Статус бар
            var statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel("Готово");
            statusStrip.Items.Add(lblStatus);

            this.Controls.AddRange(new Control[] { dataGridView, controlPanel, statusStrip });
        }

        private DataGridView dataGridView;
        private TextBox txtFilter;
        private ToolStripStatusLabel lblStatus;

        private void LoadCertificates()
        {
            try
            {
                _certificates = _repository.GetAllCertificates();
                dataGridView.DataSource = _certificates;

                // Настройка колонок
                if (dataGridView.Columns.Count > 0)
                {
                    dataGridView.Columns["CertificateID"].HeaderText = "№ Акта";
                    dataGridView.Columns["SupplierName"].HeaderText = "Поставщик";
                    dataGridView.Columns["ProductName"].HeaderText = "Товар";
                    dataGridView.Columns["AcceptedQuantity"].HeaderText = "Принято";
                    dataGridView.Columns["TotalQuantity"].HeaderText = "Всего";
                    dataGridView.Columns["AcceptedDate"].HeaderText = "Дата приемки";
                    dataGridView.Columns["InspectorName"].HeaderText = "Инспектор";
                    dataGridView.Columns["Notes"].HeaderText = "Примечания";
                    dataGridView.Columns["CreatedDate"].Visible = false;
                    dataGridView.Columns["SupplyID"].Visible = false;

                    // Форматирование
                    dataGridView.Columns["AcceptedDate"].DefaultCellStyle.Format = "dd.MM.yyyy";
                }

                lblStatus.Text = $"Загружено актов: {_certificates.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки актов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                SelectedCertificate = dataGridView.SelectedRows[0].DataBoundItem as AcceptanceCertificate;
                CertificateSelected?.Invoke(this, EventArgs.Empty);
            }
        }

        private void DataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                ShowCertificateDetails();
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadCertificates();
        }

        private void BtnViewDetails_Click(object sender, EventArgs e)
        {
            ShowCertificateDetails();
        }

        private void ShowCertificateDetails()
        {
            if (SelectedCertificate != null)
            {
                using (var dialog = new Form())
                {
                    dialog.Text = $"Детали акта приемки №{SelectedCertificate.CertificateID}";
                    dialog.Size = new Size(500, 400);
                    dialog.StartPosition = FormStartPosition.CenterParent;
                    dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                    dialog.MaximizeBox = false;

                    var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

                    var details = new StringBuilder();
                    details.AppendLine($"Номер акта: {SelectedCertificate.CertificateID}");
                    details.AppendLine($"Поставщик: {SelectedCertificate.SupplierName}");
                    details.AppendLine($"Товар: {SelectedCertificate.ProductName}");
                    details.AppendLine($"Заказано: {SelectedCertificate.TotalQuantity}");
                    details.AppendLine($"Принято: {SelectedCertificate.AcceptedQuantity}");
                    details.AppendLine($"Дата приемки: {SelectedCertificate.AcceptedDate:dd.MM.yyyy}");
                    details.AppendLine($"Инспектор: {SelectedCertificate.InspectorName ?? "Не указан"}");

                    if (!string.IsNullOrEmpty(SelectedCertificate.Notes))
                    {
                        details.AppendLine();
                        details.AppendLine("Примечания:");
                        details.AppendLine(SelectedCertificate.Notes);
                    }

                    var textBox = new TextBox
                    {
                        Multiline = true,
                        ReadOnly = true,
                        Text = details.ToString(),
                        Dock = DockStyle.Fill,
                        ScrollBars = ScrollBars.Vertical,
                        Font = new Font("Courier New", 10)
                    };

                    panel.Controls.Add(textBox);
                    dialog.Controls.Add(panel);

                    dialog.ShowDialog();
                }
            }
            else
            {
                MessageBox.Show("Выберите акт для просмотра.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnCreateReport_Click(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "Текстовые файлы (*.txt)|*.txt|CSV файлы (*.csv)|*.csv";
                dialog.FilterIndex = 1;
                dialog.DefaultExt = ".txt";
                dialog.FileName = $"Акты_приемки_{DateTime.Now:yyyyMMdd}.txt";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    ExportCertificatesToFile(dialog.FileName);
                }
            }
        }

        private void ExportCertificatesToFile(string filePath)
        {
            try
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Отчет по актам приемки");
                sb.AppendLine($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}");
                sb.AppendLine(new string('=', 80));
                sb.AppendLine();

                foreach (var cert in _certificates)
                {
                    sb.AppendLine($"Акт №{cert.CertificateID}");
                    sb.AppendLine($"  Поставщик: {cert.SupplierName}");
                    sb.AppendLine($"  Товар: {cert.ProductName}");
                    sb.AppendLine($"  Количество: {cert.AcceptedQuantity} из {cert.TotalQuantity}");
                    sb.AppendLine($"  Дата: {cert.AcceptedDate:dd.MM.yyyy}");
                    sb.AppendLine($"  Инспектор: {cert.InspectorName ?? "Не указан"}");

                    if (!string.IsNullOrEmpty(cert.Notes))
                    {
                        sb.AppendLine($"  Примечания: {cert.Notes}");
                    }

                    sb.AppendLine(new string('-', 40));
                }

                System.IO.File.WriteAllText(filePath, sb.ToString(), System.Text.Encoding.UTF8);

                MessageBox.Show($"Отчет успешно сохранен:\n{filePath}", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnFilter_Click(object sender, EventArgs e)
        {
            var filterText = txtFilter.Text.ToLower();

            if (string.IsNullOrWhiteSpace(filterText))
            {
                dataGridView.DataSource = _certificates;
            }
            else
            {
                var filtered = _certificates.FindAll(c =>
                    (c.SupplierName?.ToLower().Contains(filterText) ?? false) ||
                    (c.ProductName?.ToLower().Contains(filterText) ?? false) ||
                    (c.InspectorName?.ToLower().Contains(filterText) ?? false) ||
                    (c.Notes?.ToLower().Contains(filterText) ?? false));

                dataGridView.DataSource = filtered;
            }

            lblStatus.Text = $"Найдено: {dataGridView.RowCount} актов";
        }

        public void RefreshData()
        {
            LoadCertificates();
        }

        public DataTable GetCertificatesDataTable()
        {
            var dataTable = new DataTable();

            try
            {
                using (var connection = DatabaseContext.GetConnection())
                {
                    var query = @"
                        SELECT 
                            ac.CertificateID,
                            ac.AcceptedDate,
                            sup.Name as SupplierName,
                            p.Name as ProductName,
                            ac.AcceptedQuantity,
                            sp.Quantity as TotalQuantity,
                            ac.InspectorName,
                            ac.Notes
                        FROM AcceptanceCertificates ac
                        LEFT JOIN Supplies sp ON ac.SupplyID = sp.SupplyID
                        LEFT JOIN Suppliers sup ON sp.SupplierID = sup.SupplierID
                        LEFT JOIN Products p ON sp.ProductID = p.ProductID
                        ORDER BY ac.AcceptedDate DESC";

                    using (var command = new NpgsqlCommand(query, connection))
                    using (var adapter = new NpgsqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
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