using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using FormsDB.Tables.StorageLocations;
using FormsDB.Forms.DialogForms;

namespace FormsDB.Tables.StorageLocations
{
    public partial class StorageLocationForm : Form
    {
        private StorageLocationRepository _repository;
        private List<StorageLocation> _locations;

        public StorageLocationForm()
        {
            InitializeComponent();
            _repository = new StorageLocationRepository();
            LoadLocations();
        }

        private void InitializeComponent()
        {
            this.Text = "Управление местами хранения";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;

            // DataGridView для отображения мест хранения
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
                Height = 80
            };

            var btnAdd = new Button { Text = "Добавить", Location = new Point(10, 10), Size = new Size(100, 30) };
            var btnEdit = new Button { Text = "Редактировать", Location = new Point(120, 10), Size = new Size(100, 30) };
            var btnDelete = new Button { Text = "Удалить", Location = new Point(230, 10), Size = new Size(100, 30) };
            var btnRefresh = new Button { Text = "Обновить", Location = new Point(340, 10), Size = new Size(100, 30) };

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += BtnRefresh_Click;

            panel.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnRefresh });

            this.Controls.Add(dataGridView);
            this.Controls.Add(panel);
        }

        private DataGridView dataGridView;

        private void LoadLocations()
        {
            _locations = _repository.GetAllStorageLocations();
            dataGridView.DataSource = _locations;

            // Настройка колонок
            if (dataGridView.Columns.Count > 0)
            {
                dataGridView.Columns["LocationID"].HeaderText = "ID";
                dataGridView.Columns["Name"].HeaderText = "Название";
                dataGridView.Columns["Description"].HeaderText = "Описание";
                dataGridView.Columns["Capacity"].HeaderText = "Емкость";
                dataGridView.Columns["CreatedDate"].HeaderText = "Дата создания";
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var dialog = new GenericDialog("Добавление места хранения"))
            {
                dialog.AddTextBox("Название:", "Name", true);
                dialog.AddTextBox("Описание:", "Description", false, true);
                dialog.AddNumericBox("Емкость:", "Capacity", false);

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var location = new StorageLocation
                    {
                        Name = dialog.GetValue("Name").ToString(),
                        Description = dialog.GetValue("Description")?.ToString(),
                        Capacity = dialog.GetValue("Capacity") as int?
                    };

                    _repository.AddStorageLocation(location);
                    LoadLocations();
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                var selectedLocation = dataGridView.SelectedRows[0].DataBoundItem as StorageLocation;
                if (selectedLocation != null)
                {
                    using (var dialog = new GenericDialog("Редактирование места хранения"))
                    {
                        dialog.AddTextBox("Название:", "Name", true, selectedLocation.Name);
                        dialog.AddTextBox("Описание:", "Description", false, selectedLocation.Description, true);
                        dialog.AddNumericBox("Емкость:", "Capacity", false, selectedLocation.Capacity);

                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            selectedLocation.Name = dialog.GetValue("Name").ToString();
                            selectedLocation.Description = dialog.GetValue("Description")?.ToString();
                            selectedLocation.Capacity = dialog.GetValue("Capacity") as int?;

                            _repository.UpdateStorageLocation(selectedLocation);
                            LoadLocations();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите место хранения для редактирования.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                var selectedLocation = dataGridView.SelectedRows[0].DataBoundItem as StorageLocation;
                if (selectedLocation != null)
                {
                    var result = MessageBox.Show($"Удалить место хранения '{selectedLocation.Name}'?", "Подтверждение",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        _repository.DeleteStorageLocation(selectedLocation.LocationID);
                        LoadLocations();
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите место хранения для удаления.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadLocations();
        }
    }
}