using Domain.Interface;
using Application.Interfaces;
using Application.Model.Admin;
using Application.Model.WishwallAi;
using Domain.Interfaces;
using MediatR;

namespace Application.Usecase.Auth.ResetPassword
{
    public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, Unit>
    {
        private readonly IJwtTokenRepository _jwtTokenRepository;
        private readonly IUserRepository _userRepository;

        public ResetPasswordHandler(IJwtTokenRepository jwtTokenRepository, IUserRepository userRepository)
        {
            _jwtTokenRepository = jwtTokenRepository;
            _userRepository = userRepository;
        }

        public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Token))
                throw new ArgumentException("Reset token is missing.");

            if (string.IsNullOrWhiteSpace(request.NewPassword))
                throw new ArgumentException("New password is missing.");

            var tokenEntity = await _jwtTokenRepository.GetByTokenAsync(request.Token);
            if (tokenEntity == null || tokenEntity.IsRevoked || tokenEntity.TokenType != "ResetPassword")
                throw new ArgumentException("Invalid or expired reset token.");

            if (tokenEntity.ExpiresAt < DateTime.UtcNow)
            {
                await _jwtTokenRepository.RemoveTokenAsync(tokenEntity, cancellationToken);
                await _jwtTokenRepository.SaveChangeAsync(cancellationToken);
                throw new ArgumentException("Reset token has expired. Please request a new one.");
            }

            var user = await _userRepository.GetByIdWithoutDecryptAsync(tokenEntity.UserId, cancellationToken);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            _userRepository.UpdatePasswordOnly(user);

            await _jwtTokenRepository.RemoveTokenAsync(tokenEntity, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
