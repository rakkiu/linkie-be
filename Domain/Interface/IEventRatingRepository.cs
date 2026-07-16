using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entity;

namespace Domain.Interface
{
    public interface IEventRatingRepository
    {
        Task<bool> HasUserRatedEventAsync(Guid userId, Guid eventId, CancellationToken cancellationToken);
        Task AddRatingAsync(EventRating rating, CancellationToken cancellationToken);
        Task<List<EventRating>> GetRatingsByEventIdAsync(Guid eventId, CancellationToken cancellationToken);
    }
}
