using Domain.Interface;
using Infrastructure.Identity;

namespace Infrastructure.Repository
{
    public class JwtTokenRepository : IJwtTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public JwtTokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task DeleteAsync(string refreshToken)
            => Task.CompletedTask;

        public async Task SaveChangeAsync(CancellationToken ct = default)
            => await _context.SaveChangesAsync(ct);
    }
}
