using Domain.Entity;
using Domain.Enums;
using Infrastructure.Identity;
using Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Seed
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, IConfiguration config)
        {
            // 1. LẤY CẤU HÌNH TỪ APPSETTINGS.JSON HOẶC BIẾN MÔI TRƯỜNG
            var adminEmail = config["AdminUser:DefaultEmail"];
            var adminPassword = config["AdminUser:DefaultPassword"];

            if (string.IsNullOrEmpty(adminEmail)) adminEmail = "admin@linkie.com";
            if (string.IsNullOrEmpty(adminPassword)) adminPassword = "Admin@123";

            var encryptedEmail = EncryptionHelper.EncryptDeterministic(adminEmail);

            var admin = await context.Users.FirstOrDefaultAsync(u => u.Email == encryptedEmail);
            if (admin == null)
            {
                admin = await context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
            }

            if (admin == null)
            {
                admin = new User
                {
                    Id = Guid.NewGuid(),
                    Name = EncryptionHelper.Encrypt("Admin"),
                    Email = encryptedEmail,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                    Role = UserRole.Admin,
                    CreatedAt = DateTime.UtcNow
                };
                await context.Users.AddAsync(admin);
            }
            else
            {
                admin.Role = UserRole.Admin;
                admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword);
                context.Users.Update(admin);
            }

            // 2. Seed sample event
            var hasEvent = await context.Events.AnyAsync();
            if (!hasEvent)
            {
                var sampleEvent = new Event
                {
                    Id = Guid.NewGuid(),
                    Name = "NIGHTS FESTIVAL",
                    Description = "Festival âm nhạc đêm đầu tiên của Linkie.",
                    StartTime = new DateTime(2026, 3, 1, 18, 0, 0, DateTimeKind.Utc),
                    EndTime = new DateTime(2026, 3, 1, 23, 59, 0, DateTimeKind.Utc),
                    Location = "TP. Hồ Chí Minh",
                    Status = EventStatus.Upcoming,
                    CreatedAt = DateTime.UtcNow
                };
                await context.Events.AddAsync(sampleEvent);
            }

            await context.SaveChangesAsync();
        }
    }
}