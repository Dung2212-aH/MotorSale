using BaseCore.Common;
using BaseCore.Repository;
using BaseCore.Repository.Authen;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace BaseCore.UnitTest
{
    public class BaseConfigService
    {
        public IOptions<AppSettings> Option;
        public readonly IConfiguration ConfigurationRoot;
        protected readonly ServiceProvider ServiceProvider;

        public BaseConfigService()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            ConfigurationRoot = builder.Build();
            Option = Options.Create(new AppSettings
            {
                Secret = ""
            });

            var services = new ServiceCollection();
            services.AddDbContext<BaseCoreDbContext>(options =>
                options.UseSqlServer(ConfigurationRoot.GetConnectionString("DefaultConnection")));
            services.AddScoped<IUserRepository, UserRepository>();

            ServiceProvider = services.BuildServiceProvider();

            using var scope = ServiceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BaseCoreDbContext>();
            db.Database.Migrate();
            BaseCoreDbSeeder.SeedAsync(db).GetAwaiter().GetResult();
        }
    }
}
