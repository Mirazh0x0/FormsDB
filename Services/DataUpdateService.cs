using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using FormsDB.Database.Context;

namespace FormsDB.Services
{
    public class DataUpdateService
    {
        private CancellationTokenSource _cancellationTokenSource;

        public void StartAutoUpdate(int intervalMinutes = 5)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            Task.Run(async () =>
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(intervalMinutes * 60 * 1000, _cancellationTokenSource.Token);

                        if (!_cancellationTokenSource.Token.IsCancellationRequested)
                        {
                            UpdateStatistics();
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        // Задача отменена - нормальное завершение
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка автоматического обновления: {ex.Message}");
                    }
                }
            }, _cancellationTokenSource.Token);
        }

        public void StopAutoUpdate()
        {
            _cancellationTokenSource?.Cancel();
        }

        private void UpdateStatistics()
        {
            try
            {
                using (var connection = DatabaseContext.GetConnection())
                {
                    // Обновление каких-либо статистических данных
                    // Например, можно обновить кэшированные суммы или счетчики

                    Console.WriteLine($"Статистика обновлена: {DateTime.Now:HH:mm:ss}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обновления статистики: {ex.Message}");
            }
        }

        public void UpdateProductStock(int productId, int quantityChange)
        {
            try
            {
                using (var connection = DatabaseContext.GetConnection())
                {
                    var query = @"
                        UPDATE Products 
                        SET QuantityInStock = QuantityInStock + @QuantityChange
                        WHERE ProductID = @ProductID";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ProductID", productId);
                        command.Parameters.AddWithValue("@QuantityChange", quantityChange);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обновления запасов: {ex.Message}");
                throw;
            }
        }
    }
}