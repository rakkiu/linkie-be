using Application.Model.Auth.Login;
using MediatR;

namespace Application.Usecase.Auth.GoogleLogin
{
    public class GoogleLoginCommand : IRequest<LoginResponseDto>
    {
        public string IdToken { get; set; } = string.Empty;
    }
}
