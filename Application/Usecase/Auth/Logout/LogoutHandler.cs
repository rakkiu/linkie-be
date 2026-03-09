using MediatR;
using Domain.Interface;

namespace Application.Usecase.Auth.Logout
{
    public class LogoutHandler : IRequestHandler<LogoutCommand, bool>
    {
        private readonly IJwtTokenRepository _jwtTokenRepo;

        public LogoutHandler(IJwtTokenRepository jwtTokenRepo)
        {
            _jwtTokenRepo = jwtTokenRepo;
        }

        public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var token = await _jwtTokenRepo.GetRefreshTokenAsync(request.refreshToken, cancellationToken);
            if (token == null)
                return false;

            var userId = token.UserId;

            await _jwtTokenRepo.RemoveTokenAsync(token, cancellationToken);

            var accessToken = await _jwtTokenRepo.GetAccessTokenByUserIdAsync(userId, cancellationToken);
            if (accessToken != null)
                await _jwtTokenRepo.RemoveTokenAsync(accessToken, cancellationToken);

            await _jwtTokenRepo.SaveChangeAsync(cancellationToken);
            return true;
        }
    }
}
