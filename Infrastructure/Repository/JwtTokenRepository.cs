using Domain.Entity;
using Domain.Interface;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class JwtTokenRepository : IJwtTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public JwtTokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }



        public async Task DeleteAsync(string refreshToken)
        {
            var tokenEntity = await _context.JwtTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (tokenEntity != null)
            {
                _context.JwtTokens.Remove(tokenEntity);
            }
        }

        /// <summary>
        /// Gets the token by token string asynchronous (works for both AccessToken and RefreshToken).
        /// </summary>
        /// <param name="token">The token string.</param>
        /// <returns>JwtToken entity with User information</returns>
        public async Task<JwtToken?> GetByTokenAsync(string token)
        {
            return await _context.JwtTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        /// <summary>
        /// Saves the token asynchronous (works for both AccessToken and RefreshToken).
        /// </summary>
        /// <param name="token">The token entity.</param>
        /// <param name="ct">The cancellation token.</param>
        public async Task SaveTokenAsync(JwtToken token, CancellationToken ct = default)
        {
            // Ensure ExpiresAt is Unspecified kind for PostgreSQL compatibility
            token.ExpiresAt = DateTime.SpecifyKind(token.ExpiresAt, DateTimeKind.Unspecified);

            await _context.JwtTokens.AddAsync(token, ct);
        }

        /// <summary>
        /// Gets the refresh token asynchronous with user information.
        /// </summary>
        /// <param name="token">The refresh token string.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>RefreshToken entity with User information</returns>
        public Task<JwtToken?> GetRefreshTokenAsync(string token, CancellationToken ct = default)
        {
            return _context.JwtTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token && rt.TokenType == "RefreshToken", ct);
        }

        /// <summary>
        /// Removes the token asynchronous (works for both AccessToken and RefreshToken).
        /// </summary>
        /// <param name="token">The token entity to remove.</param>
        /// <param name="ct">The cancellation token.</param>
        public async Task RemoveTokenAsync(JwtToken token, CancellationToken ct = default)
        {
            _context.JwtTokens.Remove(token);
        }

        /// <summary>
        /// Gets the most recent access token by user identifier asynchronous.
        /// Returns null if no access token found for the user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The most recent AccessToken or null</returns>
        public Task<JwtToken?> GetAccessTokenByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            return _context.JwtTokens
                .Include(t => t.User)
                .Where(t => t.UserId == userId && t.TokenType == "AccessToken" && !t.IsRevoked)
                .OrderByDescending(t => t.ExpiresAt) // Get the most recent one
                .FirstOrDefaultAsync(ct);
        }

        /// <summary>
        /// Update (e.g. revoke) a token.
        /// </summary>
        public async Task UpdateAsync(JwtToken token, CancellationToken ct = default)
        {
            _context.JwtTokens.Update(token);
        }



        public void Update(JwtToken token)
        {
            _context.JwtTokens.Update(token);
        }
        public async Task SaveResetTokenAsync(JwtToken token, CancellationToken ct = default)
        {
            // Ensure ExpiresAt is Unspecified kind for PostgreSQL compatibility
            token.ExpiresAt = DateTime.SpecifyKind(token.ExpiresAt, DateTimeKind.Unspecified);

            await _context.JwtTokens.AddAsync(token, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task SaveChangeAsync(CancellationToken ct = default)
        {
            await _context.SaveChangesAsync(ct);
        }

    }
}
