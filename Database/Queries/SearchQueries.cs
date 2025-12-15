using Npgsql;
using NpgsqlTypes;

namespace FormsDB.Database.Queries
{
    public static class SearchQueries
    {
        public static (string, NpgsqlParameter[]) SearchCustomers(string searchTerm)
        {
            var query = @"
                SELECT * FROM customers 
                WHERE name ILIKE @search 
                   OR address ILIKE @search 
                   OR phone ILIKE @search 
                   OR email ILIKE @search 
                   OR tax_id ILIKE @search 
                ORDER BY name";

            var parameters = new[]
            {
                new NpgsqlParameter("@search", NpgsqlDbType.Varchar)
                {
                    Value = $"%{searchTerm}%"
                }
            };

            return (query, parameters);
        }

        public static (string, NpgsqlParameter[]) SearchProducts(string searchTerm)
        {
            var query = @"
                SELECT p.*, s.name as supplier_name 
                FROM products p
                LEFT JOIN suppliers s ON p.supplier_id = s.id
                WHERE p.name ILIKE @search 
                   OR p.description ILIKE @search 
                ORDER BY p.name";

            var parameters = new[]
            {
                new NpgsqlParameter("@search", NpgsqlDbType.Varchar)
                {
                    Value = $"%{searchTerm}%"
                }
            };

            return (query, parameters);
        }

        public static (string, NpgsqlParameter[]) SearchSuppliers(string searchTerm)
        {
            var query = @"
                SELECT * FROM suppliers 
                WHERE name ILIKE @search 
                   OR address ILIKE @search 
                   OR phone ILIKE @search 
                   OR email ILIKE @search 
                   OR tax_id ILIKE @search 
                ORDER BY name";

            var parameters = new[]
            {
                new NpgsqlParameter("@search", NpgsqlDbType.Varchar)
                {
                    Value = $"%{searchTerm}%"
                }
            };

            return (query, parameters);
        }

        public static (string, NpgsqlParameter[]) AdvancedSearch(
            string tableName,
            Dictionary<string, string> filters)
        {
            var conditions = new List<string>();
            var parameters = new List<NpgsqlParameter>();
            var paramIndex = 0;

            foreach (var filter in filters)
            {
                if (!string.IsNullOrEmpty(filter.Value))
                {
                    conditions.Add($"{filter.Key} ILIKE @param{paramIndex}");
                    parameters.Add(new NpgsqlParameter($"@param{paramIndex}",
                        NpgsqlDbType.Varchar)
                    { Value = $"%{filter.Value}%" });
                    paramIndex++;
                }
            }

            var whereClause = conditions.Count > 0
                ? $"WHERE {string.Join(" AND ", conditions)}"
                : "";

            var query = $"SELECT * FROM {tableName} {whereClause} ORDER BY id";

            return (query, parameters.ToArray());
        }

        public static (string, NpgsqlParameter[]) SearchInvoicesByDateRange(
            DateTime startDate,
            DateTime endDate,
            string customerName = "")
        {
            var conditions = new List<string>();
            var parameters = new List<NpgsqlParameter>();

            conditions.Add("invoice_date BETWEEN @start_date AND @end_date");
            parameters.Add(new NpgsqlParameter("@start_date",
                NpgsqlDbType.Date)
            { Value = startDate });
            parameters.Add(new NpgsqlParameter("@end_date",
                NpgsqlDbType.Date)
            { Value = endDate });

            if (!string.IsNullOrEmpty(customerName))
            {
                conditions.Add("EXISTS (SELECT 1 FROM customers c WHERE c.id = i.customer_id AND c.name ILIKE @customer_name)");
                parameters.Add(new NpgsqlParameter("@customer_name",
                    NpgsqlDbType.Varchar)
                { Value = $"%{customerName}%" });
            }

            var query = $@"
                SELECT i.*, c.name as customer_name 
                FROM invoices i
                LEFT JOIN customers c ON i.customer_id = c.id
                WHERE {string.Join(" AND ", conditions)}
                ORDER BY i.invoice_date DESC";

            return (query, parameters.ToArray());
        }
    }
}