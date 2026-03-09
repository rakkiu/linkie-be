using MediatR;

namespace Application.Usecase.Admin.Dashboard
{
    public record GetEventDashboardQuery(Guid EventId) : IRequest<EventDashboardDto>;
}
