using System;
using MediatR;

namespace Application.Usecase.EventRating
{
    public class SubmitRatingCommand : IRequest<bool>
    {
        public Guid EventId { get; set; }
        public Guid UserId { get; set; }
        public int StarRating { get; set; }
    }
}
