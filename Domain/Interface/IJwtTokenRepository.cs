
using Domain.Entity;

namespace Domain.Interface
{
    public interface IJwtTokenRepository
    {
        Task DeleteAsync(string refreshToken);
        Task SaveChangeAsync(CancellationToken ct = default);
        Task<JwtToken?> GetByTokenAsync(string token);
        /// <summary>
        /// Deletes the asynchronous.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <returns></returns>
       
        /// <summary>
        /// Saves the refresh token asynchronous.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task SaveTokenAsync(JwtToken token, CancellationToken ct = default);
        /// <summary>
        /// Gets the refresh token asynchronous.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<JwtToken?> GetRefreshTokenAsync(string token, CancellationToken ct = default);
        /// <summary>
        /// Removes the refresh token asynchronous.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task RemoveTokenAsync(JwtToken token, CancellationToken ct = default);
        Task<JwtToken?> GetAccessTokenByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task UpdateAsync(JwtToken refreshToken, CancellationToken cancellationToken);
   

        /// <summary>
        /// Update token without saving (for batch operations).
        /// </summary>
        void Update(JwtToken token);
        /// <summary>Saves the reset token asynchronous.</summary>
        /// <param name="token">The token.</param>
        /// <param name="ct">The ct.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        Task SaveResetTokenAsync(JwtToken token, CancellationToken ct = default);

    }
}
