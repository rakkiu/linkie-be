using Infrastructure.Identity;
using Infrastructure.Security;
using Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Presentation.Extentions;
using Presentation.Hubs;
using Presentation.Middlewares;

namespace Presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Fix lỗi DateTime Unspecified của PostgreSQL
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            LoadEnvFile();

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAppServices(builder.Configuration);

            // SignalR
            builder.Services.AddSignalR();

            // CORS — AllowCredentials is required for SignalR WebSocket/SSE
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend",
                    b => b.WithOrigins(
                              "http://localhost:5173",
                              "https://localhost:5173",
                              "http://localhost:3000")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials());
            });

            builder.Services.AddControllers();
            builder.Services.AddSwaggerWithJwt();

            var app = builder.Build();

            // --- TỰ ĐỘNG MIGRATE DATABASE & SEED ---
            using (var scope = app.Services.CreateScope())
            {
                var config = scope.ServiceProvider.GetRequiredService<IConfiguration>(); // Lấy config
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                // 1. Cấu hình Key mã hóa trước khi Seed (QUAN TRỌNG)
                var encryptionKey = config["EnvironmentVariables:DATA_ENCRYPTION_KEY"];
                if (!string.IsNullOrEmpty(encryptionKey))
                {
                    EncryptionHelper.ConfigureKey(encryptionKey);
                }

                // 2. Migrate Database
                dbContext.Database.Migrate();

                // 3. Chạy Seeder (Thêm dòng này)
                DbSeeder.SeedAsync(dbContext, config).Wait(); 
            }
            // --------------------------------

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Register Exception Middleware here
            app.UseMiddleware<ExceptionMiddleware>();

            app.UseHttpsRedirection();

            // Kích hoạt CORS
            app.UseCors("AllowFrontend");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<WishwallHub>("/hubs/wishwall");

            app.Run();
        }

        private static void LoadEnvFile()
        {
            var baseDir = AppContext.BaseDirectory;
            var current = new DirectoryInfo(baseDir);

            while (current != null)
            {
                var envPath = Path.Combine(current.FullName, ".env");
                if (File.Exists(envPath))
                {
                    foreach (var rawLine in File.ReadAllLines(envPath))
                    {
                        var line = rawLine.Trim();
                        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        {
                            continue;
                        }

                        var separatorIndex = line.IndexOf('=');
                        if (separatorIndex <= 0)
                        {
                            continue;
                        }

                        var key = line[..separatorIndex].Trim();
                        var value = line[(separatorIndex + 1)..].Trim().Trim('"');
                        Environment.SetEnvironmentVariable(key, value);
                    }

                    break;
                }

                current = current.Parent;
            }
        }
    }
}
