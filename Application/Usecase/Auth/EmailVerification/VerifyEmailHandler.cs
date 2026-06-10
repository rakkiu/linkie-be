using Domain.Interface;
using Domain.Interfaces;
using MediatR;

namespace Application.Usecase.Auth.EmailVerification
{
    public class VerifyEmailHandler : IRequestHandler<VerifyEmailCommand, Unit>
    {
        private readonly IJwtTokenRepository _jwtTokenRepository;
        private readonly IUserRepository _userRepository;

        public VerifyEmailHandler(
            IJwtTokenRepository jwtTokenRepository,
            IUserRepository userRepository)
        {
            _jwtTokenRepository = jwtTokenRepository;
            _userRepository = userRepository;
        }

        public async Task<Unit> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Token))
                throw new ArgumentException("Verification token is missing.");

            var tokenEntity = await _jwtTokenRepository.GetByTokenAsync(request.Token);
            if (tokenEntity == null || tokenEntity.IsRevoked || tokenEntity.TokenType != "EmailVerification")
                throw new ArgumentException("Invalid or expired verification token.");

            if (tokenEntity.ExpiresAt < DateTime.UtcNow)
            {
                await _jwtTokenRepository.RemoveTokenAsync(tokenEntity, cancellationToken);
                await _jwtTokenRepository.SaveChangeAsync(cancellationToken);
                throw new ArgumentException("Verification token has expired. Please request a new one.");
            }

            var user = await _userRepository.GetByIdWithoutDecryptAsync(tokenEntity.UserId, cancellationToken);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            user.IsEmailVerified = true;

            await _jwtTokenRepository.RemoveTokenAsync(tokenEntity, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
