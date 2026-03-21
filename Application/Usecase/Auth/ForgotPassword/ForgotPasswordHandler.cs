using Application.Interfaces;
using Domain.Entity;
using Domain.Interface;
using Application.Model.Admin;
using Application.Model.WishwallAi;
using Domain.Interfaces;
using MediatR;

namespace Application.Usecase.Auth.ForgotPassword
{
    public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand, Unit>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenRepository _jwtTokenRepository;
        private readonly IEmailService _emailService;

        public ForgotPasswordHandler(
            IUserRepository userRepository,
            IJwtTokenRepository jwtTokenRepository,
            IEmailService emailService)
        {
            _userRepository = userRepository;
            _jwtTokenRepository = jwtTokenRepository;
            _emailService = emailService;
        }

        public async Task<Unit> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null)
                throw new KeyNotFoundException("Email not found in the system.");

            // Generate a secure reset token
            var resetToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("+", "-").Replace("/", "_").TrimEnd('=');

            var tokenEntity = new JwtToken
            {
                Token = resetToken,
                TokenType = "ResetPassword",
                ExpiresAt = DateTime.SpecifyKind(DateTime.UtcNow.AddHours(1), DateTimeKind.Unspecified),
                IsRevoked = false,
                UserId = user.Id
            };

            await _jwtTokenRepository.SaveResetTokenAsync(tokenEntity, cancellationToken);

            var resetLink = $"https://linkie.app/reset-password?token={resetToken}";
            var subject = "Reset your password";
            var body = $"Click the link below to reset your password. This link expires in 1 hour.\n\n{resetLink}";

            await _emailService.SendAsync(user.Email, subject, body);

            return Unit.Value;
        }
    }
}
