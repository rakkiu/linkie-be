using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Infrastructure.Identity;
using Domain.Entity;
using System.IO;

namespace TempScript
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var services = new ServiceCollection();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

            var serviceProvider = services.BuildServiceProvider();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            using (var writer = new StreamWriter("D:\\2026_SPR\\EXE\\linkie-be\\db_check.txt"))
            {
                try
                {
                    writer.WriteLine("Starting DB Check...");
                    var events = await context.Events.ToListAsync();
                    writer.WriteLine($"Found {events.Count} events.");
                    foreach (var e in events)
                    {
                        writer.WriteLine($"Event: {e.Id} - {e.Name} - Status: {e.Status}");
                    }
                }
                catch (Exception ex)
                {
                    writer.WriteLine("Error during bulk read:");
                    writer.WriteLine(ex.ToString());

                    writer.WriteLine("\nTrying row by row...");
                    // Try to catch the specific row
                    var conn = context.Database.GetDbConnection();
                    await conn.OpenAsync();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT * FROM \"Events\"";
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                writer.WriteLine($"--- Next Row ---");
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    try
                                    {
                                        var val = reader.GetValue(i);
                                        writer.WriteLine($"{reader.GetName(i)}: {val} ({val.GetType()})");
                                    }
                                    catch (Exception colEx)
                                    {
                                        writer.WriteLine($"{reader.GetName(i)}: ERROR - {colEx.Message}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
