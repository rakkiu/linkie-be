using MediatR;
using Microsoft.AspNetCore.Http;
using Domain.Enums;

namespace Application.Usecase.EventManagement.UpdateEvent
{
    public record UpdateEventCommand(
        Guid Id,
        string Name,
        string? Description,
        DateTimeOffset StartTime,
        DateTimeOffset EndTime,
        string? Location,
        int MaxParticipants,
        bool IsWishwallEnabled,
        EventStatus Status,
        IFormFile? Thumbnail
    ) : IRequest<UpdateEventResult>;

    public record UpdateEventResult(Guid Id, string Name, string? ThumbnailUrl);
}
