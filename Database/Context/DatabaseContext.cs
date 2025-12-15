using System;
using System.IO;
using System.Configuration;
using Npgsql;

namespace FormsDB.Database.Context
{
    public static class DatabaseContext
    {
        private static string _connectionString;

        static DatabaseContext()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        public static NpgsqlConnection GetConnection()
        {
            var connection = new NpgsqlConnection(_connectionString);
            try
            {
                connection.Open();
                return connection;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка подключения к базе данных: {ex.Message}", ex);
            }
        }

        public static void TestConnection()
        {
            using (var connection = GetConnection())
            {
                if (connection.State == System.Data.ConnectionState.Open)
                {
                    Console.WriteLine("Подключение к базе данных успешно установлено.");
                }
            }
        }
    }
}