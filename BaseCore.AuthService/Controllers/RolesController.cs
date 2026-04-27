using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BaseCore.Repository;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace BaseCore.AuthService.Controllers
{
    /// <summary>
    /// Roles API Controller
    /// Teaching: Role-based Authorization (Bài 10, 11)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class RolesController : ControllerBase
    {
        private readonly BaseCoreDbContext _context;

        public RolesController(BaseCoreDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all roles
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await LoadRolesAsync());
        }

        /// <summary>
        /// Get role by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var role = (await LoadRolesAsync()).Find(r => r.Id == id);
            if (role == null)
                return NotFound(new { message = "Role not found" });

            return Ok(role);
        }

        /// <summary>
        /// Get role by UserType
        /// </summary>
        [HttpGet("by-usertype/{userType}")]
        public async Task<IActionResult> GetByUserType(int userType)
        {
            var role = (await LoadRolesAsync()).Find(r => r.UserType == userType);
            if (role == null)
                return NotFound(new { message = "Role not found for this UserType" });

            return Ok(role);
        }

        /// <summary>
        /// Get permissions for a role
        /// </summary>
        [HttpGet("{id}/permissions")]
        public async Task<IActionResult> GetPermissions(int id)
        {
            var role = (await LoadRolesAsync()).Find(r => r.Id == id);
            if (role == null)
                return NotFound(new { message = "Role not found" });

            // Define permissions based on role
            var permissions = role.Name switch
            {
                "Admin" => new[] { "users.read", "users.write", "users.delete", "products.read", "products.write", "products.delete", "orders.read", "orders.write", "orders.delete", "payments.read", "payments.write", "categories.read", "categories.write", "roles.read" },
                "Staff" => new[] { "users.read", "products.read", "products.write", "orders.read", "orders.write", "payments.read", "payments.write", "categories.read" },
                _ => new[] { "products.read", "orders.read", "categories.read" }
            };

            return Ok(new
            {
                role = role.Name,
                permissions
            });
        }

        private async Task<List<RoleDto>> LoadRolesAsync()
        {
            var roles = new List<RoleDto>();
            var connection = _context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT MaVaiTro, TenVaiTro, MoTa FROM dbo.VAITRO ORDER BY MaVaiTro";
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var name = reader.GetString(1);
                roles.Add(new RoleDto
                {
                    Id = reader.GetByte(0),
                    Name = name,
                    Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    UserType = name == "Admin" ? 1 : name == "Staff" ? 2 : 0
                });
            }

            return roles;
        }
    }

    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int UserType { get; set; }
    }
}
