using Domain.Entity;
using Domain.Enums;
using Infrastructure.Identity;
using Infrastructure.Security;

namespace Infrastructure.Seed
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            var roles = new[]
            {
                (Role: UserRole.Admin,     Name: "Admin User",     Email: "admin@linkie.com",     Password: "Admin@123"),
                (Role: UserRole.Organizer, Name: "Organizer User", Email: "organizer@linkie.com", Password: "Organizer@123"),
                (Role: UserRole.Staff,     Name: "Staff User",     Email: "staff@linkie.com",     Password: "Staff@123"),
                (Role: UserRole.Attendee,  Name: "Attendee User",  Email: "attendee@linkie.com",  Password: "Attendee@123"),
                (Role: UserRole.LED,       Name: "LED User",       Email: "led@linkie.com",       Password: "Led@123"),
            };

            foreach (var (role, name, email, password) in roles)
            {
                var encryptedEmail = EncryptionHelper.EncryptDeterministic(email);

                var exists = context.Users.Any(u => u.Email == encryptedEmail);
                if (exists) continue;

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Name = EncryptionHelper.Encrypt(name),
                    Email = encryptedEmail,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    Role = role,
                    CreatedAt = DateTime.UtcNow,
                };

                context.Users.Add(user);
            }

            await context.SaveChangesAsync();
        }
    }
}