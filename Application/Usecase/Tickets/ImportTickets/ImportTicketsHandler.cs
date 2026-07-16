using Application.Interfaces;
using Domain.Entity;
using Domain.Enums;
using Domain.Interface;
using Domain.Interfaces;
using MediatR;

namespace Application.Usecase.Tickets.ImportTickets
{
    public class ImportTicketsHandler : IRequestHandler<ImportTicketsCommand, ImportTicketsResponse>
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IExcelTicketParser _excelParser;

        public ImportTicketsHandler(
            ITicketRepository ticketRepository,
            IUserRepository userRepository,
            IEventRepository eventRepository,
            IExcelTicketParser excelParser)
        {
            _ticketRepository = ticketRepository;
            _userRepository = userRepository;
            _eventRepository = eventRepository;
            _excelParser = excelParser;
        }

        public async Task<ImportTicketsResponse> Handle(ImportTicketsCommand request, CancellationToken cancellationToken)
        {
            var response = new ImportTicketsResponse
            {
                EventId = request.EventId,
                ImportedAt = DateTime.UtcNow
            };

            var eventEntity = await _eventRepository.GetByIdAsync(request.EventId, cancellationToken);
            if (eventEntity == null)
                throw new KeyNotFoundException("Event not found");

            var parsedRows = await _excelParser.ParseAsync(request.FileStream, cancellationToken);
            response.TotalRecords = parsedRows.Count;

            var ticketsToImport = new List<Ticket>();

            foreach (var row in parsedRows)
            {
                if (!row.IsValid)
                {
                    response.FailedRecords.Add(new FailedRecord
                    {
                        RowNumber = row.RowNumber,
                        Email = row.Email,
                        Reason = string.Join("; ", row.Errors)
                    });
                    continue;
                }

                var existingTicket = await _ticketRepository.GetByCodeAsync(row.TicketCode, request.EventId, cancellationToken);
                if (existingTicket != null)
                {
                    response.FailedRecords.Add(new FailedRecord
                    {
                        RowNumber = row.RowNumber,
                        Email = row.Email,
                        Reason = "Ticket code already exists for this event"
                    });
                    continue;
                }

                var user = await _userRepository.GetByEmailAsync(row.Email, cancellationToken);
                if (user == null)
                {
                    response.FailedRecords.Add(new FailedRecord
                    {
                        RowNumber = row.RowNumber,
                        Email = row.Email,
                        Reason = "User with this email not found"
                    });
                    continue;
                }

                var status = Enum.Parse<TicketStatus>(row.Status);

                var ticket = new Ticket
                {
                    TicketId = Guid.NewGuid(),
                    EventId = request.EventId,
                    TicketCode = row.TicketCode,
                    Email = row.Email,
                    Status = status,
                    UserId = user.Id,
                    AssignedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                ticketsToImport.Add(ticket);
            }

            if (ticketsToImport.Count > 0)
            {
                await _ticketRepository.AddRangeAsync(ticketsToImport, cancellationToken);
                await _ticketRepository.SaveChangesAsync(cancellationToken);
            }

            response.ImportedTickets = ticketsToImport.Count;
            response.Success = response.FailedRecords.Count == 0;

            return response;
        }
    }
}
