using BaseCore.Entities;
using Microsoft.EntityFrameworkCore;

namespace BaseCore.Repository.Authen
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByIdAsync(int id);
        Task<List<User>> GetAllAsync();
        Task CreateAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(int id);
        Task<(List<User> Users, int TotalCount)> SearchAsync(string keyword, int page, int pageSize);
    }

    public class UserRepository : IUserRepository
    {
        private readonly BaseCoreDbContext _context;

        public UserRepository(BaseCoreDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => (u.Email == username || u.Phone == username) && u.IsActive);
            await PopulateUserTypeAsync(user);
            return user;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);
            await PopulateUserTypeAsync(user);
            return user;
        }

        public async Task<List<User>> GetAllAsync()
        {
            var users = await _context.Users
                .Where(u => u.IsActive)
                .ToListAsync();
            await PopulateUserTypesAsync(users);
            return users;
        }

        public async Task CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await SyncUserRoleAsync(user.Id, user.UserType);
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            await SyncUserRoleAsync(user.Id, user.UserType);
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return;
            }

            user.IsActive = false;
            await _context.SaveChangesAsync();
        }

        public async Task<(List<User> Users, int TotalCount)> SearchAsync(string keyword, int page, int pageSize)
        {
            var query = _context.Users.Where(u => u.IsActive);

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Trim().ToLower();
                query = query.Where(u =>
                    (u.Name != null && u.Name.ToLower().Contains(keyword)) ||
                    (u.Email != null && u.Email.ToLower().Contains(keyword)) ||
                    (u.Phone != null && u.Phone.ToLower().Contains(keyword)));
            }

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderByDescending(u => u.Created)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            await PopulateUserTypesAsync(users);

            return (users, totalCount);
        }

        private async Task PopulateUserTypeAsync(User? user)
        {
            if (user == null)
            {
                return;
            }

            user.UserType = await GetUserTypeAsync(user.Id);
        }

        private async Task PopulateUserTypesAsync(List<User> users)
        {
            foreach (var user in users)
            {
                user.UserType = await GetUserTypeAsync(user.Id);
            }
        }

        private async Task<int> GetUserTypeAsync(int userId)
        {
            var roleName = await _context.UserRoleAssignments
                .Where(assignment => assignment.UserId == userId)
                .Join(
                    _context.SystemRoles,
                    assignment => assignment.RoleId,
                    role => role.Id,
                    (assignment, role) => role.Name)
                .OrderBy(name => name == "Admin" ? 1 : name == "Staff" ? 2 : 3)
                .FirstOrDefaultAsync();

            return roleName switch
            {
                "Admin" => 1,
                "Staff" => 2,
                _ => 0
            };
        }

        private async Task SyncUserRoleAsync(int userId, int userType)
        {
            var roleName = userType switch
            {
                1 => "Admin",
                2 => "Staff",
                _ => "Customer"
            };

            await _context.Database.ExecuteSqlInterpolatedAsync($@"
DELETE FROM dbo.NGUOIDUNG_VAITRO
WHERE MaNguoiDung = {userId};

INSERT INTO dbo.NGUOIDUNG_VAITRO (MaNguoiDung, MaVaiTro, NgayTao)
SELECT {userId}, vt.MaVaiTro, SYSUTCDATETIME()
FROM dbo.VAITRO vt
WHERE vt.TenVaiTro = {roleName};");
        }
    }
}
