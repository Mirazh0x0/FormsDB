using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FormsDB
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Синхронная инициализация БД (ваш существующий код)
            Database.Context.DatabaseHelper.InitializeDatabase();

            // Асинхронная проверка/создание таблиц (дополнительно)
            InitializeDatabaseAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            // Запуск главной формы
            Application.Run(new Forms.MainForm());
        }

        private static async Task InitializeDatabaseAsync()
        {
            try
            {
                await Database.Scripts.CreateTables.CreateAllTablesAsync();
                Console.WriteLine("Асинхронная инициализация таблиц завершена");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Предупреждение при асинхронной инициализации: {ex.Message}");
                // Не прерываем работу приложения, т.к. синхронная инициализация уже выполнена
            }
        }
    }
}