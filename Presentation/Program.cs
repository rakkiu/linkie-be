using Infrastructure.Identity;
using Infrastructure.Security;
using Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Presentation.Extentions;
using Presentation.Extentions;
using Presentation.Middlewares;

namespace Presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Fix lỗi DateTime Unspecified của PostgreSQL
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAppServices(builder.Configuration);

            // Thêm CORS service
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    b => b.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
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
            app.UseCors("AllowAll");

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
