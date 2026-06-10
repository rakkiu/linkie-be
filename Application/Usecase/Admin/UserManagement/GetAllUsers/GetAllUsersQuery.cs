using Application.Model.Admin;
using MediatR;

namespace Application.Usecase.Admin.UserManagement.GetAllUsers
{
    public record GetAllUsersQuery(int Page, int PageSize) : IRequest<PaginatedResult<UserListItemDto>>;
}
