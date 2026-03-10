using MediatR;

namespace Application.Usecase.EventManagement.DeleteEvent
{
    public record DeleteEventCommand(Guid Id) : IRequest;
}
