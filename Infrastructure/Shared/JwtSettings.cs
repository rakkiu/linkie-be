
namespace Infrastructure.Shared
{
    /// <summary>
    /// 
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// Gets or sets the issuer.
        /// </summary>
        /// <value>
        /// The issuer.
        /// </value>
        public string Issuer { get; set; } = null!;
        /// <summary>
        /// Gets or sets the audience.
        /// </summary>
        /// <value>
        /// The audience.
        /// </value>
        public string Audience { get; set; } = null!;
        /// <summary>
        /// Gets or sets the secret key.
        /// </summary>
        /// <value>
        /// The secret key.
        /// </value>
        public string SecretKey { get; set; } = null!;
        /// <summary>
        /// Gets or sets the access token expires minutes.
        /// </summary>
        /// <value>
        /// The access token expires minutes.
        /// </value>
        public int AccessTokenExpiresMinutes { get; set; } = 15;
        /// <summary>
        /// Gets or sets the refresh token expires days.
        /// </summary>
        /// <value>
        /// The refresh token expires days.
        /// </value>
        public int RefreshTokenExpiresDays { get; set; } = 1;
    }
}
