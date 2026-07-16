using Domain.Enums;

namespace Application.Usecase.EventManagement.GetAdminEventList
{
    public record AdminEventDto(
        Guid Id,
        string Name,
        string? ThumbnailUrl,
        DateTimeOffset StartTime,
        DateTimeOffset EndTime,
        string? Location,
        int MaxParticipants,
        bool IsWishwallEnabled,
        EventStatus Status,
        DateTimeOffset CreatedAt
    );
}
