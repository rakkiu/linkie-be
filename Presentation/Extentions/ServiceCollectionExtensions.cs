using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Domain.Interface; // Thêm namespace này
using Infrastructure.Repository; // Thêm namespace này
using Application.Interfaces; // Thêm namespace này
using Infrastructure.Security;
using Domain.Interfaces;
using Infrastructure.Repositories;
using Infrastructure.Identity; // Add this
using Infrastructure.Services;
using Infrastructure.Shared;
using Application.Usecase.Auth.Login;   // Add this
using Presentation.Services;

namespace Presentation.Extentions
{
    /// <summary>
    /// Dependency Injection configuration
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the application services.
        /// </summary>
        public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration config)
        {
            // 🔹 Database
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (environment != "Testing")
            {
                services.AddDbContext<ApplicationDbContext>(opt =>
                    opt.UseNpgsql(config.GetConnectionString("DefaultConnection")));
            }

            // 🔹 Dependency Injection
            // Register repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IJwtTokenRepository, JwtTokenRepository>();
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IWishwallRepository, WishwallRepository>();
            services.AddScoped<IArFrameRepository, ArFrameRepository>();
            services.AddScoped<IAdminRepository, AdminRepository>();

            // Register services
            services.Configure<JwtSettings>(config.GetSection("JwtSettings")); // Configure JwtSettings
            services.AddScoped<IJwtService, JwtService>(); // Register JwtService
            services.AddScoped<IEmailService, EmailService>(); // Register EmailService
            services.AddScoped<IEncryptionService, EncryptionService>(); // Register EncryptionService
            services.AddScoped<ICloudinaryService, CloudinaryService>(); // Register CloudinaryService
            services.AddScoped<IWishwallNotifier, WishwallNotifier>(); // Register WishwallNotifier (SignalR)

            // 🔹 MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(LoginHandler).Assembly));

            // 🔹 JWT Authentication
            var secretKey = config["JwtSettings:SecretKey"];
            if (!string.IsNullOrEmpty(secretKey))
            {
                var key = System.Text.Encoding.UTF8.GetBytes(secretKey);

                services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = config["JwtSettings:Issuer"],
                            ValidAudience = config["JwtSettings:Audience"],
                            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                            ClockSkew = TimeSpan.FromSeconds(30),
                            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
                            NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier
                        };
                        // Allow SignalR to pass JWT via query string (?access_token=...)
                        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                        {
                            OnMessageReceived = ctx =>
                            {
                                var accessToken = ctx.Request.Query["access_token"];
                                var path = ctx.HttpContext.Request.Path;
                                if (!string.IsNullOrEmpty(accessToken) &&
                                    path.StartsWithSegments("/hubs"))
                                {
                                    ctx.Token = accessToken;
                                }
                                return Task.CompletedTask;
                            }
                        };
                    });
            }

            // 🔹 SignalR
            services.AddSignalR();

            // 🔹 Authorization
            services.AddAuthorization();

            return services;
        }
    }
}
