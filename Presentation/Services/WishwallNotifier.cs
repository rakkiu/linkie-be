using Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Presentation.Hubs;

namespace Presentation.Services
{
    public class WishwallNotifier : IWishwallNotifier
    {
        private readonly IHubContext<WishwallHub> _hub;

        public WishwallNotifier(IHubContext<WishwallHub> hub) => _hub = hub;

        public Task NotifyMessagePendingAsync(Guid userId, object payload)
            => _hub.Clients.Group($"user:{userId}").SendAsync("MessagePending", payload);

        public Task BroadcastApprovedMessageAsync(Guid eventId, object payload)
            => _hub.Clients.Group($"event:{eventId}").SendAsync("MessageApproved", payload);

        public Task NotifyStaffNewPendingAsync(Guid eventId, object payload)
            => _hub.Clients.Group($"staff:{eventId}").SendAsync("NewPendingMessage", payload);

        public Task DisplayOnLedAsync(Guid eventId, object payload)
            => _hub.Clients.Group($"led:{eventId}").SendAsync("LedDisplay", payload);
    }
}
