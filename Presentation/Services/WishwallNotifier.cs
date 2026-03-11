using Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Presentation.Hubs;

namespace Presentation.Services
{
    public class WishwallNotifier : IWishwallNotifier
    {
        private readonly IHubContext<WishwallHub> _hub;

        public WishwallNotifier(IHubContext<WishwallHub> hub)
        {
            _hub = hub;
        }

        public Task NotifyNewPendingAsync(Guid eventId, Guid messageId, string message, string sentiment, DateTime createdAt)
            => _hub.Clients.Group($"staff-{eventId}").SendAsync("NewPendingMessage", new
            {
                id = messageId,
                message,
                sentiment,
                createdAt
            });

        public Task NotifyMessageApprovedAsync(Guid eventId, Guid messageId, string userName, string message, string sentiment, DateTime createdAt)
            => _hub.Clients.Group($"event-{eventId}").SendAsync("MessageApproved", new
            {
                id = messageId,
                userName,
                message,
                sentiment,
                createdAt
            });

        public Task NotifyLedDisplayAsync(Guid eventId, Guid messageId, string userName, string message, string sentiment, DateTime createdAt)
            => _hub.Clients.Group($"led-{eventId}").SendAsync("LedDisplay", new
            {
                id = messageId,
                userName,
                message,
                sentiment,
                createdAt
            });

        public Task NotifyUserPendingAsync(string userId, Guid messageId, string message, DateTime createdAt)
            => _hub.Clients.User(userId).SendAsync("MessagePending", new
            {
                id = messageId,
                message,
                createdAt
            });
    }
}
