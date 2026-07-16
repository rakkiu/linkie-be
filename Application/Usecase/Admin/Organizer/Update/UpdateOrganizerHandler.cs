using Application.Interfaces;
using Domain.Interfaces;
using MediatR;

namespace Application.Usecase.Admin.Organizer.Update
{
    public record UpdateOrganizerCommand(
        Guid OrganizerId,
        string DisplayName,
        Guid ManagedEventId,
        Domain.Enums.PlanTier PlanTier
    ) : IRequest;

    public class UpdateOrganizerHandler : IRequestHandler<UpdateOrganizerCommand>
    {
        private readonly IOrganizerRepository _organizerRepo;
        private readonly IEncryptionService _encryption;

        public UpdateOrganizerHandler(IOrganizerRepository organizerRepo, IEncryptionService encryption)
        {
            _organizerRepo = organizerRepo;
            _encryption = encryption;
        }

        public async Task Handle(UpdateOrganizerCommand request, CancellationToken cancellationToken)
        {
            var organizer = await _organizerRepo.GetOrganizerByIdAsync(request.OrganizerId, cancellationToken)
                ?? throw new KeyNotFoundException($"Tài khoản Organizer với ID {request.OrganizerId} không tồn tại.");

            var managedEvent = await _organizerRepo.GetEventByIdAsync(request.ManagedEventId, cancellationToken)
                ?? throw new KeyNotFoundException($"Sự kiện với ID {request.ManagedEventId} không tồn tại.");

            // Cập nhật tên hiển thị và sự kiện được gán, cũng như Gói
            organizer.Name = _encryption.Encrypt(request.DisplayName);
            organizer.ManagedEventId = request.ManagedEventId;
            organizer.PlanTier = request.PlanTier;

            await _organizerRepo.SaveChangesAsync(cancellationToken);
        }
    }
}
