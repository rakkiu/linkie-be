using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Interface;
using MediatR;

namespace Application.Usecase.EventRating
{
    public class SubmitRatingHandler : IRequestHandler<SubmitRatingCommand, bool>
    {
        private readonly IEventRatingRepository _repository;
        private readonly IEventRepository _eventRepository;

        public SubmitRatingHandler(IEventRatingRepository repository, IEventRepository eventRepository)
        {
            _repository = repository;
            _eventRepository = eventRepository;
        }

        public async Task<bool> Handle(SubmitRatingCommand request, CancellationToken cancellationToken)
        {
            var ev = await _eventRepository.GetByIdAsync(request.EventId, cancellationToken);
            if (ev == null)
            {
                throw new KeyNotFoundException("Event not found.");
            }

            if (request.StarRating < 1 || request.StarRating > 5)
            {
                throw new ArgumentException("Star rating must be between 1 and 5.");
            }

            var hasRated = await _repository.HasUserRatedEventAsync(request.UserId, request.EventId, cancellationToken);
            if (hasRated)
            {
                throw new InvalidOperationException("User has already rated this event.");
            }

            var rating = new Domain.Entity.EventRating
            {
                Id = Guid.NewGuid(),
                EventId = request.EventId,
                UserId = request.UserId,
                StarRating = request.StarRating,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddRatingAsync(rating, cancellationToken);

            return true;
        }
    }
}
