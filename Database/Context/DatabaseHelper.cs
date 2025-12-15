using System.Configuration;
using System;
using System.IO;
using Npgsql;

namespace FormsDB.Database.Context
{
    public static class DatabaseHelper
    {
        public static void InitializeDatabase()
        {
            try
            {
                // Проверяем существование базы данных
                if (!DatabaseExists())
                {
                    CreateDatabase();
                }

                // Создаем таблицы
                CreateTables();

                Console.WriteLine("База данных инициализирована успешно.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка инициализации базы данных: {ex.Message}", ex);
            }
        }

        private static bool DatabaseExists()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            var databaseName = builder.Database;
            builder.Database = "postgres";

            using (var connection = new NpgsqlConnection(builder.ConnectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    $"SELECT 1 FROM pg_database WHERE datname = '{databaseName}'", connection))
                {
                    var result = command.ExecuteScalar();
                    return result != null;
                }
            }
        }

        private static void CreateDatabase()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            var databaseName = builder.Database;
            builder.Database = "postgres";

            using (var connection = new NpgsqlConnection(builder.ConnectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    $"CREATE DATABASE \"{databaseName}\"", connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        private static void CreateTables()
        {
            var scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "Scripts", "CreateTables.sql");

            if (File.Exists(scriptPath))
            {
                var script = File.ReadAllText(scriptPath);
                ExecuteScript(script);
            }
            else
            {
                // Создаем таблицы через код, если файл не найден
                CreateTablesProgrammatically();
            }
        }

        private static void ExecuteScript(string script)
        {
            using (var connection = DatabaseContext.GetConnection())
            {
                using (var command = new NpgsqlCommand(script, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        private static void CreateTablesProgrammatically()
        {
            var script = @"
                -- Таблица поставщиков
                CREATE TABLE IF NOT EXISTS Suppliers (
                    SupplierID SERIAL PRIMARY KEY,
                    Name VARCHAR(100) NOT NULL,
                    ContactPerson VARCHAR(100),
                    Phone VARCHAR(20),
                    Email VARCHAR(100),
                    Address TEXT,
                    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );
                
                -- Таблица клиентов
                CREATE TABLE IF NOT EXISTS Customers (
                    CustomerID SERIAL PRIMARY KEY,
                    Name VARCHAR(100) NOT NULL,
                    ContactPerson VARCHAR(100),
                    Phone VARCHAR(20),
                    Email VARCHAR(100),
                    Address TEXT,
                    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );
                
                -- Таблица мест хранения
                CREATE TABLE IF NOT EXISTS StorageLocations (
                    LocationID SERIAL PRIMARY KEY,
                    Name VARCHAR(100) NOT NULL,
                    Description TEXT,
                    Capacity INTEGER,
                    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );
                
                -- Таблица продуктов
                CREATE TABLE IF NOT EXISTS Products (
                    ProductID SERIAL PRIMARY KEY,
                    Name VARCHAR(100) NOT NULL,
                    Description TEXT,
                    Category VARCHAR(50),
                    UnitPrice DECIMAL(10,2),
                    QuantityInStock INTEGER DEFAULT 0,
                    LocationID INTEGER REFERENCES StorageLocations(LocationID),
                    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );
                
                -- Таблица поставок
                CREATE TABLE IF NOT EXISTS Supplies (
                    SupplyID SERIAL PRIMARY KEY,
                    SupplierID INTEGER REFERENCES Suppliers(SupplierID),
                    ProductID INTEGER REFERENCES Products(ProductID),
                    Quantity INTEGER NOT NULL,
                    UnitPrice DECIMAL(10,2),
                    TotalPrice DECIMAL(10,2),
                    SupplyDate DATE DEFAULT CURRENT_DATE,
                    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );
                
                -- Таблица отгрузок
                CREATE TABLE IF NOT EXISTS Shipments (
                    ShipmentID SERIAL PRIMARY KEY,
                    CustomerID INTEGER REFERENCES Customers(CustomerID),
                    ProductID INTEGER REFERENCES Products(ProductID),
                    Quantity INTEGER NOT NULL,
                    UnitPrice DECIMAL(10,2),
                    TotalPrice DECIMAL(10,2),
                    ShipmentDate DATE DEFAULT CURRENT_DATE,
                    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );
                
                -- Таблица счетов
                CREATE TABLE IF NOT EXISTS Invoices (
                    InvoiceID SERIAL PRIMARY KEY,
                    CustomerID INTEGER REFERENCES Customers(CustomerID),
                    TotalAmount DECIMAL(10,2),
                    InvoiceDate DATE DEFAULT CURRENT_DATE,
                    DueDate DATE,
                    Status VARCHAR(20) DEFAULT 'Pending',
                    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );
                
                -- Таблица актов приемки
                CREATE TABLE IF NOT EXISTS AcceptanceCertificates (
                    CertificateID SERIAL PRIMARY KEY,
                    SupplyID INTEGER REFERENCES Supplies(SupplyID),
                    AcceptedQuantity INTEGER,
                    AcceptedDate DATE DEFAULT CURRENT_DATE,
                    InspectorName VARCHAR(100),
                    Notes TEXT,
                    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );
            ";

            ExecuteScript(script);
        }
    }
}