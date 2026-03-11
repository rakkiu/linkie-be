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
            var seedAccounts = new[]
            {
                (Role: UserRole.Admin,     Name: "Admin User",     Email: "admin@linkie.com",     Password: "Admin@123"),
                (Role: UserRole.Organizer, Name: "Organizer User", Email: "organizer@linkie.com", Password: "Organizer@123"),
                (Role: UserRole.Staff,     Name: "Staff User",     Email: "staff@linkie.com",     Password: "Staff@123"),
                (Role: UserRole.Attendee,  Name: "Attendee User",  Email: "attendee@linkie.com",  Password: "Attendee@123"),
                (Role: UserRole.LED,       Name: "LED User",       Email: "led@linkie.com",       Password: "Led@123"),
            };

            foreach (var (role, name, email, password) in seedAccounts)
            {
                var encryptedEmail = EncryptionHelper.EncryptDeterministic(email);
                var exists = await context.Users.AnyAsync(u => u.Email == encryptedEmail);
                if (exists) continue;

                context.Users.Add(new User
                {
                    Id = Guid.NewGuid(),
                    Name = EncryptionHelper.Encrypt(name),
                    Email = encryptedEmail,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    Role = role,
                    CreatedAt = DateTime.UtcNow,
                });
            }

            // Seed sample event
            if (!await context.Events.AnyAsync())
            {
                context.Events.Add(new Event
                {
                    Id = Guid.NewGuid(),
                    Name = "NIGHTS FESTIVAL",
                    Description = "Festival âm nhạc đêm đầu tiên của Linkie.",
                    StartTime = new DateTime(2026, 3, 1, 18, 0, 0, DateTimeKind.Utc),
                    EndTime = new DateTime(2026, 3, 1, 23, 59, 0, DateTimeKind.Utc),
                    Location = "TP. Hồ Chí Minh",
                    Status = EventStatus.Upcoming,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await context.SaveChangesAsync();
        }
    }
}
