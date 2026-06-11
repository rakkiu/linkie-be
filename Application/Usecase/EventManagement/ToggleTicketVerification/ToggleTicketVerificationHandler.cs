using Domain.Interface;
using MediatR;

namespace Application.Usecase.EventManagement.ToggleTicketVerification
{
    public class ToggleTicketVerificationHandler
        : IRequestHandler<ToggleTicketVerificationCommand, ToggleTicketVerificationResponseDto>
    {
        private readonly IEventRepository _eventRepository;

        public ToggleTicketVerificationHandler(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<ToggleTicketVerificationResponseDto> Handle(
            ToggleTicketVerificationCommand request, CancellationToken cancellationToken)
        {
            var eventEntity = await _eventRepository.GetByIdAsync(request.EventId, cancellationToken)
                ?? throw new KeyNotFoundException($"Event {request.EventId} not found.");

            eventEntity.RequiresTicket = request.RequiresTicket;

            await _eventRepository.SaveChangesAsync(cancellationToken);

            return new ToggleTicketVerificationResponseDto
            {
                EventId = eventEntity.Id,
                RequiresTicket = eventEntity.RequiresTicket,
                Message = request.RequiresTicket
                    ? "Đã bật xác thực vé cho sự kiện này."
                    : "Đã tắt xác thực vé — mọi người đều có thể tham gia."
            };
        }
    }
}
