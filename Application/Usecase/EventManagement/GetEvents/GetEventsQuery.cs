using MediatR;

namespace Application.Usecase.EventManagement.GetEvents
{
    public record GetEventsQuery(string? Status) : IRequest<List<EventResponseDto>>;
}
