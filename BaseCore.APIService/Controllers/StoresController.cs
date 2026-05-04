using BaseCore.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace BaseCore.APIService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoresController : ControllerBase
    {
        private readonly BaseCoreDbContext _context;

        public StoresController(BaseCoreDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var stores = await LoadStoresAsync();
            var sortedStores = stores
                .OrderBy(s => s.City, StringComparer.Create(new System.Globalization.CultureInfo("vi-VN"), ignoreCase: true))
                .ThenBy(s => s.Name, StringComparer.Create(new System.Globalization.CultureInfo("vi-VN"), ignoreCase: true))
                .ToList();

            return Ok(new
            {
                items = sortedStores,
                totalCount = sortedStores.Count
            });
        }

        private async Task<List<StoreDto>> LoadStoresAsync()
        {
            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            var columns = await GetShowroomColumnsAsync(connection);
            var latitudeColumn = ResolveOptionalColumn(columns, "Latitude", "ViDo");
            var longitudeColumn = ResolveOptionalColumn(columns, "Longitude", "KinhDo");

            await using var command = connection.CreateCommand();
            command.CommandText = $@"
SELECT
    [MaShowroom],
    [TenShowroom],
    [Slug],
    [DiaChi],
    [SoDienThoai],
    [Email],
    [GioMoCua],
    [DangHoatDong],
    {latitudeColumn} AS [Latitude],
    {longitudeColumn} AS [Longitude]
FROM [dbo].[SHOWROOM]
WHERE [DangHoatDong] = CAST(1 AS bit);";

            var stores = new List<StoreDto>();
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var address = ReadString(reader, "DiaChi");
                var (city, district) = ParseAddressParts(address);
                var phoneNumber = ReadString(reader, "SoDienThoai");

                stores.Add(new StoreDto
                {
                    Id = reader.GetInt32(reader.GetOrdinal("MaShowroom")),
                    Name = ReadString(reader, "TenShowroom"),
                    Slug = ReadString(reader, "Slug"),
                    Address = address,
                    AddressLine = address,
                    City = city,
                    Province = city,
                    District = district,
                    PhoneNumber = phoneNumber,
                    Sdt = phoneNumber,
                    Email = ReadNullableString(reader, "Email"),
                    OpeningHours = ReadNullableString(reader, "GioMoCua"),
                    Latitude = ReadNullableDecimal(reader, "Latitude"),
                    Longitude = ReadNullableDecimal(reader, "Longitude"),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("DangHoatDong"))
                });
            }

            return stores;
        }

        private static async Task<HashSet<string>> GetShowroomColumnsAsync(System.Data.Common.DbConnection connection)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = @"
SELECT [COLUMN_NAME]
FROM [INFORMATION_SCHEMA].[COLUMNS]
WHERE [TABLE_SCHEMA] = N'dbo'
  AND [TABLE_NAME] = N'SHOWROOM';";

            var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                columns.Add(reader.GetString(0));
            }

            return columns;
        }

        private static string ResolveOptionalColumn(HashSet<string> columns, string primaryName, string legacyName)
        {
            if (columns.Contains(primaryName))
            {
                return $"[{primaryName}]";
            }

            if (columns.Contains(legacyName))
            {
                return $"[{legacyName}]";
            }

            return "CAST(NULL AS decimal(9, 6))";
        }

        private static string ReadString(System.Data.Common.DbDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? "" : reader.GetString(ordinal).Trim();
        }

        private static string? ReadNullableString(System.Data.Common.DbDataReader reader, string columnName)
        {
            var value = ReadString(reader, columnName);
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        private static decimal? ReadNullableDecimal(System.Data.Common.DbDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }

            var value = reader.GetValue(ordinal);
            return value switch
            {
                decimal decimalValue => decimalValue,
                double doubleValue => Convert.ToDecimal(doubleValue),
                float floatValue => Convert.ToDecimal(floatValue),
                _ => decimal.TryParse(Convert.ToString(value), out var parsedValue) ? parsedValue : null
            };
        }

        private static (string City, string? District) ParseAddressParts(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                return ("", null);
            }

            var parts = address
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(part => !string.IsNullOrWhiteSpace(part))
                .ToList();

            if (parts.Count == 0)
            {
                return ("", null);
            }

            var city = parts[^1];
            var district = parts
                .Take(Math.Max(parts.Count - 1, 0))
                .Reverse()
                .FirstOrDefault(IsDistrictPart);

            return (city, district);
        }

        private static bool IsDistrictPart(string value)
        {
            return value.Contains("Quận", StringComparison.OrdinalIgnoreCase)
                || value.Contains("Huyện", StringComparison.OrdinalIgnoreCase)
                || value.Contains("Thị xã", StringComparison.OrdinalIgnoreCase)
                || value.Contains("Thành phố", StringComparison.OrdinalIgnoreCase)
                || value.StartsWith("TP ", StringComparison.OrdinalIgnoreCase)
                || value.StartsWith("TP.", StringComparison.OrdinalIgnoreCase);
        }
    }

    public class StoreDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
        public string Address { get; set; } = "";
        public string AddressLine { get; set; } = "";
        public string City { get; set; } = "";
        public string Province { get; set; } = "";
        public string? District { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Sdt { get; set; }
        public string? Email { get; set; }
        public string? OpeningHours { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public bool IsActive { get; set; }
    }
}
