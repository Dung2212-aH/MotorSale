using BaseCore.Common;
using BaseCore.Entities;
using BaseCore.Repository.Authen;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace BaseCore.UnitTest
{
    public class UnitTestUserService : BaseConfigService
    {
        [Test]
        public async Task TestInsertUserSuccess()
        {
            using var scope = ServiceProvider.CreateScope();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            var salt = new byte[128 / 8];
            var password = TokenHelper.HashPassword("123456", out salt);
            var userName = $"test_{Guid.NewGuid():N}@example.com";
            var user = new User
            {
                Name = "Vu Tuan",
                UserName = userName,
                Contact = "Duong Noi, Ha Dong",
                Password = password,
                Salt = salt,
                Created = DateTime.UtcNow,
                Email = userName,
                Phone = "0919901195",
                IsActive = true,
                Position = "tester"
            };

            await userRepository.CreateAsync(user);
            var created = await userRepository.GetByUsernameAsync(userName);

            Assert.That(created, Is.Not.Null);
            Assert.That(created!.UserName, Is.EqualTo(userName));

            await userRepository.DeleteAsync(created.Id);
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }
    }
}
