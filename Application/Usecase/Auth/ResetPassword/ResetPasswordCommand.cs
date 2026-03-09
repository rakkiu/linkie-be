using MediatR;

namespace Application.Usecase.Auth.ResetPassword
{
    public record ResetPasswordCommand(string Token, string NewPassword) : IRequest<Unit>;
}
