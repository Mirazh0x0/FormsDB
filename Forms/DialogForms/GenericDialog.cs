using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace FormsDB.Forms.DialogForms
{
    public partial class GenericDialog : Form
    {
        private Dictionary<string, Control> _controls = new Dictionary<string, Control>();
        private int _currentY = 20;

        public GenericDialog(string title)
        {
            InitializeComponent(title);
        }

        private void InitializeComponent(string title)
        {
            this.Text = title;
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            _panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                AutoScroll = true
            };

            var btnOK = new Button { Text = "OK", Size = new Size(100, 30), DialogResult = DialogResult.OK };
            var btnCancel = new Button { Text = "Отмена", Size = new Size(100, 30), DialogResult = DialogResult.Cancel };

            btnOK.Click += BtnOK_Click;

            this.Controls.Add(_panel);

            // Кнопки будут добавлены позже
            _btnOK = btnOK;
            _btnCancel = btnCancel;
        }

        private Panel _panel;
        private Button _btnOK;
        private Button _btnCancel;

        public void AddTextBox(string label, string fieldName, bool required, string defaultValue = "", bool multiline = false)
        {
            var lbl = new Label
            {
                Text = label,
                Location = new Point(10, _currentY),
                Size = new Size(100, 25)
            };

            var txt = multiline ?
                new TextBox { Location = new Point(120, _currentY), Size = new Size(250, 60), Multiline = true, ScrollBars = ScrollBars.Vertical } :
                new TextBox { Location = new Point(120, _currentY), Size = new Size(250, 25) };

            txt.Text = defaultValue;
            txt.Tag = required;

            _panel.Controls.Add(lbl);
            _panel.Controls.Add(txt);
            _controls[fieldName] = txt;

            _currentY += multiline ? 70 : 40;
        }

        public void AddNumericBox(string label, string fieldName, bool required, int? defaultValue = null)
        {
            var lbl = new Label
            {
                Text = label,
                Location = new Point(10, _currentY),
                Size = new Size(100, 25)
            };

            var num = new NumericUpDown
            {
                Location = new Point(120, _currentY),
                Size = new Size(250, 25),
                Minimum = 0,
                Maximum = 1000000
            };

            if (defaultValue.HasValue)
                num.Value = defaultValue.Value;

            num.Tag = required;

            _panel.Controls.Add(lbl);
            _panel.Controls.Add(num);
            _controls[fieldName] = num;

            _currentY += 40;
        }

        public void AddComboBox(string label, string fieldName, List<string> items, bool required, string defaultValue = "")
        {
            var lbl = new Label
            {
                Text = label,
                Location = new Point(10, _currentY),
                Size = new Size(100, 25)
            };

            var cmb = new ComboBox
            {
                Location = new Point(120, _currentY),
                Size = new Size(250, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cmb.Items.AddRange(items.ToArray());
            if (!string.IsNullOrEmpty(defaultValue) && cmb.Items.Contains(defaultValue))
                cmb.SelectedItem = defaultValue;
            else if (cmb.Items.Count > 0)
                cmb.SelectedIndex = 0;

            cmb.Tag = required;

            _panel.Controls.Add(lbl);
            _panel.Controls.Add(cmb);
            _controls[fieldName] = cmb;

            _currentY += 40;
        }

        public void AddDatePicker(string label, string fieldName, bool required, DateTime? defaultValue = null)
        {
            var lbl = new Label
            {
                Text = label,
                Location = new Point(10, _currentY),
                Size = new Size(100, 25)
            };

            var dtp = new DateTimePicker
            {
                Location = new Point(120, _currentY),
                Size = new Size(250, 25),
                Format = DateTimePickerFormat.Short
            };

            if (defaultValue.HasValue)
                dtp.Value = defaultValue.Value;

            dtp.Tag = required;

            _panel.Controls.Add(lbl);
            _panel.Controls.Add(dtp);
            _controls[fieldName] = dtp;

            _currentY += 40;
        }

        public object GetValue(string fieldName)
        {
            if (_controls.ContainsKey(fieldName))
            {
                var control = _controls[fieldName];

                if (control is TextBox textBox)
                    return textBox.Text;
                else if (control is NumericUpDown numericUpDown)
                    return (int)numericUpDown.Value;
                else if (control is ComboBox comboBox)
                    return comboBox.SelectedItem?.ToString();
                else if (control is DateTimePicker dateTimePicker)
                    return dateTimePicker.Value;
            }

            return null;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            // Проверка обязательных полей
            foreach (var kvp in _controls)
            {
                var control = kvp.Value;
                bool required = (bool)control.Tag;

                if (required)
                {
                    if (control is TextBox textBox && string.IsNullOrWhiteSpace(textBox.Text))
                    {
                        MessageBox.Show($"Поле '{GetControlLabel(control)}' обязательно для заполнения.", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    else if (control is ComboBox comboBox && comboBox.SelectedItem == null)
                    {
                        MessageBox.Show($"Поле '{GetControlLabel(control)}' обязательно для заполнения.", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }

            this.DialogResult = DialogResult.OK;
        }

        private string GetControlLabel(Control control)
        {
            foreach (Control c in _panel.Controls)
            {
                if (c is Label label && Math.Abs(c.Location.Y - control.Location.Y) < 5)
                {
                    return label.Text.TrimEnd(':');
                }
            }
            return "Поле";
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Добавляем кнопки внизу формы
            _btnOK.Location = new Point(_panel.Width / 2 - 110, _currentY + 20);
            _btnCancel.Location = new Point(_panel.Width / 2 + 10, _currentY + 20);

            _panel.Controls.Add(_btnOK);
            _panel.Controls.Add(_btnCancel);

            // Устанавливаем высоту формы в зависимости от содержимого
            this.Height = Math.Min(_currentY + 120, 600);
        }

        internal void AddTextBox(string v1, string v2, bool v3, bool v4)
        {
            throw new NotImplementedException();
        }
    }
}