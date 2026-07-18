using MediatR;

namespace Application.Usecase.Admin.UserManagement.DeleteUser
{
    public record DeleteUserCommand(Guid UserId) : IRequest<bool>;
}
