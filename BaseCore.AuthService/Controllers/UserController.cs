using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BaseCore.Entities;
using BaseCore.Repository;
using BaseCore.Services.Authen;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace BaseCore.AuthService.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly BaseCoreDbContext _context;

        public UserController(IUserService userService, BaseCoreDbContext context)
        {
            _userService = userService;
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] string keyword = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var (users, totalCount) = await _userService.Search(keyword, page, pageSize);

            var result = users.Select(u => new UserResponse
            {
                Id = u.Id,
                Username = u.Email,
                Name = u.Name,
                Email = u.Email,
                Phone = u.Phone,
                Position = u.Position,
                IsActive = u.IsActive,
                UserType = u.UserType,
                Created = u.Created
            });

            return Ok(new
            {
                data = result,
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userService.GetById(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(new UserResponse
            {
                Id = user.Id,
                Username = user.Email,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Position = user.Position,
                IsActive = user.IsActive,
                UserType = user.UserType,
                Created = user.Created
            });
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            return Ok(ToResponse(user));
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe([FromBody] UpdateMeRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Invalid request" });
            }

            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Phone))
            {
                return BadRequest(new { message = "Name, email and phone are required" });
            }

            if (!IsValidEmail(request.Email))
            {
                return BadRequest(new { message = "Email is invalid" });
            }

            if (!IsValidPhone(request.Phone))
            {
                return BadRequest(new { message = "Phone number must be 10 digits and start with 0" });
            }

            user.Name = request.Name.Trim();
            user.Email = request.Email.Trim();
            user.Phone = request.Phone.Trim();

            try
            {
                await _userService.Update(user);
                return Ok(ToResponse(user));
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpPut("me/password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Invalid request" });
            }

            if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new { message = "Current and new passwords are required" });
            }

            if (request.NewPassword.Length < 6)
            {
                return BadRequest(new { message = "New password must be at least 6 characters" });
            }

            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            var authenticated = await _userService.Authenticate(user.Email, request.CurrentPassword);
            if (authenticated == null || authenticated.Id != user.Id)
            {
                return BadRequest(new { message = "Current password is incorrect" });
            }

            await _userService.Update(user, request.NewPassword);
            return Ok(new { message = "Password updated successfully" });
        }

        [HttpGet("me/address")]
        public async Task<IActionResult> GetMyAddress()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            await EnsureAddressTableAsync();
            var address = await ReadAddressAsync(user.Id);
            return Ok(address ?? new UserAddressResponse { UserId = user.Id });
        }

        [HttpPut("me/address")]
        public async Task<IActionResult> UpdateMyAddress([FromBody] UpdateUserAddressRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Invalid request" });
            }

            if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.PhoneNumber) || string.IsNullOrWhiteSpace(request.AddressLine) || string.IsNullOrWhiteSpace(request.Province))
            {
                return BadRequest(new { message = "Full name, phone, address line and province are required" });
            }

            if (!IsValidPhone(request.PhoneNumber))
            {
                return BadRequest(new { message = "Phone number must be 10 digits and start with 0" });
            }

            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            await EnsureAddressTableAsync();
            var now = DateTime.UtcNow;
            var defaultFlag = request.IsDefault ? 1 : 0;

            await _context.Database.ExecuteSqlInterpolatedAsync($@"
MERGE dbo.NGUOIDUNG_DIACHI AS target
USING (SELECT {user.Id} AS MaNguoiDung) AS source
ON target.MaNguoiDung = source.MaNguoiDung
WHEN MATCHED THEN
    UPDATE SET
        HoTenNhanHang = {request.FullName.Trim()},
        SoDienThoaiNhanHang = {request.PhoneNumber.Trim()},
        DiaChiNhanHang = {request.AddressLine.Trim()},
        PhuongXa = {request.Ward},
        TinhThanh = {request.Province.Trim()},
        GhiChu = {request.Note},
        LaMacDinh = {defaultFlag},
        NgayCapNhat = {now}
WHEN NOT MATCHED THEN
    INSERT (MaNguoiDung, HoTenNhanHang, SoDienThoaiNhanHang, DiaChiNhanHang, PhuongXa, TinhThanh, GhiChu, LaMacDinh, NgayTao, NgayCapNhat)
    VALUES ({user.Id}, {request.FullName.Trim()}, {request.PhoneNumber.Trim()}, {request.AddressLine.Trim()}, {request.Ward}, {request.Province.Trim()}, {request.Note}, {defaultFlag}, {now}, {now});");

            var address = await ReadAddressAsync(user.Id);
            return Ok(address);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Invalid request" });
            }

            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Email/phone and password are required" });
            }

            if (string.IsNullOrEmpty(request.Email) && !request.Username.Contains('@'))
            {
                return BadRequest(new { message = "Email is required when username is not an email address" });
            }

            if (string.IsNullOrEmpty(request.Phone))
            {
                return BadRequest(new { message = "Phone is required" });
            }

            try
            {
                var user = new User
                {
                    Name = request.Name ?? request.Username,
                    Email = string.IsNullOrWhiteSpace(request.Email) ? request.Username : request.Email,
                    Phone = request.Phone,
                    Position = request.Position,
                    UserType = request.UserType
                };

                var createdUser = await _userService.Create(user, request.Password);

                return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, new UserResponse
                {
                    Id = createdUser.Id,
                    Username = createdUser.Email,
                    Name = createdUser.Name,
                    Email = createdUser.Email,
                    Phone = createdUser.Phone,
                    Position = createdUser.Position,
                    IsActive = createdUser.IsActive,
                    UserType = createdUser.UserType,
                    Created = createdUser.Created
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to create user: " + ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Invalid request" });
            }

            var existingUser = await _userService.GetById(id);
            if (existingUser == null)
            {
                return NotFound(new { message = "User not found" });
            }

            existingUser.Name = request.Name ?? existingUser.Name;
            existingUser.Email = request.Email ?? existingUser.Email;
            existingUser.Phone = request.Phone ?? existingUser.Phone;
            existingUser.Position = request.Position ?? existingUser.Position;
            existingUser.UserType = request.UserType ?? existingUser.UserType;
            existingUser.IsActive = request.IsActive ?? existingUser.IsActive;

            await _userService.Update(existingUser, request.Password);

            return Ok(new UserResponse
            {
                Id = existingUser.Id,
                Username = existingUser.Email,
                Name = existingUser.Name,
                Email = existingUser.Email,
                Phone = existingUser.Phone,
                Position = existingUser.Position,
                IsActive = existingUser.IsActive,
                UserType = existingUser.UserType,
                Created = existingUser.Created
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var existingUser = await _userService.GetById(id);
            if (existingUser == null)
            {
                return NotFound(new { message = "User not found" });
            }

            await _userService.Delete(id);
            return NoContent();
        }

    private async Task<User?> GetCurrentUserAsync()
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdValue, out var userId))
        {
            return null;
        }

        return await _userService.GetById(userId);
    }

    private static UserResponse ToResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Username = user.Email,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            Position = user.Position,
            IsActive = user.IsActive,
            UserType = user.UserType,
            Created = user.Created
        };
    }

    private async Task EnsureAddressTableAsync()
    {
        var sql = @"
IF OBJECT_ID(N'dbo.NGUOIDUNG_DIACHI', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NGUOIDUNG_DIACHI (
        MaDiaChi INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        MaNguoiDung INT NOT NULL,
        HoTenNhanHang NVARCHAR(150) NOT NULL,
        SoDienThoaiNhanHang NVARCHAR(20) NOT NULL,
        DiaChiNhanHang NVARCHAR(255) NOT NULL,
        PhuongXa NVARCHAR(100) NULL,
        TinhThanh NVARCHAR(100) NOT NULL,
        GhiChu NVARCHAR(255) NULL,
        LaMacDinh BIT NOT NULL CONSTRAINT DF_NGUOIDUNG_DIACHI_LaMacDinh DEFAULT(1),
        NgayTao DATETIME2(0) NOT NULL CONSTRAINT DF_NGUOIDUNG_DIACHI_NgayTao DEFAULT SYSUTCDATETIME(),
        NgayCapNhat DATETIME2(0) NOT NULL CONSTRAINT DF_NGUOIDUNG_DIACHI_NgayCapNhat DEFAULT SYSUTCDATETIME()
    );

    CREATE UNIQUE INDEX UX_NGUOIDUNG_DIACHI_MacDinh
        ON dbo.NGUOIDUNG_DIACHI(MaNguoiDung)
        WHERE LaMacDinh = 1;
END";

        await _context.Database.ExecuteSqlRawAsync(sql);
    }

    private async Task<UserAddressResponse?> ReadAddressAsync(int userId)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        await using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT TOP (1)
    MaNguoiDung,
    HoTenNhanHang,
    SoDienThoaiNhanHang,
    DiaChiNhanHang,
    PhuongXa,
    TinhThanh,
    GhiChu,
    LaMacDinh,
    NgayTao,
    NgayCapNhat
FROM dbo.NGUOIDUNG_DIACHI
WHERE MaNguoiDung = @userId
ORDER BY LaMacDinh DESC, NgayCapNhat DESC, MaDiaChi DESC";

        var parameter = command.CreateParameter();
        parameter.ParameterName = "@userId";
        parameter.Value = userId;
        command.Parameters.Add(parameter);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new UserAddressResponse
        {
            UserId = reader.GetInt32(0),
            FullName = reader.GetString(1),
            PhoneNumber = reader.GetString(2),
            AddressLine = reader.GetString(3),
            Ward = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
            Province = reader.GetString(5),
            Note = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
            IsDefault = !reader.IsDBNull(7) && reader.GetBoolean(7),
            CreatedAt = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
            UpdatedAt = reader.IsDBNull(9) ? null : reader.GetDateTime(9)
        };
    }

    private static bool IsValidEmail(string value)
    {
        return Regex.IsMatch(value.Trim(), @"^[^\s@]+@[^\s@]+\.[^\s@]+$");
    }

    private static bool IsValidPhone(string value)
    {
        return Regex.IsMatch(value.Trim(), @"^0\d{9}$");
    }
    }

    public class UserResponse
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Position { get; set; }
        public bool IsActive { get; set; }
        public int UserType { get; set; }
        public DateTime Created { get; set; }
    }

    public class CreateUserRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Position { get; set; }
        public int UserType { get; set; }
    }

    public class UpdateUserRequest
    {
        public string Password { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Position { get; set; }
        public int? UserType { get; set; }
        public bool? IsActive { get; set; }
    }

    public class UpdateMeRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class UserAddressResponse
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string AddressLine { get; set; }
        public string Ward { get; set; }
        public string Province { get; set; }
        public string Note { get; set; }
        public bool IsDefault { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class UpdateUserAddressRequest
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string AddressLine { get; set; }
        public string Ward { get; set; }
        public string Province { get; set; }
        public string Note { get; set; }
        public bool IsDefault { get; set; } = true;
    }
}
