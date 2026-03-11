using Domain.Enums;

namespace Presentation.Common
{
    public record CreateEventRequest(
        string Name,
        string? Description,
        DateTimeOffset StartTime,
        DateTimeOffset EndTime,
        string? Location,
        int MaxParticipants,
        bool IsWishwallEnabled,
        string? ThumbnailUrl
    );

    public record UpdateEventRequest(
        string Name,
        string? Description,
        DateTimeOffset StartTime,
        DateTimeOffset EndTime,
        string? Location,
        int MaxParticipants,
        bool IsWishwallEnabled,
        EventStatus Status,
        string? ThumbnailUrl
    );
}
