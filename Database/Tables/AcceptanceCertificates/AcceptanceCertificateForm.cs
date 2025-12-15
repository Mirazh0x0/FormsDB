using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using FormsDB.Tables.AcceptanceCertificates;
using FormsDB.Tables.Supplies;
using FormsDB.Forms.DialogForms;

namespace FormsDB.Tables.AcceptanceCertificates
{
    public partial class AcceptanceCertificateForm : Form
    {
        private AcceptanceCertificateRepository _repository;
        private SupplyRepository _supplyRepository;
        private List<AcceptanceCertificate> _certificates;
        private List<Supply> _supplies;

        public AcceptanceCertificateForm()
        {
            InitializeComponent();
            _repository = new AcceptanceCertificateRepository();
            _supplyRepository = new SupplyRepository();
            LoadCertificates();
            LoadSupplies();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление актами приемки";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // DataGridView для отображения актов
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

            var btnAdd = new Button { Text = "Создать акт", Location = new Point(10, 10), Size = new Size(120, 30) };
            var btnEdit = new Button { Text = "Редактировать", Location = new Point(140, 10), Size = new Size(120, 30) };
            var btnDelete = new Button { Text = "Удалить", Location = new Point(270, 10), Size = new Size(120, 30) };
            var btnRefresh = new Button { Text = "Обновить", Location = new Point(400, 10), Size = new Size(120, 30) };

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += BtnRefresh_Click;

            panel.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnRefresh });

            this.Controls.Add(dataGridView);
            this.Controls.Add(panel);
        }

        private DataGridView dataGridView;

        private void LoadCertificates()
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
                dataGridView.Columns["CreatedDate"].HeaderText = "Дата создания";

                // Форматирование
                dataGridView.Columns["AcceptedDate"].DefaultCellStyle.Format = "dd.MM.yyyy";
                dataGridView.Columns["CreatedDate"].DefaultCellStyle.Format = "dd.MM.yyyy HH:mm";
            }
        }

        private void LoadSupplies()
        {
            _supplies = _supplyRepository.GetAllSupplies();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var dialog = new GenericDialog("Создание акта приемки"))
            {
                var supplyItems = new List<string>();
                supplyItems.Add("-- Выберите поставку --");
                foreach (var supply in _supplies)
                {
                    supplyItems.Add($"{supply.SupplyID} - {supply.ProductName} (поставщик: {supply.SupplierName}, количество: {supply.Quantity})");
                }

                dialog.AddComboBox("Поставка:", "SupplyID", supplyItems, true);
                dialog.AddNumericBox("Принятое количество:", "AcceptedQuantity", true);
                dialog.AddDatePicker("Дата приемки:", "AcceptedDate", true, DateTime.Now);
                dialog.AddTextBox("Инспектор:", "InspectorName", false);
                dialog.AddTextBox("Примечания:", "Notes", false, "", true);

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    int supplyId = ExtractIdFromCombo(dialog.GetValue("SupplyID")?.ToString());
                    int acceptedQuantity = Convert.ToInt32(dialog.GetValue("AcceptedQuantity"));

                    // Проверка количества
                    var supply = _supplyRepository.GetSupplyById(supplyId);
                    if (supply != null && acceptedQuantity > supply.Quantity)
                    {
                        MessageBox.Show($"Принятое количество ({acceptedQuantity}) не может превышать общее количество в поставке ({supply.Quantity})", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var certificate = new AcceptanceCertificate
                    {
                        SupplyID = supplyId,
                        AcceptedQuantity = acceptedQuantity,
                        AcceptedDate = Convert.ToDateTime(dialog.GetValue("AcceptedDate")),
                        InspectorName = dialog.GetValue("InspectorName")?.ToString(),
                        Notes = dialog.GetValue("Notes")?.ToString()
                    };

                    _repository.AddCertificate(certificate);
                    LoadCertificates();
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                var selectedCertificate = dataGridView.SelectedRows[0].DataBoundItem as AcceptanceCertificate;
                if (selectedCertificate != null)
                {
                    using (var dialog = new GenericDialog("Редактирование акта приемки"))
                    {
                        var supplyItems = new List<string>();
                        foreach (var supply in _supplies)
                        {
                            supplyItems.Add($"{supply.SupplyID} - {supply.ProductName}");
                        }

                        dialog.AddComboBox("Поставка:", "SupplyID", supplyItems, true,
                            $"{selectedCertificate.SupplyID} - {selectedCertificate.ProductName}");
                        dialog.AddNumericBox("Принятое количество:", "AcceptedQuantity", true, selectedCertificate.AcceptedQuantity);
                        dialog.AddDatePicker("Дата приемки:", "AcceptedDate", true, selectedCertificate.AcceptedDate);
                        dialog.AddTextBox("Инспектор:", "InspectorName", false, selectedCertificate.InspectorName);
                        dialog.AddTextBox("Примечания:", "Notes", false, selectedCertificate.Notes, true);

                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            int supplyId = ExtractIdFromCombo(dialog.GetValue("SupplyID")?.ToString());
                            int acceptedQuantity = Convert.ToInt32(dialog.GetValue("AcceptedQuantity"));

                            // Проверка количества
                            var supply = _supplyRepository.GetSupplyById(supplyId);
                            if (supply != null && acceptedQuantity > supply.Quantity)
                            {
                                MessageBox.Show($"Принятое количество ({acceptedQuantity}) не может превышать общее количество в поставке ({supply.Quantity})", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            selectedCertificate.SupplyID = supplyId;
                            selectedCertificate.AcceptedQuantity = acceptedQuantity;
                            selectedCertificate.AcceptedDate = Convert.ToDateTime(dialog.GetValue("AcceptedDate"));
                            selectedCertificate.InspectorName = dialog.GetValue("InspectorName")?.ToString();
                            selectedCertificate.Notes = dialog.GetValue("Notes")?.ToString();

                            _repository.UpdateCertificate(selectedCertificate);
                            LoadCertificates();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите акт для редактирования.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                var selectedCertificate = dataGridView.SelectedRows[0].DataBoundItem as AcceptanceCertificate;
                if (selectedCertificate != null)
                {
                    var result = MessageBox.Show($"Удалить акт #{selectedCertificate.CertificateID}?", "Подтверждение",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        _repository.DeleteCertificate(selectedCertificate.CertificateID);
                        LoadCertificates();
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите акт для удаления.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadCertificates();
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