using Microsoft.EntityFrameworkCore;

namespace BaseCore.Repository
{
    public static class BaseCoreDbSeeder
    {
        public static async Task SeedAsync(BaseCoreDbContext context, bool recreateWhenSchemaMismatch = false)
        {
            var schemaCheck = await GetShowroomSchemaCheckAsync(context);
            if (schemaCheck.MissingTables.Count > 0)
            {
                throw new InvalidOperationException(
                    "Database does not match Database/ShowroomDB.sql. " +
                    $"Connected database: {schemaCheck.DatabaseName}. " +
                    $"Missing tables: {string.Join(", ", schemaCheck.MissingTables)}. " +
                    "The application will not create, delete, or rebuild schema automatically. " +
                    "Run the approved ShowroomDB SQL script against the configured database.");
            }
        }

        private static async Task<(string DatabaseName, List<string> MissingTables)> GetShowroomSchemaCheckAsync(BaseCoreDbContext context)
        {
            var requiredTables = new[]
            {
                "NGUOIDUNG",
                "DANHMUC",
                "HANGXE",
                "DONGXE",
                "SHOWROOM",
                "SANPHAM",
                "BIENSANPHAM",
                "ANHSANPHAM",
                "PHUTUNG_TUONGTHICH",
                "TONKHO_GIUCHO",
                "GIOHANG",
                "CHITIET_GIOHANG",
                "DONHANG",
                "CHITIET_DONHANG",
                "THANHTOAN",
                "THANHTOAN_HOANTIEN",
                "VOUCHER",
                "VOUCHER_DANHMUC",
                "VOUCHER_HANGXE",
                "VOUCHER_SANPHAM",
                "DONHANG_VOUCHER",
                "LIENHE_YEUCAU",
                "BAIVIET",
                "FAQ",
                "DANHGIASANPHAM",
                "YEUTHICH",
                "VAITRO",
                "NGUOIDUNG_VAITRO"
            };

            var connection = context.Database.GetDbConnection();

            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            await using var command = connection.CreateCommand();
            command.CommandText = $@"
            SELECT TABLE_NAME
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_TYPE = 'BASE TABLE'
            AND TABLE_NAME IN ({string.Join(",", requiredTables.Select((_, index) => $"@p{index}"))})";

            for (var i = 0; i < requiredTables.Length; i++)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = $"@p{i}";
                parameter.Value = requiredTables[i];
                command.Parameters.Add(parameter);
            }

            var existingTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                existingTables.Add(reader.GetString(0));
            }

            var missingTables = requiredTables
                .Where(table => !existingTables.Contains(table))
                .ToList();

            return (connection.Database, missingTables);
        }
    }
}
