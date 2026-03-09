using MediatR;

namespace Application.Usecase.Auth.ForgotPassword
{
    public record ForgotPasswordCommand(string Email) : IRequest<Unit>;
}
