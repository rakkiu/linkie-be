using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entity;
using Domain.Interface;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class EventRatingRepository : IEventRatingRepository
    {
        private readonly ApplicationDbContext _context;

        public EventRatingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> HasUserRatedEventAsync(Guid userId, Guid eventId, CancellationToken cancellationToken)
        {
            return await _context.EventRatings
                .AnyAsync(r => r.UserId == userId && r.EventId == eventId, cancellationToken);
        }

        public async Task AddRatingAsync(EventRating rating, CancellationToken cancellationToken)
        {
            await _context.EventRatings.AddAsync(rating, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<EventRating>> GetRatingsByEventIdAsync(Guid eventId, CancellationToken cancellationToken)
        {
            return await _context.EventRatings
                .Where(r => r.EventId == eventId)
                .ToListAsync(cancellationToken);
        }
    }
}
