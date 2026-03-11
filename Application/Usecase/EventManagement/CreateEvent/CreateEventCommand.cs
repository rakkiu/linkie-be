using MediatR;
using Domain.Enums;

namespace Application.Usecase.EventManagement.CreateEvent
{
    public record CreateEventCommand(
        string Name,
        string? Description,
        DateTimeOffset StartTime,
        DateTimeOffset EndTime,
        string? Location,
        int MaxParticipants,
        bool IsWishwallEnabled,
        string? ThumbnailUrl
    ) : IRequest<CreateEventResult>;

    public record CreateEventResult(Guid Id, string Name, string? ThumbnailUrl);
}
