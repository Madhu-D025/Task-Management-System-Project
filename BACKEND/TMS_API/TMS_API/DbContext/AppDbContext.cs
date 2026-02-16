using AuthApplication.Models;
using Microsoft.EntityFrameworkCore;
using TMS_API.Models;

namespace TMS_API.DBContext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Client> Clients { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRoleMap> UserRoleMaps { get; set; }
        public DbSet<App> Apps { get; set; }
        public DbSet<RoleAppMap> RoleAppMaps { get; set; }
        public DbSet<UserLoginHistory> UserLoginHistory { get; set; }
        public DbSet<AuthTokenHistory> AuthTokenHistories { get; set; }
        public DbSet<EmailConfiguration> EmailConfiguration { get; set; }
        public DbSet<otpConfiguration> otpConfiguration { get; set; }
        public DbSet<PasswordResetOtpHistory> PasswordResetOtpHistorys { get; set; }
        public DbSet<MailBodyConfiguration> MailBodyConfigurations { get; set; }
        public DbSet<NewsAndNotification> NewsAndNotifications { get; set; }
        public DbSet<DocumentMaster> DocumentMaster { get; set; }
        public DbSet<Masters> Masters { get; set; }
        public DbSet<DocumentExtensions> DocumentExtensions { get; set; }
        // 08-01-2026
        public DbSet<UserActivityLog> UserActivityLog { get; set; }
        public DbSet<Project> Project { get; set; }
        public DbSet<ProjectEmployees> ProjectEmployees { get; set; }
        public DbSet<TaskRelatedInfo> TaskRelatedInfo { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RoleAppMap>(
           build =>
           {
               build.HasKey(t => new { t.RoleID, t.AppID });
           });
            modelBuilder.Entity<UserRoleMap>(
            build =>
            {
                build.HasKey(t => new { t.UserID, t.RoleID });
            });

        }
    }
}
