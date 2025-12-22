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
                Console.WriteLine("=== ИНИЦИАЛИЗАЦИЯ БАЗЫ ДАННЫХ ===");

                bool isNewDatabase = false;

                // Проверяем существование базы данных
                if (!DatabaseExists())
                {
                    Console.WriteLine("База данных не существует, создаем...");
                    CreateDatabase();
                    isNewDatabase = true;
                }
                else
                {
                    Console.WriteLine("База данных уже существует");
                }

                // Создаем таблицы (если их нет)
                CreateTables();

                // Если БД новая - заполняем начальными данными
                if (isNewDatabase)
                {
                    Console.WriteLine("Заполняем новую БД начальными данными...");
                    SeedInitialData();
                }
                else
                {
                    Console.WriteLine("БД уже содержит данные, пропускаем заполнение");
                }

                Console.WriteLine("=== ИНИЦИАЛИЗАЦИЯ ЗАВЕРШЕНА ===");
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка инициализации базы данных: {ex.Message}", ex);
            }
        }

        private static bool DatabaseExists()
        {
            try
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
            catch (Exception)
            {
                // Если не можем проверить, предполагаем что БД существует
                return true;
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
                    Status VARCHAR(20) DEFAULT 'Ожидает оплаты',  -- ИЗМЕНИТЬ: 'Pending' -> 'Ожидает оплаты'
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

        private static void SeedInitialData()
        {
            try
            {
                using (var connection = DatabaseContext.GetConnection())
                {
                    // Проверяем, есть ли уже данные в таблице Suppliers (проверяем только одну таблицу)
                    using (var checkCmd = new NpgsqlCommand("SELECT COUNT(*) FROM Suppliers", connection))
                    {
                        var count = (long)(checkCmd.ExecuteScalar() ?? 0);
                        if (count > 0)
                        {
                            Console.WriteLine("В БД уже есть данные, пропускаем заполнение");
                            return;
                        }
                    }

                    Console.WriteLine("Начинаем заполнение таблиц данными...");

                    // 1. Поставщики (5 записей)
                    SeedSuppliers(connection);

                    // 2. Клиенты (5 записей)
                    SeedCustomers(connection);

                    // 3. Места хранения (5 записей)
                    SeedStorageLocations(connection);

                    // 4. Продукты (5 записей)
                    SeedProducts(connection);

                    // 5. Поставки (5 записей)
                    SeedSupplies(connection);

                    // 6. Отгрузки (5 записей)
                    SeedShipments(connection);

                    // 7. Счета (5 записей) - с русскими статусами
                    SeedInvoices(connection);

                    // 8. Акты приемки (5 записей)
                    SeedAcceptanceCertificates(connection);

                    Console.WriteLine("Все таблицы заполнены по 5 записями");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Предупреждение при заполнении данными: {ex.Message}");
            }
        }

        private static void SeedSuppliers(NpgsqlConnection connection)
        {
            var sql = @"
                INSERT INTO Suppliers (Name, ContactPerson, Phone, Email, Address) 
                VALUES 
                ('ООО МеталлТрейд', 'Иванов Иван', '+79991234567', 'metal@trade.ru', 'Москва, ул. Металлистов, 10'),
                ('ЗАО СтройМатериалы', 'Петров Петр', '+79992345678', 'stroy@materials.ru', 'Санкт-Петербург, пр. Строителей, 25'),
                ('ИП ИнструментПро', 'Сидорова Анна', '+79993456789', 'tools@pro.ru', 'Екатеринбург, ул. Инструментальная, 5'),
                ('ОАО Электросила', 'Козлов Алексей', '+79994567890', 'electro@power.ru', 'Новосибирск, пр. Энергетиков, 15'),
                ('ТД ХимРеактив', 'Морозова Ольга', '+79995678901', 'chemicals@reactive.ru', 'Казань, ул. Химическая, 30');
            ";

            using (var command = new NpgsqlCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void SeedCustomers(NpgsqlConnection connection)
        {
            var sql = @"
                INSERT INTO Customers (Name, ContactPerson, Phone, Email, Address) 
                VALUES 
                ('ООО СтройГрад', 'Смирнов Дмитрий', '+79111111111', 'stroygrad@mail.ru', 'Москва, ул. Строителей, 10'),
                ('ИП МеталлСервис', 'Кузнецов Сергей', '+79112222222', 'metall@service.ru', 'Санкт-Петербург, пр. Металлургов, 20'),
                ('ЗАО Производство+', 'Васильев Андрей', '+79113333333', 'production@plus.ru', 'Екатеринбург, ул. Заводская, 30'),
                ('ООО ТоргКомпания', 'Николаева Елена', '+79114444444', 'trade@company.ru', 'Новосибирск, ул. Торговая, 40'),
                ('ИП Магазин Инструментов', 'Федоров Максим', '+79115555555', 'tools@shop.ru', 'Казань, ул. Магазинная, 50');
            ";

            using (var command = new NpgsqlCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void SeedStorageLocations(NpgsqlConnection connection)
        {
            var sql = @"
                INSERT INTO StorageLocations (Name, Description, Capacity) 
                VALUES 
                ('Склад А', 'Основной склад, сухой', 1000),
                ('Склад Б', 'Холодильный склад', 500),
                ('Склад В', 'Для крупногабаритных товаров', 300),
                ('Склад Г', 'Опасные материалы', 200),
                ('Склад Д', 'Временное хранение', 800);
            ";

            using (var command = new NpgsqlCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void SeedProducts(NpgsqlConnection connection)
        {
            var sql = @"
                INSERT INTO Products (Name, Description, Category, UnitPrice, QuantityInStock, LocationID) 
                VALUES 
                ('Цемент М500', 'Мешок 50 кг', 'Строительные материалы', 450.00, 100, 1),
                ('Песок речной', 'Тонна', 'Строительные материалы', 1200.00, 50, 2),
                ('Арматура 12мм', 'Пруток 6м', 'Металлопрокат', 280.00, 200, 3),
                ('Краска белая', 'Ведро 10л', 'Лакокрасочные', 1500.00, 30, 4),
                ('Гвозди 100мм', 'Упаковка 1кг', 'Крепеж', 120.00, 500, 5);
            ";

            using (var command = new NpgsqlCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void SeedSupplies(NpgsqlConnection connection)
        {
            var sql = @"
                INSERT INTO Supplies (SupplierID, ProductID, Quantity, UnitPrice, TotalPrice, SupplyDate) 
                VALUES 
                (1, 1, 100, 400.00, 40000.00, '2024-01-15'),
                (2, 2, 50, 1100.00, 55000.00, '2024-01-20'),
                (3, 3, 200, 250.00, 50000.00, '2024-02-01'),
                (4, 4, 30, 1400.00, 42000.00, '2024-02-10'),
                (5, 5, 500, 100.00, 50000.00, '2024-02-15');
            ";

            using (var command = new NpgsqlCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void SeedShipments(NpgsqlConnection connection)
        {
            var sql = @"
                INSERT INTO Shipments (CustomerID, ProductID, Quantity, UnitPrice, TotalPrice, ShipmentDate) 
                VALUES 
                (1, 1, 10, 500.00, 5000.00, '2024-01-16'),
                (2, 2, 5, 1300.00, 6500.00, '2024-01-21'),
                (3, 3, 20, 300.00, 6000.00, '2024-02-02'),
                (4, 4, 3, 1600.00, 4800.00, '2024-02-11'),
                (5, 5, 50, 150.00, 7500.00, '2024-02-16');
            ";

            using (var command = new NpgsqlCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void SeedInvoices(NpgsqlConnection connection)
        {
            var sql = @"
                INSERT INTO Invoices (CustomerID, TotalAmount, InvoiceDate, DueDate, Status) 
                VALUES 
                (1, 5000.00, '2024-01-16', '2024-02-16', 'Оплачен'),          -- ИЗМЕНИТЬ: 'Paid' -> 'Оплачен'
                (2, 6500.00, '2024-01-21', '2024-02-21', 'Ожидает оплаты'),   -- ИЗМЕНИТЬ: 'Pending' -> 'Ожидает оплаты'
                (3, 6000.00, '2024-02-02', '2024-03-02', 'Ожидает оплаты'),   -- ИЗМЕНИТЬ: 'Pending' -> 'Ожидает оплаты'
                (4, 4800.00, '2024-02-11', '2024-03-11', 'Оплачен'),          -- ИЗМЕНИТЬ: 'Paid' -> 'Оплачен'
                (5, 7500.00, '2024-02-16', '2024-03-16', 'Ожидает оплаты');   -- ИЗМЕНИТЬ: 'Pending' -> 'Ожидает оплаты'
            ";

            using (var command = new NpgsqlCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void SeedAcceptanceCertificates(NpgsqlConnection connection)
        {
            var sql = @"
                INSERT INTO AcceptanceCertificates (SupplyID, AcceptedQuantity, AcceptedDate, InspectorName, Notes) 
                VALUES 
                (1, 100, '2024-01-15', 'Петров П.П.', 'Качество отличное'),
                (2, 50, '2024-01-20', 'Иванов И.И.', 'Небольшие повреждения упаковки'),
                (3, 200, '2024-02-01', 'Сидоров С.С.', 'Все в норме'),
                (4, 30, '2024-02-10', 'Козлов К.К.', 'Принято частично'),
                (5, 500, '2024-02-15', 'Николаев Н.Н.', 'Полная приемка');
            ";

            using (var command = new NpgsqlCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}