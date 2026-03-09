using MediatR;

namespace Application.Usecase.Event.GetEvents
{
    public record GetEventsQuery(string? Status) : IRequest<List<EventResponseDto>>;
}
