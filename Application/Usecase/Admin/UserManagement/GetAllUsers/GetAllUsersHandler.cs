using Application.Interfaces;
using Application.Model.Admin;
using MediatR;

namespace Application.Usecase.Admin.UserManagement.GetAllUsers
{
    public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, PaginatedResult<UserListItemDto>>
    {
        private readonly IAdminRepository _repo;

        public GetAllUsersHandler(IAdminRepository repo) => _repo = repo;

        public async Task<PaginatedResult<UserListItemDto>> Handle(GetAllUsersQuery request, CancellationToken ct)
            => await _repo.GetAllUsersAsync(request.Page, request.PageSize, ct);
    }
}
