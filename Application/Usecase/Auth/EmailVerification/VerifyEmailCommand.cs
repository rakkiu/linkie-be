using MediatR;

namespace Application.Usecase.Auth.EmailVerification
{
    public record VerifyEmailCommand(string Token) : IRequest<Unit>;
}
