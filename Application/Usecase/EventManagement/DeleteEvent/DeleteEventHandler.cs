using Domain.Interface;
using Application.Interfaces;
using Application.Model.Admin;
using Application.Model.WishwallAi;
using MediatR;

namespace Application.Usecase.EventManagement.DeleteEvent
{
    public class DeleteEventHandler : IRequestHandler<DeleteEventCommand>
    {
        private readonly IEventRepository _eventRepository;

        public DeleteEventHandler(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task Handle(DeleteEventCommand request, CancellationToken cancellationToken)
        {
            var @event = await _eventRepository.GetByIdAsync(request.Id, cancellationToken);
            if (@event == null)
            {
                throw new KeyNotFoundException($"Event with ID {request.Id} not found.");
            }

            await _eventRepository.DeleteAsync(@event, cancellationToken);
            await _eventRepository.SaveChangesAsync(cancellationToken);
        }
    }
}
