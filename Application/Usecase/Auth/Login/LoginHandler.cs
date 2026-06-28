using Application.Interfaces;
using Application.Model.Auth.Login;
using Domain.Entity;
using Domain.Enums;
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
        private readonly IOrganizerRepository _organizerRepo;

        public LoginHandler(IUserRepository repo, IJwtService jwt, IJwtTokenRepository jwtTokenRepo, IOrganizerRepository organizerRepo)
        {
            _repo = repo;
            _jwt = jwt;
            _jwtTokenRepo = jwtTokenRepo;
            _organizerRepo = organizerRepo;
        }

        public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _repo.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng.");

            // === Phân quyền Organizer ===
            Guid? managedEventId = null;
            if (user.Role == UserRole.Organizer)
            {
                if (!user.ManagedEventId.HasValue)
                    throw new UnauthorizedAccessException("Tài khoản Organizer chưa được gán sự kiện.");

                // Kiểm tra event còn hạn không
                var managedEvent = await _organizerRepo.GetEventByIdAsync(user.ManagedEventId.Value, cancellationToken);
                if (managedEvent == null || managedEvent.EndTime < DateTime.UtcNow)
                {
                    // Xóa tài khoản Organizer vĩnh viễn khi event hết hạn
                    await _organizerRepo.DeleteOrganizerAsync(user, cancellationToken);
                    throw new UnauthorizedAccessException("Sự kiện đã kết thúc. Tài khoản Organizer của bạn đã bị thu hồi.");
                }

                managedEventId = user.ManagedEventId;
            }

            // Generate tokens
            string accessToken;
            if (managedEventId.HasValue)
            {
                // Organizer: thêm claim managed_event_id và plan_tier
                string planTier = user.PlanTier.ToString().ToLower();
                accessToken = _jwt.GenerateAccessToken(user.Id, user.Email, user.Name, user.Role.ToString(), managedEventId, planTier);
            }
            else
            {
                accessToken = _jwt.GenerateAccessToken(user.Id, user.Email, user.Name, user.Role.ToString());
            }
            
            var refreshToken = _jwt.GenerateRefreshToken();
            var accessTokenExpirationMinutes = _jwt.GetAccessTokenExpirationMinutes();
            var refreshTokenExpirationDays = _jwt.GetRefreshTokenExpirationDays();

            var accessTokenEntity = new JwtToken
            {
                Token = accessToken,
                TokenType = "AccessToken",
                ExpiresAt = DateTime.SpecifyKind(DateTime.UtcNow.AddMinutes(accessTokenExpirationMinutes), DateTimeKind.Unspecified),
                IsRevoked = false,
                UserId = user.Id
            };
            await _jwtTokenRepo.SaveTokenAsync(accessTokenEntity, cancellationToken);

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

