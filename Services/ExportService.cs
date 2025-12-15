using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace FormsDB.Services
{
    public static class ExportService
    {
        public static bool ExportToCsv(DataGridView dataGridView, string filePath)
        {
            try
            {
                var sb = new StringBuilder();

                // Заголовки
                var headers = new List<string>();
                foreach (DataGridViewColumn column in dataGridView.Columns)
                {
                    headers.Add(column.HeaderText);
                }
                sb.AppendLine(string.Join(",", headers));

                // Данные
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        var cells = new List<string>();
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            cells.Add(cell.Value?.ToString() ?? "");
                        }
                        sb.AppendLine(string.Join(",", cells));
                    }
                }

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static bool ExportToText(DataGridView dataGridView, string filePath)
        {
            try
            {
                var sb = new StringBuilder();

                // Заголовки
                var headers = new List<string>();
                foreach (DataGridViewColumn column in dataGridView.Columns)
                {
                    headers.Add(column.HeaderText);
                }
                sb.AppendLine(string.Join("\t", headers));
                sb.AppendLine(new string('-', 100));

                // Данные
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        var cells = new List<string>();
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            cells.Add(cell.Value?.ToString() ?? "");
                        }
                        sb.AppendLine(string.Join("\t", cells));
                    }
                }

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}