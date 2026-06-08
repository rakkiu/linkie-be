using Application.Interfaces;
using MediatR;

namespace Application.Usecase.Admin.UserManagement.DeleteUser
{
    public class DeleteUserHandler : IRequestHandler<DeleteUserCommand>
    {
        private readonly IAdminRepository _repo;

        public DeleteUserHandler(IAdminRepository repo) => _repo = repo;

        public async Task Handle(DeleteUserCommand request, CancellationToken ct)
            => await _repo.DeleteUserAsync(request.UserId, ct);
    }
}
