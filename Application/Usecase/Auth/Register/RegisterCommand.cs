using MediatR;

namespace Application.Usecase.Auth.Register
{
    public record RegisterCommand(
        string Name,
        string Email,
        string Password
    ) : IRequest<RegisterResponseDto>;
}
