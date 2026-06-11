using Application.Interfaces;
using Application.Model.Auth.Login;
using Domain.Entity;
using Domain.Interface;
using Application.Model.Admin;
using Application.Model.WishwallAi;
using Domain.Interfaces;
using MediatR;

namespace Application.Usecase.Auth.Login
{
    public class LoginHandler : IRequestHandler<LoginCommand, LoginResponseDto>
    {
        private readonly IUserRepository _repo;
        private readonly IJwtService _jwt;
        private readonly IJwtTokenRepository _jwtTokenRepo;

        public LoginHandler(IUserRepository repo, IJwtService jwt, IJwtTokenRepository jwtTokenRepo)
        {
            _repo = repo;
            _jwt = jwt;
            _jwtTokenRepo = jwtTokenRepo;
        }

        public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _repo.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password.");

            // Generate tokens
            var accessToken = _jwt.GenerateAccessToken(user.Id, user.Email, user.Name, user.Role.ToString());
            var refreshToken = _jwt.GenerateRefreshToken();

            // Get expiration times from settings
            var accessTokenExpirationMinutes = _jwt.GetAccessTokenExpirationMinutes();
            var refreshTokenExpirationDays = _jwt.GetRefreshTokenExpirationDays();

            // Save AccessToken to database
            var accessTokenEntity = new JwtToken
            {
                Token = accessToken,
                TokenType = "AccessToken",
                ExpiresAt = DateTime.SpecifyKind(DateTime.UtcNow.AddMinutes(accessTokenExpirationMinutes), DateTimeKind.Unspecified),
                IsRevoked = false,
                UserId = user.Id
            };
            await _jwtTokenRepo.SaveTokenAsync(accessTokenEntity, cancellationToken);

            // Save RefreshToken to database
            var refreshTokenEntity = new JwtToken
            {
                Token = refreshToken,
                TokenType = "RefreshToken",
                ExpiresAt = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(refreshTokenExpirationDays), DateTimeKind.Unspecified),
                IsRevoked = false,
                UserId = user.Id
            };
            await _jwtTokenRepo.SaveTokenAsync(refreshTokenEntity, cancellationToken);
            await _jwtTokenRepo.SaveChangeAsync(cancellationToken);

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
    }
}
