using BaseCore.Common;
using BaseCore.Entities;
using BaseCore.Repository.Authen;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BaseCore.Services.Authen
{
    public interface IUserService
    {
        Task<User?> Authenticate(string username, string password);
        Task<List<User>> GetAll();
        Task<User?> GetById(int id);
        Task<User> Create(User user, string password);
        Task Update(User user, string password = null);
        Task Delete(int id);
        Task<(List<User> Users, int TotalCount)> Search(string keyword, int page, int pageSize);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User?> Authenticate(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return null;

            var user = await _userRepository.GetByUsernameAsync(username);

            // check if username exists
            if (user == null)
                return null;

            // verify password using hash or plain text
            bool isValidPassword;

            var passwordParts = user.Password?.Split(':', 2);
            if (passwordParts?.Length == 2 &&
                Convert.TryFromBase64String(passwordParts[0], new byte[16], out var bytesWritten) &&
                bytesWritten > 0)
            {
                var salt = Convert.FromBase64String(passwordParts[0]);
                isValidPassword = TokenHelper.IsValidPassword(password, salt, passwordParts[1]);
            }
            else
            {
                // Plain text password (for seeded/legacy users)
                isValidPassword = (user.Password == password);
            }

            if (!isValidPassword)
            {
                Console.WriteLine($"Password verification failed for user: {username}");
                return null;
            }

            Console.WriteLine($"User authenticated successfully: {username}");

            // authentication successful
            return user;
        }

        public async Task<List<User>> GetAll()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<User?> GetById(int id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task<User> Create(User user, string password)
        {
            // Hash password with salt
            byte[] salt;
            var hashedPassword = TokenHelper.HashPassword(password, out salt);
            user.Password = $"{Convert.ToBase64String(salt)}:{hashedPassword}";
            user.Created = DateTime.UtcNow;
            user.IsActive = true;

            await _userRepository.CreateAsync(user);
            return user;
        }

        public async Task Update(User user, string password = null)
        {
            if (!string.IsNullOrEmpty(password))
            {
                byte[] salt;
                var hashedPassword = TokenHelper.HashPassword(password, out salt);
                user.Password = $"{Convert.ToBase64String(salt)}:{hashedPassword}";
            }
            await _userRepository.UpdateAsync(user);
        }

        public async Task Delete(int id)
        {
            await _userRepository.DeleteAsync(id);
        }

        public async Task<(List<User> Users, int TotalCount)> Search(string keyword, int page, int pageSize)
        {
            return await _userRepository.SearchAsync(keyword, page, pageSize);
        }
    }
}
