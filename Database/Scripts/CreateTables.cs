using System;
using System.Threading.Tasks;
using Npgsql;
using FormsDB.Database.Context;

namespace FormsDB.Database.Scripts
{
    /// Класс для создания и управления таблицами базы данных
    /// Используется как дополнение к DatabaseHelper
    public static class CreateTables
    {
        /// Создает все таблицы асинхронно (дополнительный метод к DatabaseHelper)
        public static async Task CreateAllTablesAsync()
        {
            try
            {
                Console.WriteLine("Начало создания таблиц...");

                using (var connection = DatabaseContext.GetConnection())
                {
                    await connection.OpenAsync();

                    // Создаем таблицы в правильном порядке
                    await CreateSuppliersTableAsync(connection);
                    await CreateCustomersTableAsync(connection);
                    await CreateStorageLocationsTableAsync(connection);
                    await CreateProductsTableAsync(connection);
                    await CreateSuppliesTableAsync(connection);
                    await CreateShipmentsTableAsync(connection);
                    await CreateInvoicesTableAsync(connection);
                    await CreateAcceptanceCertificatesTableAsync(connection);

                    Console.WriteLine("Все таблицы успешно созданы/проверены");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании таблиц: {ex.Message}");
                throw;
            }
        }

        /// Создает таблицу Suppliers
        private static async Task CreateSuppliersTableAsync(NpgsqlConnection connection)
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS Suppliers (
                    SupplierID SERIAL PRIMARY KEY,
                    Name VARCHAR(100) NOT NULL,
                    ContactPerson VARCHAR(100),
                    Phone VARCHAR(20),
                    Email VARCHAR(100),
                    Address TEXT,
                    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                )";
            await ExecuteSqlCommandAsync(connection, sql);
        }

        /// Создает таблицу Customers
        private static async Task CreateCustomersTableAsync(NpgsqlConnection connection)
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS Customers (
                    CustomerID SERIAL PRIMARY KEY,
                    Name VARCHAR(100) NOT NULL,
                    ContactPerson VARCHAR(100),
                    Phone VARCHAR(20),
                    Email VARCHAR(100),
                    Address TEXT,
                    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                )";
            await ExecuteSqlCommandAsync(connection, sql);
        }

        /// Создает таблицу StorageLocations
        private static async Task CreateStorageLocationsTableAsync(NpgsqlConnection connection)
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS StorageLocations (
                    LocationID SERIAL PRIMARY KEY,
                    Name VARCHAR(100) NOT NULL,
                    Description TEXT,
                    Capacity INTEGER,
                    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                )";
            await ExecuteSqlCommandAsync(connection, sql);
        }

        /// Создает таблицу Products
        private static async Task CreateProductsTableAsync(NpgsqlConnection connection)
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS Products (
                    ProductID SERIAL PRIMARY KEY,
                    Name VARCHAR(100) NOT NULL,
                    Description TEXT,
                    Category VARCHAR(50),
                    UnitPrice DECIMAL(10,2),
                    QuantityInStock INTEGER DEFAULT 0,
                    LocationID INTEGER REFERENCES StorageLocations(LocationID),
                    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                )";
            await ExecuteSqlCommandAsync(connection, sql);
        }

        /// Создает таблицу Supplies
        private static async Task CreateSuppliesTableAsync(NpgsqlConnection connection)
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS Supplies (
                    SupplyID SERIAL PRIMARY KEY,
                    SupplierID INTEGER REFERENCES Suppliers(SupplierID),
                    ProductID INTEGER REFERENCES Products(ProductID),
                    Quantity INTEGER NOT NULL,
                    UnitPrice DECIMAL(10,2),
                    TotalPrice DECIMAL(10,2),
                    SupplyDate DATE DEFAULT CURRENT_DATE,
                    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                )";
            await ExecuteSqlCommandAsync(connection, sql);
        }

        /// Создает таблицу Shipments
        private static async Task CreateShipmentsTableAsync(NpgsqlConnection connection)
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS Shipments (
                    ShipmentID SERIAL PRIMARY KEY,
                    CustomerID INTEGER REFERENCES Customers(CustomerID),
                    ProductID INTEGER REFERENCES Products(ProductID),
                    Quantity INTEGER NOT NULL,
                    UnitPrice DECIMAL(10,2),
                    TotalPrice DECIMAL(10,2),
                    ShipmentDate DATE DEFAULT CURRENT_DATE,
                    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                )";
            await ExecuteSqlCommandAsync(connection, sql);
        }

        /// Создает таблицу Invoices
        private static async Task CreateInvoicesTableAsync(NpgsqlConnection connection)
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS Invoices (
                    InvoiceID SERIAL PRIMARY KEY,
                    CustomerID INTEGER REFERENCES Customers(CustomerID),
                    TotalAmount DECIMAL(10,2),
                    InvoiceDate DATE DEFAULT CURRENT_DATE,
                    DueDate DATE,
                    Status VARCHAR(20) DEFAULT 'Ожидает оплаты'
                    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                )";
            await ExecuteSqlCommandAsync(connection, sql);
        }

        /// Создает таблицу AcceptanceCertificates
        private static async Task CreateAcceptanceCertificatesTableAsync(NpgsqlConnection connection)
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS AcceptanceCertificates (
                    CertificateID SERIAL PRIMARY KEY,
                    SupplyID INTEGER REFERENCES Supplies(SupplyID),
                    AcceptedQuantity INTEGER,
                    AcceptedDate DATE DEFAULT CURRENT_DATE,
                    InspectorName VARCHAR(100),
                    Notes TEXT,
                    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                )";
            await ExecuteSqlCommandAsync(connection, sql);
        }

        /// Выполняет SQL команду без использования Dapper
        private static async Task ExecuteSqlCommandAsync(NpgsqlConnection connection, string sql)
        {
            using (var command = new NpgsqlCommand(sql, connection))
            {
                await command.ExecuteNonQueryAsync();
            }
        }

        /// Проверяет, существуют ли все таблицы (дополнительная проверка)
        public static async Task<bool> CheckTablesExistAsync()
        {
            try
            {
                using (var connection = DatabaseContext.GetConnection())
                {
                    await connection.OpenAsync();

                    string[] tables = { "suppliers", "customers", "storagelocations", "products",
                                       "supplies", "shipments", "invoices", "acceptancecertificates" };

                    foreach (var table in tables)
                    {
                        var sql = $"SELECT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = '{table}')";

                        using (var command = new NpgsqlCommand(sql, connection))
                        {
                            var result = await command.ExecuteScalarAsync();
                            var exists = result != null && (bool)result;

                            if (!exists)
                            {
                                Console.WriteLine($"Таблица {table} не найдена");
                                return false;
                            }
                        }
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка проверки таблиц: {ex.Message}");
                return false;
            }
        }
    }
}