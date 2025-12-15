namespace FormsDB.Utilities
{
    public static class Constants
    {
        public const string DatabaseName = "Wholesale_Warehouse_DB";
        public const string AppName = "Система управления складом";
        public const string AppVersion = "1.0.0";

        public static class Database
        {
            public const int DefaultCommandTimeout = 30;
            public const string DateFormat = "yyyy-MM-dd";
            public const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        }

        public static class Messages
        {
            public const string SaveSuccess = "Данные успешно сохранены.";
            public const string DeleteSuccess = "Данные успешно удалены.";
            public const string DeleteConfirm = "Вы уверены, что хотите удалить эту запись?";
            public const string NoSelection = "Пожалуйста, выберите запись.";
            public const string ConnectionSuccess = "Подключение установлено успешно.";
            public const string ConnectionError = "Ошибка подключения к базе данных.";
        }

        public static class Status
        {
            public const string Active = "Активен";
            public const string Inactive = "Неактивен";
            public const string Pending = "В ожидании";
            public const string Completed = "Завершен";
            public const string Cancelled = "Отменен";
        }
    }
}