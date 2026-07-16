using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Interface;
using MediatR;

namespace Application.Usecase.EventRating
{
    public class CheckRatingStatusHandler : IRequestHandler<CheckRatingStatusQuery, bool>
    {
        private readonly IEventRatingRepository _repository;

        public CheckRatingStatusHandler(IEventRatingRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(CheckRatingStatusQuery request, CancellationToken cancellationToken)
        {
            return await _repository.HasUserRatedEventAsync(request.UserId, request.EventId, cancellationToken);
        }
    }
}
