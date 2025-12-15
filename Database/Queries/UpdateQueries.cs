using Npgsql;
using NpgsqlTypes;

namespace FormsDB.Database.Queries
{
    public static class UpdateQueries
    {
        public static (string, NpgsqlParameter[]) UpdateProductQuantity(
            int productId,
            int quantityChange)
        {
            var query = @"
                UPDATE products 
                SET quantity_in_stock = quantity_in_stock + @quantity_change,
                    updated_at = @updated_at
                WHERE id = @product_id";

            var parameters = new[]
            {
                new NpgsqlParameter("@product_id", NpgsqlDbType.Integer) { Value = productId },
                new NpgsqlParameter("@quantity_change", NpgsqlDbType.Integer) { Value = quantityChange },
                new NpgsqlParameter("@updated_at", NpgsqlDbType.Timestamp) { Value = DateTime.Now }
            };

            return (query, parameters);
        }

        public static (string, NpgsqlParameter[]) UpdateInvoiceStatus(
            int invoiceId,
            string status)
        {
            var query = @"
                UPDATE invoices 
                SET status = @status,
                    updated_at = @updated_at
                WHERE id = @invoice_id";

            var parameters = new[]
            {
                new NpgsqlParameter("@invoice_id", NpgsqlDbType.Integer) { Value = invoiceId },
                new NpgsqlParameter("@status", NpgsqlDbType.Varchar) { Value = status },
                new NpgsqlParameter("@updated_at", NpgsqlDbType.Timestamp) { Value = DateTime.Now }
            };

            return (query, parameters);
        }

        public static (string, NpgsqlParameter[]) UpdateSupplyStatus(
            int supplyId,
            string status)
        {
            var query = @"
                UPDATE supplies 
                SET status = @status,
                    delivery_date = CASE 
                        WHEN @status = 'доставлен' THEN @delivery_date 
                        ELSE delivery_date 
                    END
                WHERE id = @supply_id";

            var parameters = new[]
            {
                new NpgsqlParameter("@supply_id", NpgsqlDbType.Integer) { Value = supplyId },
                new NpgsqlParameter("@status", NpgsqlDbType.Varchar) { Value = status },
                new NpgsqlParameter("@delivery_date", NpgsqlDbType.Date) { Value = DateTime.Now }
            };

            return (query, parameters);
        }

        public static (string, NpgsqlParameter[]) BulkUpdateProductPrices(
            decimal percentageIncrease)
        {
            var query = @"
                UPDATE products 
                SET unit_price = unit_price * (1 + @percentage / 100),
                    updated_at = @updated_at
                WHERE supplier_id = @supplier_id";

            var parameters = new[]
            {
                new NpgsqlParameter("@percentage", NpgsqlDbType.Numeric) { Value = percentageIncrease },
                new NpgsqlParameter("@updated_at", NpgsqlDbType.Timestamp) { Value = DateTime.Now }
            };

            return (query, parameters);
        }

        public static (string, NpgsqlParameter[]) UpdateStorageLocationLoad(
            int locationId,
            int loadChange)
        {
            var query = @"
                UPDATE storage_locations 
                SET current_load = current_load + @load_change
                WHERE id = @location_id 
                  AND current_load + @load_change <= capacity";

            var parameters = new[]
            {
                new NpgsqlParameter("@location_id", NpgsqlDbType.Integer) { Value = locationId },
                new NpgsqlParameter("@load_change", NpgsqlDbType.Integer) { Value = loadChange }
            };

            return (query, parameters);
        }
    }
}