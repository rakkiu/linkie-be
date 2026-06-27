using Application.Interfaces;
using Domain.Interfaces;
using MediatR;

namespace Application.Usecase.Admin.Organizer.GetAll
{
    public record GetOrganizersQuery : IRequest<List<OrganizerListItemDto>>;

    public record OrganizerListItemDto(
        Guid Id,
        string Username,
        string Email,
        string DisplayName,
        Guid? ManagedEventId,
        string? ManagedEventName,
        DateTime? EventEndTime,
        bool IsExpired,
        string PlanTier
    );

    public class GetOrganizersHandler : IRequestHandler<GetOrganizersQuery, List<OrganizerListItemDto>>
    {
        private readonly IOrganizerRepository _organizerRepo;
        private readonly IEncryptionService _encryption;

        public GetOrganizersHandler(IOrganizerRepository organizerRepo, IEncryptionService encryption)
        {
            _organizerRepo = organizerRepo;
            _encryption = encryption;
        }

        public async Task<List<OrganizerListItemDto>> Handle(GetOrganizersQuery request, CancellationToken cancellationToken)
        {
            var organizers = await _organizerRepo.GetAllOrganizersAsync(cancellationToken);

            return organizers.Select(o =>
            {
                var plainEmail = _encryption.DecryptDeterministic(o.Email);
                var plainName = _encryption.Decrypt(o.Name);
                var isExpired = o.ManagedEvent?.EndTime < DateTime.UtcNow;

                return new OrganizerListItemDto(
                    o.Id,
                    o.Username ?? string.Empty,
                    plainEmail,
                    plainName,
                    o.ManagedEventId,
                    o.ManagedEvent?.Name,
                    o.ManagedEvent?.EndTime,
                    isExpired,
                    o.PlanTier.ToString()
                );
            }).ToList();
        }
    }
}
