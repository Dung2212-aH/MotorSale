using BaseCore.Common;
using BaseCore.Entities;
using Microsoft.EntityFrameworkCore;

namespace BaseCore.Repository
{
    public static class BaseCoreDbSeeder
    {
        public static async Task SeedAsync(BaseCoreDbContext context, bool recreateWhenSchemaMismatch = false)
        {
            var created = await context.Database.EnsureCreatedAsync();
            if (!created && !await HasRequiredShowroomSchemaAsync(context))
            {
                throw new InvalidOperationException(
                    "Database exists but does not match the Auto Showroom schema. " +
                    "The application will not delete or rebuild an existing database automatically. " +
                    "Create a new database, run the SQL schema script, or run a controlled EF migration.");
            }

            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@autoshowroom.vn");
            if (adminUser == null)
            {
                byte[] salt;
                var hashedPassword = TokenHelper.HashPassword("admin123", out salt);

                context.Users.Add(new User
                {
                    Password = $"{Convert.ToBase64String(salt)}:{hashedPassword}",
                    Name = "Auto Showroom Admin",
                    Email = "admin@autoshowroom.vn",
                    Phone = "0900000001",
                    IsActive = true,
                    Created = DateTime.UtcNow,
                });

                await context.SaveChangesAsync();
                return;
            }

            if (!adminUser.Password.Contains(':'))
            {
                byte[] salt;
                var hashedPassword = TokenHelper.HashPassword("admin123", out salt);
                adminUser.Password = $"{Convert.ToBase64String(salt)}:{hashedPassword}";
                adminUser.IsActive = true;
                await context.SaveChangesAsync();
            }
        }

        private static async Task<bool> HasRequiredShowroomSchemaAsync(BaseCoreDbContext context)
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
                "GIOHANG",
                "CHITIET_GIOHANG",
                "DONHANG",
                "CHITIET_DONHANG",
                "THANHTOAN",
                "VOUCHER",
                "LIENHE_YEUCAU",
                "BAIVIET",
                "FAQ"
            };

            var connection = context.Database.GetDbConnection();

            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            await using var command = connection.CreateCommand();
            command.CommandText = $@"
SELECT COUNT(*)
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

            var result = await command.ExecuteScalarAsync();
            var existingCount = Convert.ToInt32(result);
            return existingCount == requiredTables.Length;
        }
    }
}
