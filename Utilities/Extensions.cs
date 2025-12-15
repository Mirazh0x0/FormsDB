using System;
using System.Data;
using System.Windows.Forms;

namespace FormsDB.Utilities
{
    public static class Extensions
    {
        public static void ShowError(this Exception ex, string context = "")
        {
            string message = string.IsNullOrEmpty(context) ?
                ex.Message : $"{context}: {ex.Message}";

            MessageBox.Show(message, "Ошибка",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static bool ConfirmAction(this string message, string title = "Подтверждение")
        {
            var result = MessageBox.Show(message, title,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            return result == DialogResult.Yes;
        }

        public static void ShowInfo(this string message, string title = "Информация")
        {
            MessageBox.Show(message, title,
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void BindToComboBox(this ComboBox comboBox, DataTable dataTable,
            string valueMember, string displayMember, bool addEmptyItem = true)
        {
            comboBox.DataSource = null;
            comboBox.Items.Clear();

            if (addEmptyItem)
            {
                comboBox.Items.Add("-- Выберите --");
            }

            foreach (DataRow row in dataTable.Rows)
            {
                comboBox.Items.Add(new ComboBoxItem
                {
                    Value = row[valueMember],
                    Text = row[displayMember].ToString()
                });
            }

            if (comboBox.Items.Count > 0)
                comboBox.SelectedIndex = 0;
        }

        private class ComboBoxItem
        {
            public object Value { get; set; }
            public string Text { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }
    }
}