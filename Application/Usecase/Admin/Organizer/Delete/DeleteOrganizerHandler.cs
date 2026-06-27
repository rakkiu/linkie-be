using Domain.Interfaces;
using MediatR;

namespace Application.Usecase.Admin.Organizer.Delete
{
    public record DeleteOrganizerCommand(Guid OrganizerId) : IRequest;

    public class DeleteOrganizerHandler : IRequestHandler<DeleteOrganizerCommand>
    {
        private readonly IOrganizerRepository _organizerRepo;

        public DeleteOrganizerHandler(IOrganizerRepository organizerRepo)
        {
            _organizerRepo = organizerRepo;
        }

        public async Task Handle(DeleteOrganizerCommand request, CancellationToken cancellationToken)
        {
            var organizer = await _organizerRepo.GetOrganizerByIdAsync(request.OrganizerId, cancellationToken)
                ?? throw new KeyNotFoundException($"Tài khoản Organizer với ID {request.OrganizerId} không tồn tại.");

            await _organizerRepo.DeleteOrganizerAsync(organizer, cancellationToken);
        }
    }
}
