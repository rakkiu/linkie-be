using Application.Usecase.Tickets.CheckUserTicket;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace Presentation.Middlewares
{
    public class TicketVerificationFilter : IAsyncActionFilter
    {
        private readonly IMediator _mediator;

        public TicketVerificationFilter(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var requiresTicket = context.ActionDescriptor
                .EndpointMetadata
                .OfType<RequireTicketAttribute>()
                .Any();

            if (!requiresTicket)
            {
                await next();
                return;
            }

            var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var eventId = context.RouteData.Values["eventId"]?.ToString();

            if (!Guid.TryParse(userIdClaim, out var userId) ||
                !Guid.TryParse(eventId, out var parsedEventId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var query = new CheckUserTicketQuery
            {
                UserId = userId,
                EventId = parsedEventId
            };

            var result = await _mediator.Send(query);

            if (!result.HasValidTicket)
            {
                context.Result = new ForbidResult();
                return;
            }

            await next();
        }
    }
}
