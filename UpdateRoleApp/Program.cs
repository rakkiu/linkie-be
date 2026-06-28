using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Infrastructure.Identity;
using Infrastructure.Security;
using Domain.Entity;
using Domain.Enums;

namespace UpdateRoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            Console.WriteLine(">>> Starting Role Update Script...");
            
            // Load env variables
            string envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", ".env");
            if (!File.Exists(envPath))
            {
                envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
            }
            
            if (File.Exists(envPath))
            {
                foreach (var rawLine in File.ReadAllLines(envPath))
                {
                    var line = rawLine.Trim();
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                    var idx = line.IndexOf('=');
                    if (idx <= 0) continue;
                    var key = line[..idx].Trim();
                    var value = line[(idx + 1)..].Trim().Trim('"');
                    Environment.SetEnvironmentVariable(key, value);
                }
                Console.WriteLine("Loaded .env successfully.");
            }
            else
            {
                Console.WriteLine("Warning: .env file not found!");
            }

            string connStr = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
            string encKey = Environment.GetEnvironmentVariable("EnvironmentVariables__DATA_ENCRYPTION_KEY");

            if (string.IsNullOrEmpty(connStr))
            {
                Console.WriteLine("Error: DefaultConnection is null!");
                return;
            }
            if (string.IsNullOrEmpty(encKey))
            {
                Console.WriteLine("Error: DATA_ENCRYPTION_KEY is null!");
                return;
            }

            // Configure Encryption Key
            EncryptionHelper.ConfigureKey(encKey);

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(connStr);

            using (var context = new ApplicationDbContext(optionsBuilder.Options))
            {
                string targetEmail = "tahieunhann@gmail.com";
                string encryptedEmail = EncryptionHelper.EncryptDeterministic(targetEmail);
                
                Console.WriteLine($"Looking for encrypted email: {encryptedEmail}");

                var user = await context.Users.FirstOrDefaultAsync(u => u.Email == encryptedEmail);

                if (user != null)
                {
                    Console.WriteLine($"Found user: ID={user.Id}, Current Role={user.Role}");
                    user.Role = UserRole.Organizer;
                    context.Users.Update(user);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"Successfully updated role of user {targetEmail} to Organizer.");
                }
                else
                {
                    Console.WriteLine($"User with email {targetEmail} not found. Creating a new one with Organizer role...");
                    var newUser = new User
                    {
                        Id = Guid.NewGuid(),
                        Email = encryptedEmail,
                        Name = EncryptionHelper.Encrypt("Tạ Hiếu Nhân"),
                        Role = UserRole.Organizer,
                        IsEmailVerified = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    await context.Users.AddAsync(newUser);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"Successfully created new user {targetEmail} with Organizer role.");
                }
            }
            
            Console.WriteLine(">>> Finished.");
        }
    }
}
