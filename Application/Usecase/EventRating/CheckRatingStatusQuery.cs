using System;
using MediatR;

namespace Application.Usecase.EventRating
{
    public class CheckRatingStatusQuery : IRequest<bool>
    {
        public Guid EventId { get; set; }
        public Guid UserId { get; set; }
    }
}
