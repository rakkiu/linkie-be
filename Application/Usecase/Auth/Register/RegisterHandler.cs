using Application.Interfaces;
using Domain.Entity;
using Domain.Enums;
using Domain.Interface;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Usecase.Auth.Register
{
    public class RegisterHandler : IRequestHandler<RegisterCommand, RegisterResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenRepository _jwtTokenRepository;
        private readonly IEncryptionService _encryption;
        private readonly IEmailService _emailService;
        private readonly ILogger<RegisterHandler> _logger;

        public RegisterHandler(
            IUserRepository userRepository,
            IJwtTokenRepository jwtTokenRepository,
            IEncryptionService encryption,
            IEmailService emailService,
            ILogger<RegisterHandler> logger)
        {
            _userRepository = userRepository;
            _jwtTokenRepository = jwtTokenRepository;
            _encryption = encryption;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<RegisterResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Name is required.");

            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Email is required.");

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
                throw new ArgumentException("Password must be at least 6 characters.");

            var existing = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (existing != null)
                throw new InvalidOperationException("Email is already registered.");

            var user = new User
            {
                Name = _encryption.Encrypt(request.Name),
                Email = _encryption.EncryptDeterministic(request.Email),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = UserRole.Attendee,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            try
            {
                var verificationToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                    .Replace("+", "-").Replace("/", "_").TrimEnd('=');

                var tokenEntity = new JwtToken
                {
                    Token = verificationToken,
                    TokenType = "EmailVerification",
                    ExpiresAt = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(7), DateTimeKind.Unspecified),
                    IsRevoked = false,
                    UserId = user.Id
                };

                await _jwtTokenRepository.SaveResetTokenAsync(tokenEntity, cancellationToken);

                var verifyLink = $"https://linkie.app/verify-email?token={verificationToken}";
                var subject = "Verify your email address";
                var body = $"Please verify your email by clicking the link below. This link expires in 7 days.\n\n{verifyLink}";

                await _emailService.SendAsync(request.Email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send verification email for user {UserId}", user.Id);
            }

            return new RegisterResponseDto
            {
                Id = user.Id,
                Name = request.Name,
                Email = request.Email,
                Role = user.Role.ToString()
            };
        }
    }
}
