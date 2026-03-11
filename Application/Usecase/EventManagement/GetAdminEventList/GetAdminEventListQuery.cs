using MediatR;

namespace Application.Usecase.EventManagement.GetAdminEventList
{
    public record GetAdminEventListQuery : IRequest<List<AdminEventDto>>;
}
