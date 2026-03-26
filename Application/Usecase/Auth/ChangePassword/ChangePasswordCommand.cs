using MediatR;

namespace Application.Usecase.Auth.ChangePassword
{
    public record ChangePasswordCommand(
         Guid UserId,
         string CurrentPassword,
         string NewPassword
     ) : IRequest<Unit>;

}
