using Application.Interfaces;
using Domain.Interface;
using MediatR;
using Application.Model.Auth.Login;

namespace Application.Usecase.Auth.Login
{
    public class LoginHandler : IRequestHandler<LoginCommand, LoginResponseDto>
    {
        private readonly IJwtService _jwt;
        private readonly IJwtTokenRepository _jwtTokenRepo;

        public LoginHandler(IJwtService jwt, IJwtTokenRepository jwtTokenRepo)
        {
            _jwt = jwt;
            _jwtTokenRepo = jwtTokenRepo;
        }

        public Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("Authentication not yet implemented.");
        }
    }
}
