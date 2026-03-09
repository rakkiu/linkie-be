using Application.Interfaces;
using Application.Model.Auth.Login;
using Domain.Entity;
using Domain.Interface;
using Domain.Interfaces;
using MediatR;

namespace Application.Usecase.Auth.Login
{
    public class LoginHandler : IRequestHandler<LoginCommand, LoginResponseDto>
    {
        private readonly IUserRepository _repo;
        private readonly IJwtService _jwt;
        private readonly IJwtTokenRepository _jwtTokenRepo;

        public LoginHandler(IJwtService jwt, IJwtTokenRepository jwtTokenRepo)
        {
            _jwt = jwt;
            _jwtTokenRepo = jwtTokenRepo;
        }

        public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _repo.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password.");

            // Generate tokens
            var accessToken = _jwt.GenerateAccessToken(user.Id, user.Email, user.Name);
            var refreshToken = _jwt.GenerateRefreshToken();

            // Get expiration times from settings
            var accessTokenExpirationMinutes = _jwt.GetAccessTokenExpirationMinutes();
            var refreshTokenExpirationDays = _jwt.GetRefreshTokenExpirationDays();

            // Save AccessToken to database
            var accessTokenEntity = new JwtToken
            {
                Token = accessToken,
                TokenType = "AccessToken",
                ExpiresAt = DateTime.UtcNow.AddMinutes(accessTokenExpirationMinutes),
                IsRevoked = false,
                UserId = user.Id
            };
            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
    }
}
