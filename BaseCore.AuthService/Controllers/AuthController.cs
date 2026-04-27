using Microsoft.AspNetCore.Mvc;
using BaseCore.Common;
using BaseCore.Repository;
using BaseCore.Services.Authen;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BaseCore.AuthService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly BaseCoreDbContext _context;
        private readonly IConfiguration _configuration;
        private const int TokenExpirationMinutes = 480; // 8 hours

        public AuthController(IUserService userService, BaseCoreDbContext context, IConfiguration configuration)
        {
            _userService = userService;
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Email/phone and password are required" });
            }

            var user = await _userService.Authenticate(request.Username, request.Password);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            var role = await ResolveRole(user);

            // Generate JWT token
            var token = TokenHelper.GenerateToken(
                _configuration["Jwt:SecretKey"] ?? "YourSecretKeyForAuthenticationShouldBeLongEnough",
                TokenExpirationMinutes,
                user.Id.ToString(),
                user.Email,
                role
            );

            return Ok(new LoginResponse
            {
                Token = token,
                UserId = user.Id.ToString(),
                Username = user.Email,
                Name = user.Name,
                Email = user.Email,
                Role = role,
                ExpiresIn = TokenExpirationMinutes * 60
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
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

            if (request.Password.Length < 6)
            {
                return BadRequest(new { message = "Password must be at least 6 characters" });
            }

            try
            {
                var user = new BaseCore.Entities.User
                {
                    Name = request.Name ?? request.Username,
                    Email = string.IsNullOrWhiteSpace(request.Email) ? request.Username : request.Email,
                    Phone = request.Phone,
                    UserType = 0 // Customer
                };

                var createdUser = await _userService.Create(user, request.Password);

                return Ok(new { message = "Registration successful", userId = createdUser.Id });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = "Registration failed: " + ex.Message });
            }
        }

        private async Task<string> ResolveRole(BaseCore.Entities.User user)
        {
            if (string.Equals(user.Email, "admin@autoshowroom.vn", System.StringComparison.OrdinalIgnoreCase))
            {
                return "Admin";
            }

            var connection = _context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            await using var command = connection.CreateCommand();
            command.CommandText = @"
SELECT TOP (1) vt.TenVaiTro
FROM dbo.NGUOIDUNG_VAITRO ur
JOIN dbo.VAITRO vt ON vt.MaVaiTro = ur.MaVaiTro
WHERE ur.MaNguoiDung = @userId
ORDER BY CASE vt.TenVaiTro WHEN 'Admin' THEN 1 WHEN 'Staff' THEN 2 ELSE 3 END";
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@userId";
            parameter.Value = user.Id;
            command.Parameters.Add(parameter);

            var dbRole = await command.ExecuteScalarAsync() as string;
            if (!string.IsNullOrWhiteSpace(dbRole))
            {
                return dbRole;
            }

            return user.UserType switch
            {
                1 => "Admin",
                2 => "Staff",
                _ => "Customer"
            };
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public string Token { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public int ExpiresIn { get; set; }
    }

    public class RegisterRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }

}
