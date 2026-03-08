using Infrastructure.Identity;

namespace Infrastructure.Seed
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            await Task.CompletedTask;
        }
    }
}