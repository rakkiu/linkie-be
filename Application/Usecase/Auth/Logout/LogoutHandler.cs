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

        public Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("Logout not yet implemented.");
        }
    }
}
