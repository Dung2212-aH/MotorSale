using BaseCore.LogService.Entities;
using Microsoft.EntityFrameworkCore;

namespace BaseCore.LogService
{
    public class LogDbContext : DbContext
    {
        public LogDbContext(DbContextOptions<LogDbContext> options) : base(options)
        {
        }

        public DbSet<LogAction> LogActions => Set<LogAction>();
        public DbSet<LogError> LogErrors => Set<LogError>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<LogAction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(200);
                entity.Property(e => e.Action).HasMaxLength(200);
                entity.Property(e => e.IPAddress).HasMaxLength(100);
                entity.Property(e => e.LocalName).HasMaxLength(500);
                entity.Property(e => e.CreatedUser).HasMaxLength(200);
            });

            modelBuilder.Entity<LogError>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Header).HasMaxLength(1000);
                entity.Property(e => e.Message).HasMaxLength(4000);
                entity.Property(e => e.CreatedUser).HasMaxLength(200);
            });
        }
    }
}
